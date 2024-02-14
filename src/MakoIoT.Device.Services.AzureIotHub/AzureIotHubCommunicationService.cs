using MakoIoT.Device.Services.AzureIotHub.Configuration;
using MakoIoT.Device.Services.Interface;
using nanoFramework.Azure.Devices.Client;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MakoIoT.Device.Services.AzureIotHub
{
    public sealed class AzureIotHubCommunicationService : ICommunicationService
    {
        private readonly INetworkProvider _networkProvider;
        private readonly ILog _logger;
        private readonly IConfigurationService _configService;

        private DeviceClient _client;
        public bool CanSend => _client != null && _client.IsConnected && _networkProvider.IsConnected;

        public string ClientAddress => _networkProvider.ClientAddress;

        public string ClientName { get; set; }

        public event EventHandler MessageReceived;

        public AzureIotHubCommunicationService(IConfigurationService configService, ILog logger, INetworkProvider networkProvider)
        {
            _logger = logger;
            _configService = configService;
            _networkProvider = networkProvider;
        }

        public void Connect(string[] subscriptions)
        {
            if (_client == null)
            {
                var cert = (AzureIotHubCertConfig)_configService.GetConfigSection(AzureIotHubCertConfig.SectionName, typeof(AzureIotHubCertConfig)); ;
                var certificate = new X509Certificate(cert.AzureRootCa);
                var config = (AzureIotHubConfig)_configService.GetConfigSection(AzureIotHubConfig.SectionName, typeof(AzureIotHubConfig));
                ClientName = config.DeviceFriendlyName ?? config.DeviceId;
                _client = new DeviceClient(config.Host, config.DeviceId, config.SasKey, azureCert: certificate);
                _client.CloudToDeviceMessage += _client_CloudToDeviceMessage;
            }

            OpenMqttClient();
        }

        private void OpenMqttClient(byte attempt = 0)
        {
            if (_client.IsConnected)
            {
                _logger.Information($"AzureIoT client connected");
                return;
            }

            try
            {
                var mqttConnectResult = _client.Open();
                if (!_client.IsConnected)
                {
                    var message = $"Could not connect to AzureIoT. Broker returned {mqttConnectResult}";
                    _logger.Error(message);
                    throw new Exception(message);
                }
            }
            catch(Exception ex)
            {
                if (attempt <= 3)
                {
                    _logger.Warning($"Unable to open AzureIoT connection. Rety: {++attempt}");
                    Thread.Sleep(attempt * 1000);
                    OpenMqttClient(attempt);
                }

                _logger.Error(ex);
                throw;
            }
        }

        private void _client_CloudToDeviceMessage(object sender, CloudToDeviceMessageEventArgs e)
        {
            _logger.Trace("Received message from topic AzureIoT hub");
            _logger.Trace(e.Message);
            MessageReceived?.Invoke(this, new ObjectEventArgs(e.Message));
        }

        public void Disconnect()
        {
            _client?.Close();
        }

        public void Publish(string messageString, string messageType)
        {
            _logger.Information($"AzureIoT publishing message");
            var isReceived = _client.SendMessage(messageString, null, new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token);
            if (!isReceived)
            {
                _logger.Error($"Unable to send message. {messageString}");
            }
        }

        public void Send(string messageString, string recipient)
        {
            throw new NotImplementedException();
        }
    }
}
