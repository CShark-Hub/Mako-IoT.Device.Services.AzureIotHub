using MakoIoT.Device.Services.Interface;
using nanoFramework.DependencyInjection;

namespace MakoIoT.Device.Services.AzureIotHub.Extensions
{

    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddAzureIotHub(this IDeviceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ICommunicationService), typeof(AzureIotHubCommunicationService));
            return builder;
        }
    }
}
