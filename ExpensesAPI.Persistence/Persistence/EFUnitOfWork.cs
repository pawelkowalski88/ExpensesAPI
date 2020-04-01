using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly MainDbContext context;

        public EFUnitOfWork(MainDbContext context)
        {
            this.context = context;
        }
        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
