using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
using SmogDetector.DataAccess.IRepositories;

namespace SmogDetector.DataAccess.Repositories
{
    /// <summary>
    /// A generic class for operations on a database
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Database context
        /// </summary>
        protected readonly SmogDetectorModel Context;

        /// <summary>
        /// Database table representation
        /// </summary>
        protected readonly DbSet<TEntity> DbSet;

        /// <summary>
        /// Creates a new instance of <see cref="GenericRepository{TEntity}"/>
        /// </summary>
        public GenericRepository()
        {
            Context = new SmogDetectorModel();
            DbSet = Context.Set<TEntity>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="GenericRepository{TEntity}"/>
        /// </summary>
        /// <param name="context">Context</param>
        public GenericRepository(SmogDetectorModel context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        /// <inheritdoc />
        public IQueryable<TEntity> GetQueryable() => DbSet;

        /// <inheritdoc />
        public void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            DbSet.AddOrUpdate(entity);
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        /// <inheritdoc />
        public int Save()
        {
            return Context.SaveChanges();
        }

        /// <inheritdoc />
        public void BulkInsert(IEnumerable<TEntity> entities)
        {
            Context.BulkInsert(entities);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context.Dispose();
            }
        }

        /// <inheritdoc />
        ~GenericRepository()
        {
            Dispose(false);
        }
    }
}
