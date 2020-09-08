using AppComponents;
using AppComponents.Extensions.EnumerableEx;
using AppComponents.Extensions.EnumEx;
using DomainModel.CAST.TenantConfig;
using DomainModel.Common;
using DomainModel.Core.API;
using DomainModel.Core.EventHandlers.ViewModelBuilders;
using DomainModel.Core.Sensors;
using DomainModel.SiteOperations;
using DomainModel.SiteOperations.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using TACAzure.Data.Tables;

namespace DomainModel.CAST
{
    public class DeviceLogic
    {
        private DeviceRepository _repo = new DeviceRepository();
        private TagCountsLogic _tagCountLogic = new TagCountsLogic();
        private CustomerAndServicePlanRepository _customerRepository = new CustomerAndServicePlanRepository();

        

        public void AddTagsToDevice(Guid id, IEnumerable<string> tags, string tenant)
        {
            var counts = AddTagsToDevice(tags, _repo.Load(id, tenant));
            _tagCountLogic.CountTagsAdded(TagCountType.Device, counts, tenant);
        }

        private IEnumerable<Tuple<string, int>> AddTagsToDevice(IEnumerable<string> tags, Tuple<DeviceDataModel, RepositoryContext> data)
        {
            var device = data.Item1;
            var counts = new List<Tuple<string, int>>();
            bool changed = false;

            foreach (var tag in tags)
            {
                var tg = TagUtility.MakeTag(tag);
                if (!device.Tags.Contains(tg))
                {
                    changed = true;
                    counts.Add(Tuple.Create(tg, 1));
                    device.Tags.Add(tg);
                }
            }

            if (changed)
                _repo.Save(data);

            return counts;
        }

        public void AddTagsToDevices(IEnumerable<Guid> ids, IEnumerable<string> tags, string tenant)
        {
            var devices = _repo.LoadMany(ids, tenant);
            foreach (var device in devices.Item1)
            {
                AddTagsToDevice(tags, Tuple.Create(device, devices.Item2));
            }
        }

        public void AddTagToDevice(string sn, IEnumerable<string> tags, string tenant) 
        {
            var device = _repo.LoadBySerialNumber(sn, tenant);
            AddTagsToDevice(tags, Tuple.Create(device.Item1, device.Item2));
        }

        public void AddTagsToDeviceQuery(DevicesQuery query, IEnumerable<string> tags, string tenant)
        {
            var devices = _repo.QuerySysDevices(query, tenant);
            var context = devices.Item2;

            foreach (var device in devices.Item1)
            {
                AddTagsToDevice(tags, Tuple.Create(device, context));
            }
        }

        public void RemoveTagsFromDevice(Guid id, IEnumerable<string> tags, string tenant)
        {
            RemoveTagsFromDevice(tags, _repo.Load(id, tenant));
        }

        private IEnumerable<Tuple<string, int>> RemoveTagsFromDevice(IEnumerable<string> tags, Tuple<DeviceDataModel, RepositoryContext> data)
        {
            var counts = new List<Tuple<string, int>>();

            var device = data.Item1;
            bool changed = false;
            foreach (var tag in tags)
            {
                var tg = TagUtility.MakeTag(tag);
                if (device.Tags.Contains(tg))
                {
                    counts.Add(Tuple.Create(tg, 1));
                    changed = true;
                    device.Tags.Remove(tg);
                }
            }

            if (changed)
            {
                _repo.Save(data);
            }

            return counts;

        }

        public void RemoveTagsFromDevices(IEnumerable<Guid> ids, IEnumerable<string> tags, string tenant)
        {

            var devices = _repo.LoadMany(ids, tenant);

            foreach (var device in devices.Item1)
            {
                var deviceCounts = RemoveTagsFromDevice(tags, Tuple.Create(device, devices.Item2));
                _tagCountLogic.CountTagsRemoved(TagCountType.Device, deviceCounts, tenant);
            }
        }

        public void RemoveTagsUsingDeviceQuery(DevicesQuery query, IEnumerable<string> tags, string tenant)
        {
            var devices = _repo.QuerySysDevices(query, tenant);
            var context = devices.Item2;

            foreach (var device in devices.Item1)
            {
                RemoveTagsFromDevice(tags, Tuple.Create(device, context));
            }
        }

        public void ReplaceTagsOnDevice(Guid id, string oldTag, string newTag, string tenant)
        {
            int oldCount, newCount;
            ReplaceTagOnDevice(oldTag, newTag, _repo.Load(id, tenant), out oldCount, out newCount);
            _tagCountLogic.CountTagRemoved(TagCountType.Device, oldTag, oldCount, tenant);
            _tagCountLogic.CountTagAdded(TagCountType.Device, newTag, newCount, tenant);
        }

