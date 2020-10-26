using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Models
{
    public class Device
    {
        public string AppType { get; set; }
        public string ModelCountryCode { get; set; }
        public string CountryCode { get; set; }
        public string ModelName { get; set; }
        public int DeviceType { get; set; }
        public string DeviceCode { get; set; }
        public string Alias { get; set; }
        public string DeviceId { get; set; }
        public string FwVer { get; set; }
        public string ImageFileName { get; set; }
        public string ImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string Ssid { get; set; }
        public string SoftapId { get; set; }
        public string SoftapPass { get; set; }
        public string MacAddress { get; set; }
        public string NetworkType { get; set; }
        public string TimezoneCode { get; set; }
        public string TimezoneCodeAlias { get; set; }
        public int UtcOffset { get; set; }
        public string UtcOffsetDisplay { get; set; }
        public int DstOffset { get; set; }
        public string DstOffsetDisplay { get; set; }
        public int CurOffset { get; set; }
        public string CurOffsetDisplay { get; set; }
        public string SdsGuide { get; set; }
        public string NewRegYn { get; set; }
        public string RemoteControlType { get; set; }
        public string UserNo { get; set; }
        public string TftYn { get; set; }
        public float ModelJsonVer { get; set; }
        public string ModelJsonUri { get; set; }
        public float AppModuleVer { get; set; }
        public string AppModuleUri { get; set; }
        public string AppRestartYn { get; set; }
        public int AppModuleSize { get; set; }
        public float LangPackProductTypeVer { get; set; }
        public string LangPackProductTypeUri { get; set; }
        public string DeviceState { get; set; }
        public Snapshot Snapshot { get; set; }
        public bool Online { get; set; }
        public string PlatformType { get; set; }
        public int Area { get; set; }
        public float RegDt { get; set; }
        public string BlackboxYn { get; set; }
        public string ModelProtocol { get; set; }
        public int Order { get; set; }
        public string DrServiceYn { get; set; }
        public Fwinfolist[] FwInfoList { get; set; }
        public Modeminfo ModemInfo { get; set; }
        public string GuideTypeYn { get; set; }
        public string GuideType { get; set; }
        public string RegDtUtc { get; set; }
        public int RegIndex { get; set; }
        public string GroupableYn { get; set; }
        public string ControllableYn { get; set; }
        public string CombinedProductYn { get; set; }
        public string MasterYn { get; set; }
        public string PccModelYn { get; set; }
        public Sdspid SdsPid { get; set; }
        public string AutoOrderYn { get; set; }
        public bool InitDevice { get; set; }
        public string ExistsEntryPopup { get; set; }
        public int Tclcount { get; set; }
    }
    public class Snapshot
    {
        [JsonProperty("airState.windStrength")]
        public float AirStatewindStrength { get; set; }
        [JsonProperty("airState.wMode.lowHeating")]
        public float AirStatewModelowHeating { get; set; }
        [JsonProperty("airState.diagCode")]
        public float AirStatediagCode { get; set; }
        [JsonProperty("airState.lightingState.displayControl")]
        public float AirStatelightingStatedisplayControl { get; set; }
        [JsonProperty("airState.wDir.hStep")]
        public float AirStatewDirhStep { get; set; }
        [JsonProperty("mid")]
        public float Mid { get; set; }
        [JsonProperty("airState.energy.onCurrent")]
        public float AirStateenergyonCurrent { get; set; }
        [JsonProperty("airState.wMode.airClean")]
        public float AirStatewModeairClean { get; set; }
        [JsonProperty("airState.quality.sensorMon")]
        public float AirStatequalitysensorMon { get; set; }
        [JsonProperty("airState.tempState.target")]
        public float AirStatetempStatetarget { get; set; }
        [JsonProperty("airState.operation")]
        public float AirStateoperation { get; set; }
        [JsonProperty("airState.wMode.jet")]
        public float AirStatewModejet { get; set; }
        [JsonProperty("airState.wDir.vStep")]
        public float AirStatewDirvStep { get; set; }
        [JsonProperty("timestamp")]
        public float Timestamp { get; set; }
        [JsonProperty("airState.powerSave.basic")]
        public float AirStatepowerSavebasic { get; set; }
        public Static Static { get; set; }
        [JsonProperty("airState.tempState.current")]
        public float AirStatetempStatecurrent { get; set; }
        [JsonProperty("airState.miscFuncState.extraOp")]
        public float AirStatemiscFuncStateextraOp { get; set; }
        [JsonProperty("airState.reservation.sleepTime")]
        public float AirStatereservationsleepTime { get; set; }
        [JsonProperty("airState.miscFuncState.autoDry")]
        public float AirStatemiscFuncStateautoDry { get; set; }
        [JsonProperty("airState.reservation.targetTimeToStart")]
        public float AirStatereservationtargetTimeToStart { get; set; }
        public Meta Meta { get; set; }
        public bool Online { get; set; }
        [JsonProperty("airState.opMode")]
        public float AirStateopMode { get; set; }
        [JsonProperty("airState.reservation.targetTimeToStop")]
        public float AirStatereservationtargetTimeToStop { get; set; }
        [JsonProperty("airState.filterMngStates.maxTime")]
        public float AirStatefilterMngStatesmaxTime { get; set; }
        [JsonProperty("airState.filterMngStates.useTime")]
        public float AirStatefilterMngStatesuseTime { get; set; }
    }

    public class Static
    {
        public string DeviceType { get; set; }
        public string CountryCode { get; set; }
    }

    public class Meta
    {
        public bool AllDeviceInfoUpdate { get; set; }
        public string MessageId { get; set; }
    }

    public class Modeminfo
    {
        public string ModelName { get; set; }
        public string AppVersion { get; set; }
        public string ModemType { get; set; }
        public string RuleEngine { get; set; }
    }

    public class Sdspid
    {
        public string Sds4 { get; set; }
        public string Sds3 { get; set; }
        public string Sds2 { get; set; }
        public string Sds1 { get; set; }
    }

    public class Fwinfolist
    {
        public string Checksum { get; set; }
        public string PartNumber { get; set; }
        public float Order { get; set; }
    }
}
