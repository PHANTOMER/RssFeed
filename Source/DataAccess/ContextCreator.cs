using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using NLog;

namespace Core.DataAccess
{
    static class ContextExtensions
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this IMongoDatabase database,
            string collectionName)
        {
            var result = await database.GetCollection<TEntity>(collectionName).FindAsync(FilterDefinition<TEntity>.Empty);
            return await result.ToListAsync();
        }

        public static async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(this IMongoDatabase database,
            Expression<Func<TEntity, bool>> predicate, string collectionName)
        {
            var result = await database.GetCollection<TEntity>(collectionName).FindAsync(predicate);
            return await result.ToListAsync();
        }

        public static async Task<TEntity> GetAsync<TEntity>(this IMongoDatabase database,
            Expression<Func<TEntity, bool>> predicate, string collectionName)
        {
            var result = await database.GetCollection<TEntity>(collectionName).FindAsync(predicate);
            return await result.FirstOrDefaultAsync();
        }

        public static async Task<bool> SaveAsync<TEntity>(this IMongoDatabase database,
            IEnumerable<TEntity> entities, string collectionName)
        {
            try
            {
                await database.GetCollection<TEntity>(collectionName).InsertManyAsync(entities);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public static async Task<bool> SaveAsync<TEntity>(this IMongoDatabase database,
            TEntity entity, string collectionName)
        {
            try
            {
                await database.GetCollection<TEntity>(collectionName).InsertOneAsync(entity);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }

        public static async Task<bool> UpdateAsync<TEntity>(this IMongoDatabase database,
           UpdateOneModel<TEntity> update, string collectionName)
        {
            try
            {
                await database.GetCollection<TEntity>(collectionName).UpdateOneAsync(update.Filter, update.Update);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return false;
        }
    }

    static class ContextCreatorExtensions
    {
        public static async Task<TResult> FromContext<TResult>(
            this ContextCreator contextCreator, 
            Func<IMongoDatabase, Task<TResult>> action)
        {
            var db = contextCreator.GetDatabase();
            return await action(db);
        }
    }

    class ContextCreator
    {
        private readonly string _connectionString;

        public ContextCreator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IMongoDatabase GetDatabase()
        {
            MongoUrlBuilder builder = new MongoUrlBuilder(_connectionString);

            // connect to local host
            MongoClient client = new MongoClient(_connectionString);

            // get DB
            return client.GetDatabase(builder.DatabaseName);
        }

        public IMongoDatabase GetDatabase(string connectionString)
        {
            MongoUrlBuilder builder = new MongoUrlBuilder(connectionString);

            // connect to local host
            MongoClient client = new MongoClient(connectionString);

            // get DB
            return client.GetDatabase(builder.DatabaseName);
        }
        

        //public static MongoCollection<T> GetCollection<T>(string CollectionName, string dbName)
        //{
        //    // get db
        //    var db = MongoHelper.GetDB(dbName);

        //    // get collection
        //    return db.GetCollection<T>(CollectionName);
        //}

        //public static T GetDocument<T>(string id, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    return collection.FindOneById(id);
        //}

        //public static T GetDocument<T>(int id, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    return collection.FindOneById(id);
        //}

        //public static long DeleteDocuments<T>(IMongoQuery query, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    var result = collection.Remove(query);
        //    return result.DocumentsAffected;
        //}

        //public static T FindOneDocument<T>(IMongoQuery query, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    return collection.FindOne(query);
        //}

        //public static List<T> FindDocuments<T>(IMongoQuery query, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    var cursor = collection.FindAs<T>(query);

        //    return cursor != null && Enumerable.Any<T>(cursor) ? Enumerable.ToList<T>(cursor) : null;
        //}

        //public static void SaveDocument<T>(T document, string collectionName, string dbName)
        //{
        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    collection.Save(document);
        //}

        //public static void CreateTTLIndex<T>(int ttl, string ttlField, string collectionName, string dbName)
        //{
        //    // set index builder options
        //    IndexOptionsBuilder indexOpt = new IndexOptionsBuilder();

        //    indexOpt = indexOpt.SetBackground(true).SetTimeToLive(new TimeSpan(0, 0, ttl));

        //    IndexKeysBuilder keys1 = IndexKeys.Ascending(ttlField);

        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    if (!collection.IndexExists(keys1))
        //        collection.CreateIndex(keys1, indexOpt);
        //}

        //public static void CreateIndexAsc<T>(string[] indexFields, string collectionName, string dbName)
        //{
        //    // set index builder options
        //    IndexOptionsBuilder indexOpt = new IndexOptionsBuilder();
        //    indexOpt.SetBackground(true);

        //    IndexKeysBuilder keys1 = IndexKeys.Ascending(indexFields);

        //    var collection = MongoHelper.GetCollection<T>(collectionName, dbName);
        //    if (!collection.IndexExists(keys1))
        //        collection.CreateIndex(keys1, indexOpt);
        //}
    }
}
