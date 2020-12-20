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
        private OAuthClient _oAuthClient;
        private Passport _passport;
        private Gateway _gateway;
        private ThinqClient _thinqClient;

        public async Task Initialize(string username, string password, string country, string languageCode, string chromiumPath = null)
        {
            _oAuthClient = new OAuthClient(username, password, country, languageCode, chromiumPath);
            _passport = await _oAuthClient.GetPassport();
            GatewayClient gatewayClient = new GatewayClient(_passport, _oAuthClient);
            _gateway = await gatewayClient.GetGateway();
            _thinqClient = new ThinqClient(_passport, _gateway.Thinq2Uri, _oAuthClient);
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
                    deviceClient = new AcClient(_passport, _gateway.Thinq2Uri, device.DeviceId, _oAuthClient);
                    break;
            }
            return deviceClient;
        }

        public async Task<ThinqMqttClient> GetMqttClient()
        {
            CommonClient commonClient = new CommonClient(_passport, Constants.COMMON_BASE_URL, _oAuthClient);
            RouteResponse route = await commonClient.GetRoute();
            ThinqMqttClient mqttClient = new ThinqMqttClient(route.MqttServer, _thinqClient);
            return mqttClient;
        }
    }
}
