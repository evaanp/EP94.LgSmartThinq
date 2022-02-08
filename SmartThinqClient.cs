using EP94.LgSmartThinq.Clients;
using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq
{
    public class SmartThinqClient
    {
        public delegate void InitializationSuccessfulHandler();
        public InitializationSuccessfulHandler OnInitializationSuccessful;
        public bool IsInitializedSuccessful { get; set; } = false;
        private OAuthClient _oAuthClient;
        private Passport _passport;
        private Gateway _gateway;
        private ThinqClient _thinqClient;
        private const int MAX_INIT_TIMEOUT = 30000;
        private int _initTimeout = 2000;
        private bool _stopInitializing = false;

        public async Task Initialize(string username, string password, string country, string languageCode, string chromiumPath = null, string loginCode = null)
        {
            SmartThinqLogger.Log("Initializing...", LogLevel.Information);
            try
            {
                await DoInitialize(username, password, country, languageCode, chromiumPath, loginCode);
            }
            catch (Exception e)
            {
                SmartThinqLogger.Log("Exception occured while initializing: {0}", LogLevel.Error, e);
                TryInitializing(username, password, country, languageCode, chromiumPath);
            }
        }

        private async void TryInitializing(string username, string password, string country, string languageCode, string chromiumPath)
        {
            while (!IsInitializedSuccessful)
            {
                await Task.Delay(_initTimeout);
                try
                {
                    SmartThinqLogger.Log("Start retrying initializing...", LogLevel.Debug);
                    await DoInitialize(username, password, country, languageCode, chromiumPath);
                }
                catch (Exception e)
                {
                    SmartThinqLogger.Log("Exception occured while initializing: {0}", LogLevel.Error, e);
                }
                if (_stopInitializing) return;
                _initTimeout = Math.Min(_initTimeout * 2, MAX_INIT_TIMEOUT);
                SmartThinqLogger.Log("Try again in {0} seconds", LogLevel.Information, _initTimeout / 1000);
            }
        }

        private async Task DoInitialize(string username, string password, string country, string languageCode, string chromiumPath, string loginCode = null)
        {
            
            _oAuthClient = new OAuthClient(username, password, country, languageCode, chromiumPath);
            _passport = await _oAuthClient.GetPassport(loginCode);
            if (_passport == null)
            {
                _stopInitializing = true;
                return;
            }
            GatewayClient gatewayClient = new GatewayClient(_passport, _oAuthClient);
            _gateway = await gatewayClient.GetGateway();
            SmartThinqLogger.Log("Gateway object received", LogLevel.Debug);
            _thinqClient = new ThinqClient(_passport, _gateway.Thinq2Uri, _oAuthClient);
            SmartThinqLogger.Log("Initializing successful", LogLevel.Information);
            IsInitializedSuccessful = true;
            OnInitializationSuccessful?.Invoke();
        }

        public async Task<List<Device>> GetDevices()
        {
            return await _thinqClient.GetDevices();
        }

        public DeviceClient GetDeviceClient(Device device)
        {
            DeviceClient deviceClient = null;
            switch (device.DeviceType)
            {
                case DeviceType.AC:
                    deviceClient = new AcClient(_passport, _gateway.Thinq2Uri, device, _oAuthClient, _thinqClient);
                    break;
            }
            return deviceClient;
        }
    }
}
