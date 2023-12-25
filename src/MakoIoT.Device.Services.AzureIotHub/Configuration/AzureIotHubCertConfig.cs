using System;

namespace MakoIoT.Device.Services.AzureIotHub.Configuration
{
    public sealed class AzureIotHubCertConfig
    {
        public static string SectionName => "AzureIotHubCert";

        public string AzureRootCa { get; set; }
    }
}
