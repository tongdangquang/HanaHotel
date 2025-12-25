using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.DataAccessLayer.Abstract
{
    public interface IContactDal : IGenericDal<Contact>
    {
        List<Contact> GetRepliedContactsCount();
        List<Contact> GetUnRepliedContactCount();
        List<Contact> GetSpesificCategoryContacts(int categoryId);

        List<Contact> GetContactWithCategory();
    }
}
