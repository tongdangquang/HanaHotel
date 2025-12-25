using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class ContactService : IContactService
    {
        private readonly IContactDal _contactDal;

        public ContactService(IContactDal contactDal)
        {
            _contactDal = contactDal;
        }

        public List<Contact> TGetRepliedContactsCount()
        {
            return _contactDal.GetRepliedContactsCount();
        }

        public List<Contact> TGetUnRepliedContactCount()
        {
            return _contactDal.GetUnRepliedContactCount();
        }

        public void TDelete(Contact entity)
        {
            _contactDal.Delete(entity);
        }

        public Contact TGetByID(int id)
        {
            return _contactDal.GetByID(id);
        }

        public List<Contact> TGetList()
        {
            return _contactDal.GetList();
        }

        public void TInsert(Contact entity)
        {
            _contactDal.Insert(entity);
        }

        public void TUpdate(Contact entity)
        {
            _contactDal.Update(entity);
        }

        public List<Contact> TGetSpesificCategoryContacts(int categoryId)
        {
            return _contactDal.GetSpesificCategoryContacts(categoryId);
        }

        public List<Contact> TGetContactWithCategory()
        {
            return _contactDal.GetContactWithCategory();
        }
    }
}