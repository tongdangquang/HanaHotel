using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class MessageCategoryService : IMessageCategoryService
    {
        private readonly IMessageCategoryDal _messageCategoryDal;

        public MessageCategoryService(IMessageCategoryDal messageCategoryDal)
        {
            _messageCategoryDal = messageCategoryDal;
        }

        public void TDelete(MessageCategory entity)
        {
            _messageCategoryDal.Delete(entity);
        }

        public MessageCategory TGetByID(int id)
        {
            return _messageCategoryDal.GetByID(id);
        }

        public List<MessageCategory> TGetList()
        {
            return _messageCategoryDal.GetList();
        }

        public void TInsert(MessageCategory entity)
        {
            _messageCategoryDal.Insert(entity);
        }

        public void TUpdate(MessageCategory entity)
        {
            _messageCategoryDal.Update(entity);
        }
    }
}