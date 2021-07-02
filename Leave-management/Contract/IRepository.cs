using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Contract
{
    public interface IRepository<T> where T : class
    {
        ICollection<T> FindAll();
        bool isExists(int id);
        T FindById(int id);
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        bool Save();
    }
}
