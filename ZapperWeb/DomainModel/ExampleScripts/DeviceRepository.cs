using AppComponents;
using AppComponents.ControlFlow;
using AppComponents.Extensions.EnumerableEx;
using AppComponents.Extensions.EnumEx;
using AppComponents.Extensions.ExceptionEx;
using AppComponents.Mongo;
using AppComponents.RandomNumbers;
using DomainModel.CAST.UserSettings;
using DomainModel.Common;
using LinqKit;
using log4net;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RestSharp.Extensions;
using System.ComponentModel;

namespace DomainModel.CAST
{



    /// <summary>
    /// Describes how to filter the devices.
    /// Each property that is not null will be applied to the query.
    /// </summary>
    public class DevicesQuery
    {
        /// <summary>
        /// Used to translate LastHeartbeatTime, if used, to UTC
        /// and also to translate result times from UTC to
        /// local user time
        /// </summary>
        public int TimeZoneOffsetMinutes { get; set; }

        /// <summary>
        /// Find a device serial number starting or ending with the
        /// given substring
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Find a device ICCID starting or ending with the given 
        /// substring
        /// </summary>
        public string ICCID { get; set; }

        /// <summary>
        /// Find a device that has reported a VIN beginning
        /// or ending with the given substring
        /// </summary>
        public string VIN { get; set; }

        /// <summary>
        /// If not null, filter by this heart beat time
        /// </summary>
        public DateTime? LastHeartbeatTime { get; set; }

        /// <summary>
        /// Filter before the given last heartbeat filter, or for after the given filter?
        /// </summary>
        public bool Before { get; set; }


        /// <summary>
        /// Looks for devices with the given tags included
        /// </summary>
        public List<string> IncludeWithTags { get; set; } = new List<string>();

        /// <summary>
        /// Included devices matching tags filtering logic mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TagQueryMode IncludedTagsMode { get; set; }


        /// <summary>
        /// Looks for devices with the given tags excluded
        /// </summary>
        public List<string> ExcludeWithTags { get; set; } = new List<string>();

        /// <summary>
        /// Excluded devices matching tags filtering logic mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TagQueryMode ExcludedTagsMode { get; set; }

        /// <summary>
        /// If not null, filters based on configuration state (pending, completed, etc)
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ConfigurationStates? ConfigurationStateFilter { get; set; }


        /// <summary>
        /// Matches if the configuration hash matches the given configuration file hash, as 
        /// specified by configuration file title
        /// </summary>
        public string MatchingStandardConfigurationTitle { get; set; }

        public string MatchingDeltaConfigurationTitle { get; set; }

        /// <summary>
        /// Matches based on firmware version
        /// </summary>
        public string MatchingApplicationFirmwareVersion { get; set; }

        /// <summary>
        /// Matches based on firmware version
        /// </summary>
        public string MatchingBluetoothFirmwareVersion { get; set; }

        /// <summary>
        /// Matches based on firmware version
        /// </summary>
        public string MatchingSupervisorFirmwareVersion { get; set; }

        public string OrderField { get; set; }

        public bool OrderByAscending { get; set; }
    }


    public class DevicesQueryPaged : DevicesQuery
    {
        public int DesiredPage { get; set; }
        public int PageSize { get; set; }

        public string CustomerId { get; set; }
    }

    public class DevicesExportQuery
    {
        public DevicesQueryPaged Query { get; set; }
        public IList<GridColumnsDefinitionsDataModel> ColumnDefinitions { get; set; }
    }
    public class DevicesList
    {
        public List<DeviceViewModel> Data { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int LastPage { get; set; }
        public int TotalCount { get; set; }
    }

    public class DeviceRepository : CASTMongoRepository<DeviceDataModel>
    {
        private ILog _log;
        private ConfigFileRepository _configFileRepo = new ConfigFileRepository();

        public DeviceRepository()
        {
            _log = ClassLogger.Create(GetType());
        }



        private static IMongoCollection<DeviceDataModel> Collection(string tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant))
                tenant = ContextRegistry.GetTenancy();

            var db = DocumentStoreLocator.ForceResolve(
                CASTDB.CASTDatabase.EnumName(),
                tenant,
                Constants.DatabaseConnection());

            var collection = db.GetCollection<DeviceDataModel>();

