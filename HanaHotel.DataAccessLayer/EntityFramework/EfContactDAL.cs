using Microsoft.EntityFrameworkCore;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.Repositories;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.EntityFramework
{
    public class EfContactDAL : GenericRepository<Contact>, IContactDal
    {
        private readonly DataContext _context;

        public EfContactDAL(DataContext dataContext) : base(dataContext)
        {
            _context = dataContext;
        }

        public List<Contact> GetRepliedContactsCount()
        {
            return [.. _context.Set<Contact>().Where(item => item.IsReplied)];
        }

        public List<Contact> GetSpesificCategoryContacts(int categoryId)
        {
            return [.. _context.Set<Contact>().Where(item => item.MessageCategoryId == categoryId)];
        }

        public List<Contact> GetUnRepliedContactCount()
        {
            return [.. _context.Set<Contact>().Where(item => !item.IsReplied)];

        }


        public new List<Contact> GetContactWithCategory()
        {
            return [.. _context.Set<Contact>().Include(c => c.MessageCategory)];
        }

    }
}
