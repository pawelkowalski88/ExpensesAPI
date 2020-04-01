using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public interface IUnitOfWork
    {
        Task CompleteAsync();
    }
}
