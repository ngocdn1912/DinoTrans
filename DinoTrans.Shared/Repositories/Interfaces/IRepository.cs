using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Queryable();
        IQueryable<T> AsNoTracking();
        T Get(object id);
        Task<T> GetAsync(object id);
        T Add(T entity);
        IList<T> AddRange(IList<T> entities);
        T Update(T entity);
        IList<T> UpdateRange(IList<T> entities);
        bool Delete(T entity);
        bool DeleteRange(IList<T> entities);
        int SaveChange();
        Task<int> SaveChangeAsync();
    }
}
