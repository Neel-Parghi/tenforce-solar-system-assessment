using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(HttpClientSettings.ApiKeyEnvironmentVariable)))
            {
                Console.WriteLine(ExceptionMessage.MissingApiKey);
                Environment.ExitCode = 1;
                return;
            }

            ConsoleWriter.WriteStep(OutputString.Starting);

            var serviceCollection = new ServiceCollection();
            //The ConfigureServices function configures the services.
            ConfigureServices(serviceCollection);
            
            //The RunServiceOperations function executes the code that can create the outputs.
            await RunServiceOperationsAsync(serviceCollection);
        }

        private static async Task RunServiceOperationsAsync(IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //The service provider gets the services.
            var screenOutputService = serviceProvider.GetService<IOutputService>();

            var outputOperations = new Func<Task>[]
            {
                screenOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToConsoleAsync,
                screenOutputService.OutputAllMoonsAndTheirMassToConsoleAsync,
                screenOutputService.OutputAllPlanetsAndTheirMoonsToConsoleAsync
            };

            foreach (var outputOperation in outputOperations)
            {
                try
                {
                    await outputOperation();
                }
                catch (UnauthorizedAccessException exception)
                {
                    Logger.Instance.Error(exception.Message);
                    Console.WriteLine(exception.Message);
                    Environment.ExitCode = 1;
                    break;
                }
                catch (Exception exception)
                {
                    //The users and developers can see the thrown exceptions.
                    Logger.Instance.Error($"{LoggerMessage.ScreenOutputOperationFailed}{exception.Message}");
                    Console.WriteLine($"{ExceptionMessage.ScreenOutputOperationFailed}{exception.Message}");
                    System.Diagnostics.Debug.WriteLine($"{ExceptionMessage.ScreenOutputOperationFailed}{exception.Message}");
                    Environment.ExitCode = 1;
                }
            }

            serviceProvider.Dispose();
            ConsoleWriter.WriteStep(OutputString.Done);
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            //The function configures all the services.
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo(Path.Combine(AppContext.BaseDirectory, ConfigurationFileName.Logger)));
            serviceCollection.AddHttpClient<HttpClientService>();
            serviceCollection.AddSingleton<IPlanetService, PlanetService>();
            serviceCollection.AddSingleton<IOutputService, ScreenOutputService>();
            serviceCollection.AddSingleton<IMoonService, MoonService>();
        }
    }
}