        private void ReplaceTagOnDevice(string oldTag, string newTag, Tuple<DeviceDataModel, RepositoryContext> data, out int oldCount, out int newCount)
        {
            var device = data.Item1;
            bool changed = false;

            var oldTg = TagUtility.MakeTag(oldTag);
            var newTg = TagUtility.MakeTag(newTag);
            oldCount = 0;
            newCount = 0;

            if (device.Tags.Contains(oldTg))
            {
                oldCount += 1;
                changed = true;
                device.Tags.Remove(oldTg);
                if (!device.Tags.Contains(newTg))
                {
                    device.Tags.Add(newTg);
                    newCount += 1;
                }
            }

            if (changed)
                _repo.Save(data);
        }

        public void ReplaceTagsOnDevices(IEnumerable<Guid> ids, string oldTag, string newTag, string tenant)
        {
            var devices = _repo.LoadMany(ids, tenant);
            foreach (var device in devices.Item1)
            {
                int oldCount, newCount;
                ReplaceTagOnDevice(oldTag, newTag, Tuple.Create(device, devices.Item2), out oldCount, out newCount);
                _tagCountLogic.CountTagRemoved(TagCountType.Device, oldTag, oldCount, tenant);
                _tagCountLogic.CountTagAdded(TagCountType.Device, newTag, newCount, tenant);
            }
        }

        public void ReplaceTagsOnDevicesQuery(DevicesQuery query, string oldTag, string newTag, string tenant)
        {
            var devices = _repo.QuerySysDevices(query, tenant);
            var context = devices.Item2;

            foreach (var device in devices.Item1)
            {
                int oldCount, newCount;
                ReplaceTagOnDevice(oldTag, newTag, Tuple.Create(device, context), out oldCount, out newCount);
                _tagCountLogic.CountTagRemoved(TagCountType.Device, oldTag, oldCount, tenant);
                _tagCountLogic.CountTagAdded(TagCountType.Device, newTag, newCount, tenant);
            }
        }

        public class CreateDeviceRecordCommand
        {
            public string SerialNumber { get; set; }
            public string ICCID { get; set; }
            public string CustomerId { get; set; }
            public bool ForeignSIM { get; set; }
            public string SimSource { get; set; }
        }

        public void CreateDeviceRecord(CreateDeviceRecordCommand command)
        {
            var dcc = DevicePurchaseRepository
                .RecordDeviceDetails(
                command.SerialNumber, 
                command.CustomerId, 
                command.ICCID, 
                command.ForeignSIM, DateTime.UtcNow,
                command.SimSource);

            var purchaserName = DevicePurchaseRepository.LookupPurchaserName(command.CustomerId);

            DevicePurchaseRepository.AssignDeviceToCAST(command.CustomerId, dcc);

            

            var dhs = DeviceHealthScoreRepository.Fetch(command.SerialNumber);
            if (null != dhs)
            {
                dhs.CustomerId = command.CustomerId;
                dhs.CustomerName = purchaserName;
                dhs.SimSource = command.SimSource;
                DeviceHealthScoreRepository.UpsertImmediate(EnumerableEx.OfOne(dhs));
               
            } 

            var subs = GetSubscribers().Where(it => it.PurchaserId == command.CustomerId && !it.SubKey).ToDictionary(it => it.PurchaserId);
            if (subs.ContainsKey(command.CustomerId))
            {
                var sub = subs[command.CustomerId];
                MaybeAssignDeviceToApiKey(dcc.SerialNumber, sub.APIKey);
               
            }
        }

        public static string LookupPurchaserName(string id)
        {
            var coll = DevicePurchaseRepository.PurchaserCollection();
            var purchaser = coll
                .FindSync(Builders<DevicePurchaserViewModel>.Filter.Eq(it => it.CustomerNumber, id))
                .SingleOrDefault();
            string purchaserName = null;
            if (null != purchaser) purchaserName = purchaser.CompanyName;
            return purchaserName;
        }

       

        private static void MaybeAssignDeviceToApiKey(string sensorId, string apiKey)
        {
            var apiSensor = ApiSensorAssociationViewModel.AccessAssociation();
            var existing = apiSensor.Get(property: AssociationVerbs.Hosts.EnumName(), value: sensorId);
            foreach (var item in existing)
            {
                apiSensor.Delete(item);
            }

            apiSensor.Put(apiKey, AssociationVerbs.Hosts.EnumName(), sensorId);

        }

        private IList<APISubscriber> GetSubscribers()
        {
            CloudTableEx<TEA<APISubscriber>> _table = APISubscriberTableSchema.Table();



            return _table.GetAllUnSafe().Select(it => it.Value).ToList();

        }

        public string GetTenantFromCustomer(string customerNumber)
        {
            string retval = "";
            if (!string.IsNullOrEmpty(customerNumber))
            {
                try
                {
                    Tenant tenant = TenantEnrollment.FetchTenant(customerNumber);
                    retval = tenant.TenantId;
                }
                catch(Exception ex)
                {

                }
            }
            return retval;
        }
    }
}