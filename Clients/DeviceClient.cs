using AutoMapper;
using EP94.LgSmartThinq.Models;
using EP94.LgSmartThinq.Utils;
using FastMember;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EP94.LgSmartThinq.Clients
{
    public abstract class DeviceClient : ThinqApiClient
    {
        private Snapshot _snapshot;
        private Device _device;
        private ThinqClient _thinqClient;
        private IMapper _objectMapper;
        private TypeAccessor _typeAccessor;

        internal DeviceClient(Passport passport, string baseUrl, Device device, OAuthClient oAuthClient, ThinqClient thinqClient) : base(passport, baseUrl, oAuthClient)
        {
            _device = device;
            _thinqClient = thinqClient;
            _typeAccessor = TypeAccessor.Create(typeof(Snapshot));
            MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<Snapshot, Snapshot>().ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null)));
            _objectMapper = mapperConfiguration.CreateMapper();
            ConnectMqtt();
        }

        private static Dictionary<Type, Type> _typeConversions = new Dictionary<Type, Type>()
        {
            {  typeof(bool), typeof(int) }
        };
        public async Task<bool> SetSnapshot(Snapshot desiredSnapshot)
        {
            try
            {
                Snapshot current = await GetDeviceSnapshot();
                foreach (Member member in _typeAccessor.GetMembers())
                {
                    object currentValue = _typeAccessor[current, member.Name];
                    object desiredValue = _typeAccessor[desiredSnapshot, member.Name];
                    if (!currentValue.Equals(desiredValue) && desiredValue != null)
                    {
                        Attribute attribute = member.GetAttribute(typeof(JsonIgnoreAttribute), false);
                        if (attribute is JsonIgnoreAttribute) continue;
                        string name = null;
                        attribute = member.GetAttribute(typeof(JsonPropertyAttribute), false);
                        if (attribute is JsonPropertyAttribute jpa) name = jpa.PropertyName;
                        else name = member.Name;
                        string command = name.Contains("operation") ? "Operation" : "Set";

                        if (_typeConversions.TryGetValue(desiredValue.GetType(), out Type type))
                        {
                            desiredValue = Convert.ChangeType(desiredValue, type);
                        }
                        await SendCommand(command, name, desiredValue);
                        await Task.Delay(500);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SmartThinqLogger.Log("Exception occured {0}", LogLevel.Error, e);
                return false;
            }
        }

        protected async void ConnectMqtt()
        {
            CommonClient commonClient = new CommonClient(_passport, Constants.COMMON_BASE_URL, _oAuthClient);
            RouteResponse route = await commonClient.GetRoute();
            ThinqMqttClient mqttClient = ThinqMqttClient.GetOrCreate(route.MqttServer, _thinqClient);
            mqttClient.SubscribeToChanges(_device, (snapshot) =>
            {
                if (_snapshot == null)
                    _snapshot = snapshot;
                else
                {
                    _objectMapper.Map(snapshot, _snapshot);
                    _snapshot.LastUpdated = DateTime.UtcNow;
                }
            });
            mqttClient.OnConnectionStatusChange += (connected) =>
            {
                if (!connected && _snapshot != null)
                    _snapshot.LastUpdated = DateTime.MinValue;
            };
            _ = mqttClient.Connect(true);
        }

        protected async Task<bool> SendCommand(string command, string dataKey, object dataValue)
        {
            using HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(HttpMethod.Post, $"/service/devices/{_device.DeviceId}/control-sync");
            Dictionary<string, string> commandDict = new Dictionary<string, string>
            {
                { "command", command },
                { "dataKey", dataKey },
                { "dataValue", dataValue.ToString() },
                { "ctrlKey", "basicCtrl" }
            };
            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(commandDict));
            httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await ExecuteRequest(httpRequestMessage);
        }

        protected async Task<Snapshot> GetDeviceSnapshot()
        {
            try
            {
                if (_snapshot == null || DateTime.UtcNow - _snapshot.LastUpdated > TimeSpan.FromMinutes(5))
                {
                    Snapshot snapshot = await GetFreshSnapshot();
                    _snapshot = snapshot;
                }
                return _snapshot;
            }
            catch
            {
                return null;
            }
        }

        private async Task<Snapshot> GetFreshSnapshot()
        {
            using HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(HttpMethod.Get, $"/service/devices/{_device.DeviceId}");
            Device device = await ExecuteRequest<Device>(httpRequestMessage);
            return device.Snapshot;
        }
    }
}
