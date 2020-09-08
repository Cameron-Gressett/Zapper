using AppComponents;
using AppComponents.Extensions.ExceptionEx;
using AppComponents.Validation;
using DomainModel.CAST.Device;
using DomainModel.Core;
using DomainModel.DataService;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppComponents.Diagnostics;
using AppComponents.Extensions.EnumerableEx;
using DomainModel.CAST.ConfigFile;

namespace DomainModel.CAST
{


    /// <summary>
    /// For a given device, what kind of
    /// encounters has it had with CAST?
    /// </summary>
    public enum ConfigurationStates
    {
        /// <summary>
        /// Device is registered, but we haven't heard from it yet
        /// </summary>
        New,

        /// <summary>
        /// The device has sent a heartbeat to CAST before.
        /// </summary>
        Contacted,

        /// <summary>
        /// Pending config or firmware changes 
        /// </summary>
        Pending,

        /// <summary>
        /// CAST has updated configuration or firmware on this device
        /// and completed it.
        /// </summary>
        Updated,

        /// <summary>
        /// It's been a long time since this device reported a heartbeat.
        /// </summary>
        OutOfContact,

        PendingAnalysis
    }


    /// <summary>
    /// Details about devices as recorded from their most recent heartbeat
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DeviceDataModel: ICASTDataModel
    {
        /// <summary>
        /// Unique identifier for the device record
        /// </summary>
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        /// <summary>
        /// Device serial number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Last time a heartbeat was received (UTC)
        /// </summary>
        public DateTime LastContact { get; set; }

     
        /// <summary>
        /// Geolocation of the device
        /// </summary>
        public Location LastKnownLocation { get; set; }

        public int LocationAgeMin { get; set; }

        /// <summary>
        /// Vehicle VIN reading at time of heartbeat
        /// </summary>
        public string VIN { get; set; }

        /// <summary>
        /// Vehicle information based on last read VIN
        /// </summary>
        public string VehicleInformation { get; set; }

        /// <summary>
        /// Current Bluetooth Firmware version
        /// </summary>
        public string BluetoothFirmwareVersion { get; set; }

        /// <summary>
        /// Current supervisor firmware version
        /// </summary>
        public string SupervisorFirmwareVersion { get; set; }

        /// <summary>
        /// Current appliaction firmware version
        /// </summary>
        public string ApplicationFirmwareVersion { get; set; }

        /// <summary>
        /// Device supports high voltage?
        /// </summary>
        public bool IsHighVoltage { get; set; }

        /// <summary>
        /// Device application supports J1939?
        /// </summary>
        public bool Is1939AppSupport { get; set; }

        /// <summary>
        /// Device supervisor supports J1939?
        /// </summary>
        public bool Is1939SupervisorSupport { get; set; }

        /// <summary>
        /// Device application supports J1708?
        /// </summary>
        public bool Is1708AppSupport { get; set; }

        /// <summary>
        /// Device supervisor supports J1708 
        /// </summary>
        public bool Is1708SupervisorSupport { get; set; }

        /// <summary>
        /// How is Bluetooth support working right now?
        /// </summary>
        public BLEState BLEState { get; set; }

        /// <summary>
        /// Modem firmware version
        /// </summary>
        public int ModemFirmwareVersion { get; set; }

        /// <summary>
        /// Board version
        /// </summary>
        public int BoardVersion { get; set; }

        /// <summary>
        /// Customer application version
        /// </summary>
        public int CustomerApplicationVersion { get; set; }


        /// <summary>
        /// Sim Type
        /// </summary>
        public int SimType { get; set; } // interpretation?


        /// <summary>
        /// cellular integrated circuit card identifier
        /// </summary>
        public string ICCID { get; set; }

        /// <summary>
        /// SMS phone number.
        /// </summary>
        public string MSISDN { get; set; }

        /// <summary>
        /// cellular international mobile equipment identity
        /// </summary>
        public string IMEI { get; set; }


        /// <summary>
        /// Car voltage 
        /// </summary>
        public int ExternalVoltage { get; set; }

        /// <summary>
        /// Int. battery voltage
        /// </summary>
        public int BatteryVoltage { get; set; }

        /// <summary>
        /// No idea
        /// </summary>
        public int ModemSwitcher { get; set; }

        /// <summary>
        /// Measures internal temperature
        /// </summary>
        public int Thermistor { get; set; }


        /// <summary>
        /// gps state command count??
        /// </summary>
        public int CommandCount { get; set; }

        /// <summary>
        /// gps state (needs interpretation)
        /// </summary>
        public int GPSState { get; set; }

        /// <summary>
        /// gps fix mode (needs interpretation)
        /// </summary>
        public int GSAFixMode { get; set; }

        /// <summary>
        /// how many satellites can we see?
        /// </summary>
        public int GSVSatsTracking { get; set; }


        /// <summary>
        /// how many satellites are we using?
        /// </summary>
        public int NumSats { get; set; }

        /// <summary>
        /// Satellite signal
        /// </summary>
        public SatSignal[] SatSignals { get; set; }


        /// <summary>
        /// GSM Registration
        /// </summary>
        public int GSMReg { get; set; }

        /// <summary>
        /// GPRS Registration
        /// </summary>
        public int GPRSReg { get; set; }

        /// <summary>
        /// Received signal strength indicator
        /// Cellular signal str
        /// </summary>
        public int RSSI { get; set; }

        /// <summary>
        /// GPRS attach
        /// https://riazdaguru.files.wordpress.com/2007/08/gprs-attach_detach.pdf
        /// </summary>
        public int GPRSAttached { get; set; }

        /// <summary>
        /// GPRS is active
        /// </summary>
        public bool IsGPRSActive { get; set; }



        public string NetworkMode { get; set; }

        /// <summary>
        /// mobile country code
        /// </summary>
        public string MCC { get; set; }

        /// <summary>
        /// mobile network code
        /// </summary>
        public string MNC { get; set; }

        /// <summary>
        /// Loction area code
        /// </summary>
        public string LAC { get; set; }

        /// <summary>
        /// cell phone tower id
        /// </summary>
        public string CI { get; set; }


        /// <summary>
        /// cell carrier
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// frequency band
        /// </summary>
        public string Band { get; set; }

        /// <summary>
        /// cell carrier device ip address
        /// </summary>
        public string IPAddress { get; set; }


        /// <summary>
        /// parameters that differ from the standard
        /// </summary>
        public string DeltaParameters { get; set; }



        public int CanType { get; set; }

        public int BusMode { get; set; }

        public int NodeType { get; set; }

        /// <summary>
        /// can bus speed
        /// </summary>
        public int BaudRate { get; set; }


        /// <summary>
        /// What was the configuration hash in the last heartbeat?
        /// </summary>
        public string LastReportedStandardConfigurationHash { get; set; }

        public string LastReportedDeltaConfigurationHash { get; set; }


        public Guid? LastReportedStandardConfiguration { get; set; }

        public Guid? LastReportedDeltaConfiguration { get; set; }
        
        public Guid? LastReportedApplicationFirmwareVersion { get; set; }
        public Guid? LastReportedBluetoothFirmwareVersion { get; set; }
        public Guid? LastReportedSupervisorFirmwareVersion { get; set; }
        


        /// <summary>
        /// Desired bluetooth firmware version
        /// </summary>
        public Guid? DesiredBluetoothFirmwareVersion { get; set; }

        /// <summary>
        /// Desired supervisor firmware version
        /// </summary>
        public Guid? DesiredSupervisorFirmwareVersion { get; set; }

        /// <summary>
        /// Desired application firmware version
        /// </summary>
        public Guid? DesiredApplicationFirmwareVersion { get; set; }
        
              

        /// <summary>
        /// Reference to the desired configuration on the device
        /// </summary>
        public Guid? DesiredDeltaConfiguration { get; set; }

        public string DesiredDeltaConfigurationHash { get; set; }

        public bool DesiresResetConfig { get; set; }
        

        /// <summary>
        /// How many heartbeats we have received from this device
        /// </summary>
        public int PingCount { get; set; }

        /// <summary>
        /// How have we interacted with this device so far, and is it in contact
        /// </summary>
        public ConfigurationStates State { get; set; }

        /// <summary>
        /// What kind of device is this?
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// organizing tags
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        // user friendly properties

        public string StateFriendly { get; set; }

        public string BluetoothFirmwareVersionTitle { get; set; }

        public string SupervisorFirmwareVersionTitle { get; set; }

        public string ApplicationFirmwareVersionTitle { get; set; }

        public string DesiredSupervisorFirmwareVersionTitle { get; set; }

        public string DesiredBluetoothFirmwareVersionTitle { get; set; }

        public string DesiredApplicationFirmwareVersionTitle { get; set; }

        public string StandardConfigurationTitle { get; set; }

        

        public string DeltaConfigurationTitle { get; set; }

        public string DesiredDeltaConfigurationTitle { get; set; }

        public string Message { get; set; }

        public bool IsExcludedFromConfigurationUpdates { get; set; }

        public CASTIntegrationMethods CommunicationMethod { get; set; }

        public string SimSource { get; set; }

        public int RecentPingsCount { get; set; }

        public DateTime Lockout { get; set; }


        public string SettledDeltaHash { get; set; }
        public DateTime SettledDeltaHashTime { get; set; }

        public string ExtraConfig { get; set; }

        public List<string> AssignedKeywords { get; set; } = new List<string>();
        public DateTime KeywordsChanged { get; set; }

        public List<string> Capabilities { get; set; } = new List<string>();

        private static ConfigFileRepository _cfRepo = new ConfigFileRepository();
        private static FirmwareFileRepository _ffRepo = new FirmwareFileRepository();
        private static GlobalConfigHashMapRepository _configHash = new GlobalConfigHashMapRepository();

        public bool UpdateFromPing(ConfigPingDataModel cp, CASTIntegrationMethods method, string tenant)
        {
            bool requiresHarvest = false;

            try
            {
                var pt = new PerfTrack();
                pt.Begin("CAST DeviceDataModel UpdateFromPing");
                using (Disposable.Create(() => pt.End()))
                {

                    cp.Requires().IsNotNull();
                    tenant.Requires().IsNotNullOrEmpty();

                    
                    LastKnownLocation = new Location {GeoLocation = cp.Location};
                    LocationAgeMin = cp.LocationAgeMin;
                    if (!string.IsNullOrWhiteSpace(cp.VIN))
                        VIN = cp.VIN;

                    IsHighVoltage = cp.IsHighVoltage;
                    Is1939AppSupport = cp.Is1939AppSupport;
                    Is1939SupervisorSupport = cp.Is1939SupervisorSupport;
                    Is1708AppSupport = cp.Is1708AppSupport;
                    Is1708SupervisorSupport = cp.Is1708SupervisorSupport;
                    BLEState = cp.BLEState;
                    ModemFirmwareVersion = cp.ModemFirmwareVersion;
                    BoardVersion = cp.BoardVersion;
                    CustomerApplicationVersion = cp.CustomerApplicationVersion;
                    SimType = cp.SimType;
                    if (!string.IsNullOrWhiteSpace(cp.ICCID)) ICCID = cp.ICCID;
                    if (!string.IsNullOrWhiteSpace(cp.MSISDN)) MSISDN = cp.MSISDN;
                    if (!string.IsNullOrWhiteSpace(cp.IMEI)) IMEI = cp.IMEI;
                    ExternalVoltage = cp.ExternalVoltage;
                    BatteryVoltage = cp.BatteryVoltage;
                    ModemSwitcher = cp.ModemSwitcher;
                    Thermistor = cp.Thermistor;
                    CommandCount = cp.CommandCount;
                    GPSState = cp.GPSState;
                    GSAFixMode = cp.GSAFixMode;
                    GSVSatsTracking = cp.GSVSatsTracking;
                    NumSats = cp.NumSats;
                    if (null != cp.SatSignals && cp.SatSignals.Any()) SatSignals = cp.SatSignals;
                    GSMReg = cp.GSMReg;
                    GPRSReg = cp.GPRSReg;
                    RSSI = cp.RSSI;
                    GPRSAttached = cp.GPRSAttached;
                    IsGPRSActive = cp.IsGPRSActive;
                    if (!string.IsNullOrWhiteSpace(cp.NetworkMode)) NetworkMode = cp.NetworkMode;
                    if (!string.IsNullOrWhiteSpace(cp.MCC)) MCC = cp.MCC;
                    if (!string.IsNullOrWhiteSpace(cp.MNC)) MNC = cp.MNC;
                    if (!string.IsNullOrWhiteSpace(cp.LAC)) LAC = cp.LAC;
                    if (!string.IsNullOrWhiteSpace(cp.Operator)) Operator = cp.Operator;
                    if (!string.IsNullOrWhiteSpace(cp.Band)) Band = cp.Band;
                    CanType = cp.CanType;
                    BusMode = cp.BusMode;
                    NodeType = cp.NodeType;
                    BaudRate = cp.BaudRate;

                    CommunicationMethod = method;

                    if (!string.IsNullOrWhiteSpace(cp.DeltaParameters))
                        DeltaParameters = cp.DeltaParameters;


                    if ((cp.StandardHash != null && LastReportedStandardConfigurationHash != cp.StandardHash) ||
                        (cp.StandardHash != null && StandardConfigurationTitle == null))
                    {
                        LastReportedStandardConfigurationHash = cp.StandardHash;
                        requiresHarvest = _configHash.DoesStandardConfigExist(cp.StandardHash) == false;

                      
                    }

                    if ((cp.DeltaHash != null && LastReportedDeltaConfigurationHash != cp.DeltaHash) ||
                        (cp.DeltaHash != null && DeltaConfigurationTitle == null))
                    {
                        LastReportedDeltaConfigurationHash = cp.DeltaHash;

                        if (cp.DeltaHash == "0")
                        {
                            LastReportedDeltaConfiguration = null;
                            DeltaConfigurationTitle = null;
                        }
                        else
                        {
                            var dcf = _cfRepo.CachedFindByHash(LastReportedDeltaConfigurationHash, tenant);
                            if (null != dcf)
                            {
                                LastReportedDeltaConfiguration = dcf.Id;
                                DeltaConfigurationTitle = dcf.Title;
                            }
                        }
                    }

                    if (BluetoothFirmwareVersion != cp.BLEVersion.ToString())
                    {
                        BluetoothFirmwareVersion = cp.BLEVersion.ToString();
                        var bfw = _ffRepo.CachedLoadByVersion(BluetoothFirmwareVersion);
                        if (null != bfw)
                        {
                            BluetoothFirmwareVersionTitle = bfw.Title;
                            LastReportedBluetoothFirmwareVersion = bfw.Id;
                        }
                    }

                    if (SupervisorFirmwareVersion != cp.SupervisorVersion.ToString())
                    {
                        SupervisorFirmwareVersion = cp.SupervisorVersion.ToString();
                        var sfw = _ffRepo.CachedLoadByVersion(SupervisorFirmwareVersion);
                        if (null != sfw)
                        {
                            SupervisorFirmwareVersionTitle = sfw.Title;
                            LastReportedSupervisorFirmwareVersion = sfw.Id;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(cp.ApplicationVersion) &&
                        ApplicationFirmwareVersion != cp.ApplicationVersion)
                    {
                        ApplicationFirmwareVersion = cp.ApplicationVersion;
                        var afw = _ffRepo.CachedLoadByVersion(ApplicationFirmwareVersion);
                        if (null != afw)
                        {
                            ApplicationFirmwareVersionTitle = afw.Title;
                            LastReportedApplicationFirmwareVersion = afw.Id;
                        }
                    }

                    if (cp.Capability.EmptyIfNull().Any())
                    {
                        Capabilities = new List<string>();
                        Capabilities.AddRange(cp.Capability);
                    }

                    

                    PingCount += 1;

                    var elapsedTime = Math.Abs((LastContact - cp.PingTime).TotalHours);
                    if (elapsedTime < 24.0)
                        RecentPingsCount += 1;
                    else
                        RecentPingsCount = 0;

                    if (RecentPingsCount > 3)
                    {
                        Lockout = DateTime.UtcNow.AddDays(1.0);
                    }

                    LastContact = cp.PingTime;

                }

            } catch (Exception ex)
            {
                var aa = Catalog.Factory.Resolve<IApplicationAlert>();
                aa.RaiseAlert(ApplicationAlertKind.Warning, ex.TraceInformation());
                throw;
            }

            return requiresHarvest;
        }





    }



}
