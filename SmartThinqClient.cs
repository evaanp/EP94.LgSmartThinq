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
        private Passport _passport;
        private Gateway _gateway;

        public async Task Initialize(string username, string password, string country, string languageCode)
        {
            OAuthClient oAuthClient = new OAuthClient(username, password, country, languageCode);
            _passport = await oAuthClient.GetPassport();
            GatewayClient gatewayClient = new GatewayClient(_passport);
            _gateway = await gatewayClient.GetGateway();
        }

        public async Task<List<Device>> GetDevices()
        {
            ThinqClient thinqClient = new ThinqClient(_passport, _gateway.Thinq2Uri);
            return await thinqClient.GetDevices();
        }

        public DeviceClient GetDeviceClient(Device device)
        {
            switch (device.DeviceType)
            {
                // ac
                case 401:
                    return new AcClient(_passport, _gateway.Thinq2Uri, device.DeviceId);
            }
            return null;
        }
    }
}
