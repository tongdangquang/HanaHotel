using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;

namespace HanaHotel.DataAccessLayer.Repositories
{
    public class GenericRepository<T> : IGenericDal<T> where T : class
    {
        private readonly DataContext _context;

        public GenericRepository(DataContext dataContext)
        {
            _context = dataContext;
        }
        public void Delete(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
        }

        public T GetByID(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public List<T> GetList()
        {
            return _context.Set<T>().ToList();
        }

        public void Insert(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();

        }

        public void Update(T entity)
        {
            _context.Update(entity);
            _context.SaveChanges();
        }
    }
}