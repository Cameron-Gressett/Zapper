using DomainModel.Common;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace DomainModel.CAST
{

    /// <summary>
    /// Empowers CASTMongoRepository[T] to use
    /// the Id property for any entity using it
    /// </summary>
    public interface ICASTDataModel
    {
        Guid Id { get; set; }
    }

    /// <summary>
    /// Base class for mongo db repository with common
    /// functions implemented for loading, saving, and deleting
    /// data in mongo db.
    /// Assumes the entity must implement ICASTDataModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CASTMongoRepository<T> where T:class, ICASTDataModel
    {
        protected abstract IMongoCollection<T> CreateCollection(string tenant);
        
        public virtual void Create(T newItem, string tenant)
        {
            var coll = CreateCollection(tenant);
            coll.InsertOne(newItem);
        }

        public virtual Tuple<List<T>, RepositoryContext> GetPage(string tenant, int count = 100)
        {
            var collection = CreateCollection(tenant);
            var items = collection.AsQueryable().Take(count).ToList();
            return Tuple.Create(items, new RepositoryContext { Context = collection });
        }

        public virtual Tuple<T, RepositoryContext> Load(Guid id, string tenant)
        {
            var collection = CreateCollection(tenant);
            var ddm = collection.FindSync(Builders<T>.Filter.Eq("Id", id)).SingleOrDefault();
            return Tuple.Create(ddm, new RepositoryContext { Context = collection });
        }

        public virtual Tuple<List<T>, RepositoryContext> LoadMany(IEnumerable<Guid> ids, string tenant)
        {
            var collection = CreateCollection(tenant);
            
            var items = collection.AsQueryable().Where(it => ids.Contains(it.Id)).ToList();
            return Tuple.Create(items, new RepositoryContext { Context = collection });
        }

        public virtual void Save(Tuple<T, RepositoryContext> data)
        { 
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.ReplaceOne(Builders<T>.Filter.Eq("Id", data.Item1.Id), data.Item1,
                new ReplaceOptions() {IsUpsert = true});
            
        }

        public virtual void Save(Tuple<T, RepositoryContext> data, string tenant)
        {
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.ReplaceOne(Builders<T>.Filter.Eq("Id", data.Item1.Id), data.Item1,
                new ReplaceOptions() { IsUpsert = true });

        }

        public virtual void SaveOver(T data, string tenant)
        {
            var collection = CreateCollection(tenant);
            collection.ReplaceOne(Builders<T>.Filter.Eq("Id", data.Id), data,
                new ReplaceOptions() { IsUpsert = true });
        }

        public virtual void Delete(Tuple<T, RepositoryContext> data)
        {
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.FindOneAndDelete(Builders<T>.Filter.Eq("Id", data.Item1.Id));
            
        }

        public virtual void Delete(Tuple<T, RepositoryContext> data, string tenant)
        {
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.FindOneAndDelete(Builders<T>.Filter.Eq("Id", data.Item1.Id));

        }

        public virtual void Delete(Guid id, string tenant)
        {
            var collection = CreateCollection(tenant);
            collection.FindOneAndDelete(Builders<T>.Filter.Eq("Id", id));
        }




    }
}
