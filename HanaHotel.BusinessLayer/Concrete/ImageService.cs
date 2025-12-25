using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class ImageService : IImageService
    {
        private readonly IImageDal _imageDal;

        public ImageService(IImageDal imageDal)
        {
            _imageDal = imageDal;
        }

        public void TDelete(Image entity) => _imageDal.Delete(entity);
        public Image TGetByID(int id) => _imageDal.GetByID(id);
        public List<Image> TGetList() => _imageDal.GetList();
        public void TInsert(Image entity) => _imageDal.Insert(entity);
        public void TUpdate(Image entity) => _imageDal.Update(entity);
        public List<Image> GetImagesByRoomId(int roomId) => _imageDal.GetList().Where(i => i.RoomId == roomId).ToList();
    }
}
