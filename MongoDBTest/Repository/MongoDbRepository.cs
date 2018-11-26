﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBTest
{
    public class MongoDbRepository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
    {
        private MongoDatabase database;
        private MongoCollection<TEntity> collection;

        public MongoDbRepository()
        {
            GetDatabase();
            GetCollection();
        }

        public WriteConcernResult Insert(TEntity entity)
        {
            entity.Id = ObjectId.GenerateNewId().ToString();
            WriteConcernResult result = collection.Insert(entity);
            return result;
        }

        public WriteConcernResult Update(TEntity entity)
        {
            if (entity.Id == null)
                return Insert(entity);

            WriteConcernResult result = collection
                 .Save(entity);
            return result;

        }

        public WriteConcernResult Delete(TEntity entity)
        {
            WriteConcernResult result = collection
                .Remove(Query.EQ("_id", entity.Id));
            return result;

        }

        public IList<TEntity>
            SearchFor(Expression<Func<TEntity, bool>> predicate)
        {
            return collection
                .AsQueryable<TEntity>()
                    .Where(predicate.Compile())
                        .ToList();
        }

        public IList<TEntity> GetAll()
        {
            return collection.FindAllAs<TEntity>().ToList();
        }

        public TEntity GetById(string id)
        {
            return collection.FindOneByIdAs<TEntity>(id);
        }

        #region Private Helper Methods
        private void GetDatabase()
        {
            var client = new MongoClient(GetConnectionString());

            var server = client.GetServer();

            database = server.GetDatabase(GetDatabaseName());
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.AppSettings .Get("MongoDbConnectionString") .Replace("{DB_NAME}", GetDatabaseName());
        }

        private string GetDatabaseName()
        {
            return ConfigurationManager.AppSettings .Get("MongoDbDatabaseName");
        }

        private void GetCollection()
        {
            collection = database
                .GetCollection<TEntity>(typeof(TEntity).Name);
        }
        #endregion
    }
}