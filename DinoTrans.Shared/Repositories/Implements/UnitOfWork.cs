using DinoTrans.Shared.Data;
using DinoTrans.Shared.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.Repositories.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        public DinoTransDbContext Context { get; }
        public UnitOfWork(DinoTransDbContext context)
        {
            Context = context;
        }
        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public void BeginTransaction()
        {
            if (Context.Database.CurrentTransaction != null)
            {
                return;
            }
            Context.Database.BeginTransaction();
        }

        public void Commit()
        {
            if (Context.Database.CurrentTransaction == null)
            {
                return;
            }
            Context.Database.CommitTransaction();
        }

        public void Rollback()
        {
            if (Context.Database.CurrentTransaction == null)
            {
                return;
            }
            Context.Database.RollbackTransaction();
        }
    }
}
