# TenForce Test and Taste - Solar System

Console application that reads data from the [Solar System OpenData API](https://api.le-systeme-solaire.net/en/) and shows:

- all planets with at least one moon and the average temperature of their moons
- all moons and their mass
- all planets and their moons

## Requirements

- .NET 10 SDK
- A free API key for the Solar System OpenData API

## API key

Since version 2.0.0 the API rejects requests without a key (HTTP 401). The key is not stored in the code or in the repository. The application reads it from the environment variable `SOLAR_SYSTEM_API_KEY` and sends it as `Authorization: Bearer <key>`.

1. Get a key at https://api.le-systeme-solaire.net/generatekey.html
2. Set the environment variable:

PowerShell (current session only):
```powershell
$env:SOLAR_SYSTEM_API_KEY = "your-key"
```

Windows (permanent, open a new terminal afterwards):
```
setx SOLAR_SYSTEM_API_KEY "your-key"
```

macOS / Linux:
```bash
export SOLAR_SYSTEM_API_KEY="your-key"
```

If the variable is missing the application stops with a message. If the key is wrong, it stops after the first request and says the key was rejected.

## Run

From the repository root:

```
dotnet run --project test-and-taste-developer-exercise-main/Test-Taste-Console-Application
```

Example of the temperature output:

```
--------------------+------------------------------
Planet's Id         |Avg. Moon Temperature (K)
--------------------+------------------------------
Uranus              |unknown
Neptune             |unknown
Jupiter             |unknown
Mars                |unknown
Saturne             |unknown
Terre               |unknown
--------------------+------------------------------
```

## Average moon temperature

The API has an `avgTemp` field (in Kelvin) for every body. When I checked the data, all 336 moons return `avgTemp = 0`. Only the planets and a few dwarf planets have a real value.

0 K is absolute zero, so I treat 0 as "no data" and not as a real temperature. Averaging those zeros would give 0 K for every planet, which is wrong. So:

- a moon without a known temperature is left out of the average
- if none of the moons of a planet has a known temperature, the output shows `unknown`

With the current data every planet shows `unknown`. As soon as the API gets real moon temperatures, the averages are shown without any code change.

## Changes to the domain model

- `Moon.AverageTemperature` (`float?`): new. It is `null` when the API sends 0, so "unknown" can't be mixed up with a real value.
- `Planet.AverageMoonTemperature` (`double?`): replaces `AverageMoonGravity`, which was a placeholder that always returned 0. The assignment describes this method as the starting point for the temperature. It is calculated from the moons and has no setter, so it can't get out of sync with them.
- The output method was renamed from `...AverageMoonGravity...` to `...AverageMoonTemperature...`, and it now only lists planets that have moons, as the assignment asks.

## Changes to the API interface

- `avgTemp` is added to the `data` fields of the moons request.
- The planets request asked for `semiMajorAxis`, but the API field is `semimajorAxis`. The API silently ignores unknown fields, so the semi-major axis was always 0. Fixed.
- The API key is added as a Bearer token on the shared `HttpClient`.
- All requests go through `HttpClientService.GetAsync`. That is the one place that handles a rejected key (401/403).
- `IPlanetService`, `IMoonService` and `IOutputService` are now async (`Task<...>`, `...Async`). Before, every call blocked with `.Result`, which also wraps errors in an `AggregateException`.

## Error handling and feedback

- Progress messages for every step ("Loading planets and their moons...", "Writing moons and their mass...", "Done.").
- Missing API key: clear message before anything else runs.
- Rejected API key: clear message, the remaining outputs are skipped because they would fail too.
- Network errors: the real error message is shown, and the other outputs still run.
- A moon request that fails is logged and skipped, so one bad moon doesn't break the whole list.
- Null checks for missing `bodies` in the response and for moons without a `rel` link.
- Exit code 1 when something went wrong.

## Other fixes

- Upgraded from .NET 5 (out of support, not in current SDKs) to .NET 10 (LTS). log4net is updated from 2.0.12 to 3.6.0 because of a known vulnerability; the other packages are updated to match.
- Fixed a compile error in `Program.cs` (doubled quotes).
- `log4net.config` was loaded relative to the current folder, so logging silently stopped when the app was started from another folder. It is now loaded from the application folder.
- The temperature output printed "No moons found." when no planets were found.

## Known limitations and next steps

- Slow: the planets list only contains links to the moons, so every moon is requested on its own (around 200 requests), and the planets are loaded twice. A full run takes around 3 minutes. Next steps would be caching the planets in the service and loading the moons in one request (the moons list can return `avgTemp` and `mass` directly) or in parallel.
- No unit tests yet. `Planet.AverageMoonTemperature` would be the first candidate.
- The key can only be set with an environment variable. .NET user secrets could be added for local development.
