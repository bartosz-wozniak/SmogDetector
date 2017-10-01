using System;
using System.Collections.Generic;
using System.Linq;

namespace SmogDetector.DataAccess.IRepositories
{
    /// <summary>
    /// A generic interface for operations on a database
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> :IDisposable where TEntity : class
    {
        /// <summary>
        /// Returns the query 
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> GetQueryable();

        /// <summary>
        /// Inserts entity into the database
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        void Insert(TEntity entity);

        /// <summary>
        /// Updates or Inserts entity into the database
        /// </summary>
        /// <param name="entity">The entity to Insert/Update</param>
        void Update(TEntity entity);

        /// <summary>
        /// Removes entity from the database
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Saves changes
        /// </summary>
        int Save();

        /// <summary>
        /// Bulk Insert
        /// </summary>
        /// <param name="entities">enities</param>
        void BulkInsert(IEnumerable<TEntity> entities);
    }
}
