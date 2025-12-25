using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HanaHotel.DataAccessLayer.Concrete
{
    public class EfImageDAL : IImageDal
    {
        private readonly DataContext _context;

        public EfImageDAL(DataContext context)
        {
            _context = context;
        }

        public void Insert(Image entity) { _context.Images.Add(entity); _context.SaveChanges(); }
        public void Delete(Image entity) { _context.Images.Remove(entity); _context.SaveChanges(); }
        public void Update(Image entity) { _context.Images.Update(entity); _context.SaveChanges(); }
        public List<Image> GetList() => _context.Images.AsNoTracking().ToList();
        public Image GetByID(int id) => _context.Images.Find(id);
    }
}
