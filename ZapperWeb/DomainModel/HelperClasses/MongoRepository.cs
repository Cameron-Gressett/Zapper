using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace DomainModel.HelperClasses
{

    /// <summary>
    /// Empowers CASTMongoRepository[T] to use
    /// the Id property for any entity using it
    /// </summary>
    public interface IDataModel
    {
        Guid Id { get; set; }
    }
    public class RepositoryContext
    {
        public object Context { get; set; }
    }

    public abstract class MongoRepository<T> where T : class, IDataModel
    {
        protected abstract IMongoCollection<T> CreateCollection();
        public virtual void Create(T newItem)
        {
            var coll = CreateCollection();
            coll.InsertOne(newItem);
        }

        public virtual Tuple<List<T>, RepositoryContext> GetPage(string tenant, int count = 100)
        {
            var collection = CreateCollection();
            var items = collection.AsQueryable().Take(count).ToList();
            return Tuple.Create(items, new RepositoryContext { Context = collection });
        }

        public virtual Tuple<T, RepositoryContext> Load(Guid id, string tenant)
        {
            var collection = CreateCollection();
            var ddm = collection.FindSync(Builders<T>.Filter.Eq("Id", id)).SingleOrDefault();
            return Tuple.Create(ddm, new RepositoryContext { Context = collection });
        }

        public virtual Tuple<List<T>, RepositoryContext> LoadMany(IEnumerable<Guid> ids, string tenant)
        {
            var collection = CreateCollection();

            var items = collection.AsQueryable().Where(it => ids.Contains(it.Id)).ToList();
            return Tuple.Create(items, new RepositoryContext { Context = collection });
        }

        public virtual void Save(Tuple<T, RepositoryContext> data)
        {
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.ReplaceOne(Builders<T>.Filter.Eq("Id", data.Item1.Id), data.Item1,
                new ReplaceOptions() { IsUpsert = true });

        }

        public virtual void Save(Tuple<T, RepositoryContext> data, string tenant)
        {
            var collection = (IMongoCollection<T>)data.Item2.Context;
            collection.ReplaceOne(Builders<T>.Filter.Eq("Id", data.Item1.Id), data.Item1,
                new ReplaceOptions() { IsUpsert = true });

        }

        public virtual void SaveOver(T data, string tenant)
        {
            var collection = CreateCollection();
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
            var collection = CreateCollection();
            collection.FindOneAndDelete(Builders<T>.Filter.Eq("Id", id));
        }



    }
}
