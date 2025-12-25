using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Abstract
{
    public interface IImageService : IGenericService<Image>
    {
        // optional helper
        List<Image> GetImagesByRoomId(int roomId);
    }
}
