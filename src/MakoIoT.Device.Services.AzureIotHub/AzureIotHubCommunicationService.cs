﻿using MakoIoT.Device.Services.AzureIotHub.Configuration;
using MakoIoT.Device.Services.Interface;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace MakoIoT.Device.Services.AzureIotHub
{
    internal sealed class AzureIotHubCommunicationService : ICommunicationService
    {
        private readonly INetworkProvider _networkProvider;
        private readonly ILog _logger;
        private readonly AzureIotHubConfig _config;
        private readonly X509Certificate _certificate;

        private DeviceClient _client;
        public bool CanSend => _client != null && _client.IsConnected && _networkProvider.IsConnected;

        public string ClientAddress => _networkProvider.ClientAddress;

        public string ClientName => _config.DeviceFriendlyName ?? _config.DeviceId;

        public event EventHandler MessageReceived;

        public AzureIotHubCommunicationService(INetworkProvider networkProvider, IConfigurationService configService, ILog logger)
        {
            _networkProvider = networkProvider;
            _logger = logger;
            _config = (AzureIotHubConfig)configService.GetConfigSection(AzureIotHubConfig.SectionName, typeof(AzureIotHubConfig));
            var cert = (AzureIotHubCertConfig)configService.GetConfigSection(AzureIotHubCertConfig.SectionName, typeof(AzureIotHubCertConfig)); ;
            _certificate = new X509Certificate(cert.AzureRootCa);
        }

        public void Connect(string[] subscriptions)
        {
            if (!_networkProvider.IsConnected)
            {
                _networkProvider.Connect();
                if (!_networkProvider.IsConnected)
                {
                    _logger.Error("Could not connect to network.");
                    return;
                }
            }

            if (_client == null)
            {
                _client = new DeviceClient(_config.Host, _config.DeviceId, _config.SasKey, azureCert: _certificate);
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
                    _logger.Error($"Could not connect to AzureIoT. Broker returned {mqttConnectResult}");
                    return;
                }
            }
            catch(Exception)
            {
                if (attempt <= 3)
                {
                    _logger.Warning($"Unable to open AzureIoT connection. Rety: {++attempt}");
                    Thread.Sleep(attempt * 1000);
                    OpenMqttClient(attempt);
                }

                throw;
            }
        }

        private void _client_CloudToDeviceMessage(object sender, CloudToDeviceMessageEventArgs e)
        {
            _logger.Trace($"Received message from topic AzureIoT hub");
            _logger.Trace(e.Message);
            MessageReceived?.Invoke(this, new ObjectEventArgs(e.Message));
        }

        public void Disconnect()
        {
            _client?.Close();
        }

        public void Publish(string messageString, string messageType)
        {
            var isReceived = _client.SendMessage(messageString, "application/json", new System.Collections.ArrayList() { new UserProperty("messageType", messageType) }, new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token);
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
