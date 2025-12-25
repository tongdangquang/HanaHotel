using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Abstract
{
    public interface IContactService : IGenericService<Contact>
    {
        List<Contact> TGetRepliedContactsCount();
        List<Contact> TGetUnRepliedContactCount();
        List<Contact> TGetSpesificCategoryContacts(int categoryId);
        List<Contact> TGetContactWithCategory();

    }
}
