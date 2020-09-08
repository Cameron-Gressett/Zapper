using AppComponents.Extensions.EnumEx;
using AppComponents.Extensions.StringEx;
using AppComponents.RandomNumbers;
using AutoMapper;
using DomainModel.Common;
using System;
using System.Linq;

namespace DomainModel.CAST
{
    public class DeviceViewModel : DeviceDataModel
    {
        static DeviceViewModel()
        {
            Mapper.CreateMap<DeviceDataModel, DeviceViewModel>();
        }

        public DeviceViewModel()
        {

        }

        public DeviceViewModel(DeviceDataModel dm, int tzOffsetMinutes)
        {
            Mapper.Map(dm, this);

            LastContactLocal = WebDateOffset.GetLocalDateTime(LastContact, tzOffsetMinutes);
            StateFriendly = dm.State.EnumName().SpacePascalCase();

        }

        public DeviceViewModel(Random rng, int tzOffsetMinutes)
        {
            int satCount = rng.Next(10);
            int useSatCount = satCount - (rng.Next() % 5);

            if (useSatCount < 1)
            {
                useSatCount = 1;
            }

            var dm = new DeviceDataModel
            {
                Id = Guid.NewGuid(),
                SerialNumber = Test.GenSN(rng),
                LastContact = DateTime.UtcNow - TimeSpan.FromHours(rng.Next() % 100),

                LocationAgeMin = rng.Next() % 4,
                VIN = Test.GenString(17, rng),
                VehicleInformation = "IMAGINARY FORD AEROSUV 4DR MRFUSION ENGINE",

                SupervisorFirmwareVersion = (rng.Next() % 100).ToString(),

                IsHighVoltage = Test.Flip(rng),
                Is1939AppSupport = Test.Flip(rng),
                Is1939SupervisorSupport = Test.Flip(rng),
                Is1708AppSupport = Test.Flip(rng),

                BLEState = GoodSeedRandom.RandomPickEnum<BLEState>(rng),
                BluetoothFirmwareVersion = (rng.Next() % 20).ToString(),

                ModemFirmwareVersion = rng.Next() % 32000,
                BoardVersion = 80 + (rng.Next() % 12),

                CustomerApplicationVersion = rng.Next() % 12,
                ApplicationFirmwareVersion = Test.GenString(4, rng),
                SimType = rng.Next() % 4,

                ICCID = "89011704" + (rng.Next() % 100000000000).ToString("D12"),
                IMEI = Test.GenString(15, rng),

                ExternalVoltage = 12000 + (rng.Next() % 1100),
                BatteryVoltage = 10000 + (rng.Next() % 1111),

                ModemSwitcher = 0,
                Thermistor = 20 + (rng.Next() % 50),

                CommandCount = rng.Next() % 2000,
                GPSState = 1,
                GSAFixMode = 1,
                GSVSatsTracking = satCount,
                NumSats = useSatCount,

                SatSignals = Enumerable.Range(0, NumSats)
                    .Select(it =>
                        new SatSignal
                        {
                            PRN = rng.Next() % 30,
                            SignalQuality = rng.Next() % 50
                        }
                    ).ToArray(),

                GSMReg = 1,
                GPRSReg = 1,

                RSSI = 1,
                GPRSAttached = rng.Next() % 100,
                IsGPRSActive = true,
                NetworkMode = "HSDPA",
                Operator = "AT&T",
                Band = "WCDMA PCS 1900",

                IPAddress = Test.GenIP(rng),
                MCC = Test.GenString(4, rng),
                MNC = Test.GenString(4, rng),
                LAC = Test.GenString(5, rng),

                DeltaParameters = "4=sim.attz.com;5=fakepass;12=1.2.3.4",

                CanType = 1,
                BusMode = 1,
                NodeType = 1,
                BaudRate = rng.Next() % 4,

                LastReportedStandardConfigurationHash = Test.GenString(4, rng),
                LastReportedDeltaConfigurationHash = Test.GenString(4, rng),


                PingCount = rng.Next() % 1500,
                State = GoodSeedRandom.RandomPickEnum<ConfigurationStates>(rng),
                DeviceModel = rng.NextDouble() <= 0.5 ? "80" : "81",
                Tags = Test.GenTags(15, rng, Test.RandomTags)


            };

            Mapper.Map(dm, this);

            LastContactLocal = WebDateOffset.GetLocalDateTime(LastContact, tzOffsetMinutes);

            StateFriendly = dm.State.EnumName().SpacePascalCase();

            BluetoothFirmwareVersionTitle = "Geometris BLE 2938";
            SupervisorFirmwareVersionTitle = "Supervisor 2291";
            ApplicationFirmwareVersionTitle = "Application 2923";
            DesiredApplicationFirmwareVersionTitle = "Geometris BLE 2938";
            DesiredBluetoothFirmwareVersionTitle = "Supervisor 2294";
            DesiredSupervisorFirmwareVersionTitle = "Application 2929";
            StandardConfigurationTitle = rng.NextDouble() <= 0.5 ? "Standard Config A" : "Standard Config B";

            DeltaConfigurationTitle = rng.NextDouble() <= 0.5 ? "Standard Config A" : "Standard Config B";
            DesiredDeltaConfigurationTitle = "Standard Config B";


        }

        public DateTime LastContactLocal { get; set; }
    }
}
