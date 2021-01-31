using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using FastMember;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public class AcClient : DeviceClient
    {
        internal AcClient(Passport passport, string baseUrl, Device device, OAuthClient oAuthClient, ThinqClient thinqClient) : base(passport, baseUrl, device, oAuthClient, thinqClient)
        {
        }

        public async Task<bool> TurnOnAc() => await SendCommand("Operation", "airState.operation", 1);
        public async Task<bool> TurnOffAc() => await SendCommand("Operation", "airState.operation", 0);
        public async Task<bool> SetVerticalStep(int value) => await SendCommand("Set", "airState.wDir.vStep", value);
        public async Task<bool> SetHorizontalStep(int value) => await SendCommand("Set", "airState.wDir.hStep", value);
        public async Task<bool> SetMode(Mode mode) => await SendCommand("Set", "airState.opMode", (int)mode);
        public async Task<bool> SetTemperatureSetpoint(int setpoint) => await SendCommand("Set", "airState.tempState.target", setpoint);
        public async Task<bool> SetFanSpeed(FanSpeed speed) => await SendCommand("Set", "airState.windStrength", (int)speed);
        public async Task<double> GetMeasuredTemperature() => (await GetDeviceSnapshot()).MeasuredTemperature.Value;
        public async Task<Snapshot> GetSnapshot() => await GetDeviceSnapshot();
        public async Task RefreshToken() => await _oAuthClient.RefreshOAuthToken(_passport);
        public async Task<HvacState> GetState()
        {
            Snapshot snapshot = await GetSnapshot();
            if (snapshot == null || !snapshot.IsOn.HasValue) return HvacState.Error;
            bool isOn = snapshot.IsOn.Value;
            if (!isOn) return HvacState.Off;
            switch (snapshot.OperationMode)
            {
                case (int)Mode.Heat: return HvacState.Heating;
                case (int)Mode.Cool: return HvacState.Cooling;
                case (int)Mode.Auto: return HvacState.Auto;
                case (int)Mode.Dry: return HvacState.Dehumidifying;
                case (int)Mode.Fan: return HvacState.Fan;
            }
            return HvacState.Error;
        }
    }
}
