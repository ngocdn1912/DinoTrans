using DinoTrans.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        DinoTransDbContext Context { get; }
        void BeginTransaction();
        void SaveChanges();       
        void Commit();
        void Rollback();
    }
}
