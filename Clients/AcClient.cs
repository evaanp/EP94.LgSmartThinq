using EP94.LgSmartThinq.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public class AcClient : DeviceClient
    {
        internal AcClient(Passport passport, string baseUrl, string deviceId) : base(passport, baseUrl, deviceId)
        {
        }

        public async Task<bool> TurnOnAc() => await SendCommand("Operation", "airState.operation", 1);
        public async Task<bool> TurnOffAc() => await SendCommand("Operation", "airState.operation", 0);
        public async Task<bool> SetVerticalStep(int value) => await SendCommand("Set", "airState.wDir.vStep", value);
        public async Task<bool> SetHorizontalStep(int value) => await SendCommand("Set", "airState.wDir.hStep", value);
        public async Task<bool> SetMode(Mode mode) => await SendCommand("Set", "airState.opMode", (int)mode);
        public async Task<bool> SetTemperatureSetpoint(double setpoint) => await SendCommand("Set", "airState.tempState.target", setpoint);
        public async Task<bool> SetFanSpeed(FanSpeed speed) => await SendCommand("Set", "airState.windStrength", (int)speed);
        public async Task<double> GetMeasuredTemperature() => (await GetDeviceSnapshot()).AirStatetempStatecurrent;
    }
}
