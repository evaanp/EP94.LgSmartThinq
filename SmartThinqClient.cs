using EP94.LgSmartThinq.Clients;
using EP94.LgSmartThinq.Models;
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

        public async Task Initialize(string username, string password, string country, string languageCode, string chromiumPath = null)
        {
            _oAuthClient = new OAuthClient(username, password, country, languageCode, chromiumPath);
            _passport = await _oAuthClient.GetPassport();
            GatewayClient gatewayClient = new GatewayClient(_passport, _oAuthClient);
            _gateway = await gatewayClient.GetGateway();
        }

        public async Task<List<Device>> GetDevices()
        {
            ThinqClient thinqClient = new ThinqClient(_passport, _gateway.Thinq2Uri, _oAuthClient);
            return await thinqClient.GetDevices();
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
    }
}