            RunOnce.ThisCode(() =>
            {
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.SerialNumber));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.LastContact));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.ICCID));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.VIN));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.BluetoothFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.SupervisorFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.ApplicationFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.DesiredBluetoothFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.DesiredSupervisorFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.DesiredApplicationFirmwareVersion));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.LastReportedStandardConfigurationHash));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.DesiredDeltaConfigurationHash));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.State));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.DeviceModel));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.Tags));
                collection.AssureIndex(Builders<DeviceDataModel>.IndexKeys.Ascending(it => it.Capabilities));
            });


            return collection;

        }

        public DevicesList QueryDevices(DevicesQueryPaged query, string tenant)
        {
            DevicesList retval = new DevicesList();

            try
            {
                var pageSize = query.PageSize > 0 ? query.PageSize : Constants.GetPageSize();

                var dataQuery = CreateDeviceQuery(query, tenant);

                var totalCount = dataQuery.Item1.Count();
                var skipTo = pageSize * query.DesiredPage < totalCount ? pageSize * query.DesiredPage : pageSize * 0;
                var data =
                    dataQuery.Item1
                    .Skip(skipTo)
                    .Take(pageSize)
                    .ToList();

                retval.Data = data.Select(ddm => new DeviceViewModel(ddm, query.TimeZoneOffsetMinutes)).ToList();
                retval.CurrentPage = query.DesiredPage;
                retval.ItemsPerPage = pageSize;
                retval.LastPage = pageSize * query.DesiredPage > totalCount ? 0 : 1;
                retval.TotalCount = totalCount;
            }
            catch (Exception ex)
            {
                var aa = Catalog.Factory.Resolve<IApplicationAlert>();
                aa.RaiseAlert(ApplicationAlertKind.System, ex.TraceInformation());
                throw;
            }

            return retval;
        }

        public DevicesList QueryDevicesNoPaged(DevicesQueryPaged query, string tenant)
        {
            DevicesList retval = new DevicesList();

            var dataQuery = CreateDeviceQuery(query, tenant);
            var totalCount = dataQuery.Item1.Count();
            var data = dataQuery.Item1;

            retval.Data = data.ToList().Select(ddm => new DeviceViewModel(ddm, query.TimeZoneOffsetMinutes)).ToList();
            retval.CurrentPage = query.DesiredPage;
            retval.LastPage = 1;
            retval.TotalCount = totalCount;

            return retval;
        }
        /// <summary>
        /// Get devices Id by serial numbers 
        /// </summary>
        public List<Guid> QueryManyBySN(List<string> sns, string tenant)
        {
            List<Guid> retVal = new List<Guid>();
            var collection = CreateCollection(tenant);
            foreach (var sn in sns)
            {
                var data = collection.AsQueryable().Where(it => it.SerialNumber == sn).FirstOrDefault();
                if (null != data)
                {
                    retVal.Add(data.Id);
                }
            }
            return retVal;
        }

        public Tuple<DeviceDataModel, RepositoryContext> LoadBySerialNumber(string serialNumber, string tenant)
        {
            var collection = CreateCollection(tenant);

            var item = collection.FindSync(Builders<DeviceDataModel>.Filter.Eq(it => it.SerialNumber, serialNumber)).FirstOrDefault();

            return Tuple.Create(item, new RepositoryContext { Context = collection });
        }

        public Tuple<List<DeviceDataModel>, RepositoryContext> QuerySysDevices(DevicesQuery query, string tenant)
        {
            var dataQuery = CreateDeviceQuery(query, tenant);
            var pgSize = Constants.GetLargePageSize();
            var done = false;
            int page = 0;
            var data = new List<DeviceDataModel>();

            // fetch it a page at a time because mongo has a limit on how much it will
            // give you at once
            do
            {
                var pageData = dataQuery.Item1.Skip(page * pgSize).Take(pgSize);
                if (pageData.Any())
                {
                    data.AddRange(pageData);
                    page += 1;
                }
                else
                    done = true;
            } while (!done);
            return Tuple.Create(data, dataQuery.Item2);
        }

        public Tuple<List<DeviceDataModel>, RepositoryContext> QueryAllSysDevices(string tenant)
        {
            var coll = Collection(tenant);

            var dataQuery = coll.AsQueryable();

            var pgSize = Constants.GetLargePageSize();
            var done = false;
            int page = 0;
            var data = new List<DeviceDataModel>();

            // fetch it a page at a time because mongo has a limit on how much it will
            // give you at once
            do
            {
                var pageData = dataQuery.Skip(page * pgSize).Take(pgSize);
                if (pageData.Any())
                {
                    data.AddRange(pageData);
                    page += 1;
                }
                else
                    done = true;
            } while (!done);
            return Tuple.Create(data, new RepositoryContext { Context = coll });
        }

        private Tuple<IQueryable<DeviceDataModel>, RepositoryContext> CreateDeviceQuery(DevicesQuery query, string tenant)
        {
            // TODO: Fixup from data model

            var pred = PredicateBuilder.True<DeviceDataModel>();

            if (!string.IsNullOrWhiteSpace(query.SerialNumber))
            {
                pred = pred.And(it => it.SerialNumber != null && (it.SerialNumber.StartsWith(query.SerialNumber) || it.SerialNumber.EndsWith(query.SerialNumber)));
            }

            if (!string.IsNullOrWhiteSpace(query.ICCID))
            {
                pred = pred.And(it => it.ICCID != null && (it.ICCID.StartsWith(query.ICCID) || it.ICCID.EndsWith(query.ICCID)));
            }

            if (!string.IsNullOrWhiteSpace(query.VIN))
            {
                pred = pred.And(it => it.VIN != null && (it.VIN.StartsWith(query.VIN) || it.VIN.EndsWith(query.VIN)));
            }

            if (query.LastHeartbeatTime.HasValue)
            {
                if (query.Before)
                {
                    pred = pred.And(it => it.LastContact < query.LastHeartbeatTime.Value);
                }
                else
                {
                    pred = pred.And(it => it.LastContact > query.LastHeartbeatTime.Value);
                }
            }

            if (query.IncludeWithTags.EmptyIfNull().Any())
            {
                var userTags = query.IncludeWithTags.Where(it => !it.IsUpperCase()).Select(it => it.ToLowerInvariant());
                var capTags = query.IncludeWithTags.Where(it => it.IsUpperCase());

                if (userTags.Any())
                {
                    if (query.IncludedTagsMode == TagQueryMode.Conjunctive)
                    {
                        //pred = pred.And(it => !query.IncludeWithTags.Any(tg => !it.Tags.Contains(tg)));
                        // TODO: review the change
                        foreach (var tag in userTags)
                        {
                            pred = pred.And(u => u.Tags.Contains(tag));
                        }
                    }
                    else
                    {
                        pred = pred.And(it => it.Tags.Any(tg => userTags.Contains(tg)));
                    }
                }

                if (capTags.Any())
                {
                    pred = pred.And(it => it.Capabilities.Any(tg => capTags.Contains(tg)));
                }
            }

            if (query.ExcludeWithTags.EmptyIfNull().Any())
            {
                if (query.ExcludedTagsMode == TagQueryMode.Conjunctive)
                {
                    pred = pred.And(it => query.IncludeWithTags.Any(tg => it.Tags.Contains(tg)));
                }
                else
                {
                    pred = pred.And(it => !it.Tags.Any(tg => query.ExcludeWithTags.Contains(tg)));
                }
            }

            if (query.ConfigurationStateFilter.HasValue)
            {
                pred = pred.And(it => it.State == query.ConfigurationStateFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.MatchingStandardConfigurationTitle))
            {
                var cf = _configFileRepo.FindByTitle(query.MatchingStandardConfigurationTitle, tenant);
                if (null != cf)
                {
                    pred = pred.And(it => it.LastReportedStandardConfigurationHash == cf.Item1.ConfigurationHash);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.MatchingDeltaConfigurationTitle))
            {
                var configFiles = _configFileRepo.FindAllByTitle(query.MatchingDeltaConfigurationTitle, tenant);
                var configFileHashValues = configFiles.Select(u => u.Item1.ConfigurationHash).ToList();

                pred = pred.And(u => configFileHashValues.Contains(u.LastReportedDeltaConfigurationHash));
            }

            if (!string.IsNullOrWhiteSpace(query.MatchingApplicationFirmwareVersion))
            {
                pred = pred.And(it => it.ApplicationFirmwareVersion == query.MatchingApplicationFirmwareVersion);
            }

            if (!string.IsNullOrWhiteSpace(query.MatchingSupervisorFirmwareVersion))
            {
                pred = pred.And(it => it.SupervisorFirmwareVersion == query.MatchingSupervisorFirmwareVersion);
            }

            if (!string.IsNullOrWhiteSpace(query.MatchingBluetoothFirmwareVersion))
            {
                pred = pred.And(it => it.BluetoothFirmwareVersion == query.MatchingBluetoothFirmwareVersion);
            }

            var coll = Collection(tenant);

            IQueryable<DeviceDataModel> data = null;

            if (string.IsNullOrWhiteSpace(query.OrderField))
            {
                data = coll.AsQueryable().AsExpandable().Where(pred);
            }
            else
            {
                PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(typeof(DeviceDataModel)).Find(query.OrderField, true);

                if (propertyDescriptor == null)
                {
                    data = coll.AsQueryable().AsExpandable().Where(pred);
                }
                else if (query.OrderByAscending)
                {
                    data = coll.AsQueryable().AsExpandable().Where(pred).OrderBy(u => propertyDescriptor.GetValue(u)).Select(u => u);
                }
                else
                {
                    data = coll.AsQueryable().AsExpandable().Where(pred).OrderByDescending(u => propertyDescriptor.GetValue(u)).Select(u => u);
                }
            }


            return Tuple.Create(data, new RepositoryContext { Context = coll });
        }

        public IList<DeviceViewModel> GenerateTestData(int itemCount, int tzOffsetMinutes)
        {
            var rng = GoodSeedRandom.Create();
            return Enumerable.Range(0, itemCount)
                .Select(_ => new DeviceViewModel(rng, tzOffsetMinutes))
                .ToList();

        }

        protected override IMongoCollection<DeviceDataModel> CreateCollection(string tenant)
        {
            return Collection(tenant);
        }

        public void Save(DeviceDataModel dm, string tenant)
        {
            Collection(tenant).ReplaceOne(Builders<DeviceDataModel>.Filter.Eq(it => it.Id, dm.Id),
                dm, new ReplaceOptions() { IsUpsert = true });
        }

        public void Save(IEnumerable<DeviceDataModel> dm, string tenant)
        {
            var coll = Collection(tenant);
            foreach (var dev in dm)
            {
                coll.ReplaceOne(Builders<DeviceDataModel>.Filter.Eq(it => it.Id, dev.Id),
                    dev, new ReplaceOptions() { IsUpsert = true });
            }
        }

    }

}
