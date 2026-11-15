namespace Test_Taste_Console_Application.Constants
{
    public static class ExceptionMessage
    {
        public const string FileOutputOperationFailed = "The file output operation failed with the following error: ";

        public const string ScreenOutputOperationFailed =
            "The screen output operation failed with the following error: ";

        public const string MissingApiKey =
            "No API Key found, please set the SOLAR SYSTEM API Key ";

        public const string InvalidApiKey =
            "The API key was rejected by the Solar System API. Please check the SOLAR_SYSTEM_API_KEY environment variable.";
    }
}
