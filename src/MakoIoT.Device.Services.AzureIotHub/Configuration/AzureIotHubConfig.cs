using System;

namespace MakoIoT.Device.Services.AzureIotHub.Configuration
{
    public sealed class AzureIotHubConfig
    {
        public string AzureRootCa { get; set; }

        public string DeviceId { get; set; }

        public string Host { get; set; }

        public string SasKey { get; set; }

        public static string SectionName => "AzureIotHub";

    }
}
