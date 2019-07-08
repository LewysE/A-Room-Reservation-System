using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calendar.Data.Interfaces
{
    public interface IRepo<T> where T:class
    {
        Task<List<T>> GetAll();

        List<T> Find(Func<T, bool> predicate);

        Task<T> GetById(int? id);

        Task Add(T entity);

        Task Update(T entity);

        Task Delete(T entity);
    }
}
