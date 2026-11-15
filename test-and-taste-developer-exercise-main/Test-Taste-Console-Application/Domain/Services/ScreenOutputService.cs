using System;
using System.Linq;
using System.Threading.Tasks;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class ScreenOutputService : IOutputService
    {
        private readonly IPlanetService _planetService;

        private readonly IMoonService _moonService;

        public ScreenOutputService(IPlanetService planetService, IMoonService moonService)
        {
            _planetService = planetService;
            _moonService = moonService;
        }

        public async Task OutputAllPlanetsAndTheirMoonsToConsoleAsync()
        {
            //The service gets all the planets from the API.
            ConsoleWriter.WriteStep(OutputString.LoadingPlanets);
            var planets = (await _planetService.GetAllPlanetsAsync()).ToArray();

            //If the planets aren't found, then the function stops and tells that to the user via the console.
            if (!planets.Any())
            {
                Console.WriteLine(OutputString.NoPlanetsFound);
                return;
            }

            //The column sizes and labels for the planets are configured here. 
            var columnSizesForPlanets = new[] { 20, 20, 30, 20 };
            var columnLabelsForPlanets = new[]
            {
                OutputString.PlanetNumber, OutputString.PlanetId, OutputString.PlanetSemiMajorAxis,
                OutputString.TotalMoons
            };

            //The column sizes and labels for the moons are configured here.
            //The second moon's column needs the 2 extra '-' characters so that it's aligned with the planet's column.
            var columnSizesForMoons = new[] { 20, 70 + 2 };
            var columnLabelsForMoons = new[]
            {
                OutputString.MoonNumber, OutputString.MoonId
            };

            ConsoleWriter.WriteStep(OutputString.WritingPlanetsAndMoons);

            //The for loop creates the correct output.
            for (int i = 0, j = 1; i < planets.Length; i++, j++)
            {
                //First the line is created.
                ConsoleWriter.CreateLine(columnSizesForPlanets);

                //Under the line the header is created.
                ConsoleWriter.CreateText(columnLabelsForPlanets, columnSizesForPlanets);

                //Under the header the planet data is shown.
                ConsoleWriter.CreateText(
                    new[]
                    {
                        j.ToString(), CultureInfoUtility.TextInfo.ToTitleCase(planets[i].Id),
                        planets[i].SemiMajorAxis.ToString(),
                        planets[i].Moons.Count.ToString()
                    },
                    columnSizesForPlanets);

                //Under the planet data the header for the moons is created.
                ConsoleWriter.CreateLine(columnSizesForPlanets);
                ConsoleWriter.CreateText(columnLabelsForMoons, columnSizesForMoons);

                //The for loop creates the correct output.
                for (int k = 0, l = 1; k < planets[i].Moons.Count; k++, l++)
                {
                    ConsoleWriter.CreateText(
                        new[]
                        {
                            l.ToString(), CultureInfoUtility.TextInfo.ToTitleCase(planets[i].Moons.ElementAt(k).Id)
                        },
                        columnSizesForMoons);
                }

                //Under the data the footer is created.
                ConsoleWriter.CreateLine(columnSizesForMoons);
                ConsoleWriter.CreateEmptyLines(2);

                /*
                    This is an example of the output for the planet Earth:
                    --------------------+--------------------+------------------------------+--------------------
                    Planet's Number     |Planet's Id         |Planet's Semi-Major Axis      |Total Moons
                    10                  |Terre               |0                             |1
                    --------------------+--------------------+------------------------------+--------------------
                    Moon's Number       |Moon's Id
                    1                   |La Lune
                    --------------------+------------------------------------------------------------------------
                */
            }
        }

        public async Task OutputAllMoonsAndTheirMassToConsoleAsync()
        {
            //The function works the same way as the PrintAllPlanetsAndTheirMoonsToConsole function. You can find more comments there.
            ConsoleWriter.WriteStep(OutputString.LoadingMoons);
            var moons = (await _moonService.GetAllMoonsAsync()).ToArray();
            
            if (!moons.Any())
            {
                Console.WriteLine(OutputString.NoMoonsFound);
                return;
            }

            var columnSizesForMoons = new[] { 20, 20, 30, 20 };
            var columnLabelsForMoons = new[]
            {
                OutputString.MoonNumber, OutputString.MoonId, OutputString.MoonMassExponent, OutputString.MoonMassValue
            };

            ConsoleWriter.WriteStep(OutputString.WritingMoonsAndMass);
            ConsoleWriter.CreateHeader(columnLabelsForMoons, columnSizesForMoons);

            for (int i = 0, j = 1; i < moons.Length; i++, j++)
            {
                ConsoleWriter.CreateText(
                    new[]
                    {
                        j.ToString(), CultureInfoUtility.TextInfo.ToTitleCase(moons[i].Id),
                        moons[i].MassExponent.ToString(), moons[i].MassValue.ToString()
                    },
                    columnSizesForMoons);
            }

            ConsoleWriter.CreateLine(columnSizesForMoons);
            ConsoleWriter.CreateEmptyLines(2);
            
            /*
                This is an example of the output for the moon around the earth:
                --------------------+--------------------+------------------------------+--------------------
                Moon's Number       |Moon's Id           |Moon's Mass Exponent          |Moon's Mass Value
                --------------------+--------------------+------------------------------+--------------------
                1                   |Lune                |22                            |7,346             
                ...more data...
                --------------------+--------------------+------------------------------+--------------------
             */
        }

        public async Task OutputAllPlanetsAndTheirAverageMoonTemperatureToConsoleAsync()
        {
            //The function works the same way as the PrintAllPlanetsAndTheirMoonsToConsole function. You can find more comments there.
            ConsoleWriter.WriteStep(OutputString.LoadingPlanets);
            var planets = (await _planetService.GetAllPlanetsAsync()).ToArray();
            if (!planets.Any())
            {
                Console.WriteLine(OutputString.NoPlanetsFound);
                return;
            }

            var columnSizes = new[] { 20, 30 };
            var columnLabels = new[]
            {
                OutputString.PlanetId, OutputString.PlanetMoonAverageTemperature
            };

            ConsoleWriter.WriteStep(OutputString.WritingAverageMoonTemperature);
            ConsoleWriter.CreateHeader(columnLabels, columnSizes);

            foreach (Planet planet in planets.Where(planet => planet.HasMoons()))
            {
                var averageTemperature = planet.AverageMoonTemperature?.ToString("0.0")
                                         ?? OutputString.TemperatureUnknown;

                ConsoleWriter.CreateText(
                    new[] { CultureInfoUtility.TextInfo.ToTitleCase(planet.Id), averageTemperature },
                    columnSizes);
            }

            ConsoleWriter.CreateLine(columnSizes);
            ConsoleWriter.CreateEmptyLines(2);

            /*
                --------------------+------------------------------
                Planet's Id         |Avg. Moon Temperature (K)
                --------------------+------------------------------
                Terre               |unknown
                --------------------+------------------------------
            */
        }
    }
}
