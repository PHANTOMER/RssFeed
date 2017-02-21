using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Core.DataAccess
{
    public interface IDbContext
    {
        IEnumerable<TEntity> GetAll<TEntity>();

        IEnumerable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicate);

        bool Save<TEntity>(IEnumerable<TEntity> entities);

        bool Save<TEntity>(TEntity entity);

        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(string collectionName);

        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, string collectionName);

        Task<TEntity> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, string collectionName);

        Task<bool> SaveAsync<TEntity>(IEnumerable<TEntity> entities, string collectionName);

        Task<bool> SaveAsync<TEntity>(TEntity entity, string collectionName);

        Task<bool> UpdateAsync<TEntity>(UpdateOneModel<TEntity> update, string collectionName);
    }

    class MongoContext : IDbContext
    {
        private readonly ContextCreator _contextCreator;

        public MongoContext(ContextCreator contextCreator)
        {
            _contextCreator = contextCreator;
        }

        public IEnumerable<TEntity> GetAll<TEntity>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public bool Save<TEntity>(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public bool Save<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(string collectionName)
        {
            return await _contextCreator.FromContext(c => c.GetAllAsync<TEntity>(collectionName));
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, string collectionName)
        {
            return await _contextCreator.FromContext(c => c.GetAllAsync(predicate, collectionName));
        }

        public async Task<TEntity> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, string collectionName)
        {
            return await _contextCreator.FromContext(c => c.GetAsync(predicate, collectionName));
        }

        public async Task<bool> SaveAsync<TEntity>(IEnumerable<TEntity> entities, string collectionName)
        {
            return await _contextCreator.FromContext(c => c.SaveAsync(entities, collectionName));
        }

        public async Task<bool> SaveAsync<TEntity>(TEntity entity, string collectionName)
        {
            return await _contextCreator.FromContext(c => c.SaveAsync(entity, collectionName));
        }

        public async Task<bool> UpdateAsync<TEntity>(UpdateOneModel<TEntity> update, string collectionName)
        {
            return await _contextCreator.FromContext(c => c.UpdateAsync(update, collectionName));
        }
    }
}
