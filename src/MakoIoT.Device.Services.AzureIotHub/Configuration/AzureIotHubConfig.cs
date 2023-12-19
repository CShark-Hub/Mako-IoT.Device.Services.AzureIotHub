using System;

namespace MakoIoT.Device.Services.AzureIotHub.Configuration
{
    public sealed class AzureIotHubConfig
    {
        public static string SectionName => "AzureIotHub";

        public string AzureRootCa { get; set; }

        /// <summary>
        /// Device id used to connect to AzureIoT.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Device name that will be visible in message.
        /// </summary>
        public string DeviceFriendlyName { get; set; }

        public string Host { get; set; }

        public string SasKey { get; set; }


    }
}
