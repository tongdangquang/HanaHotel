using HanaHotel.DtoLayer.DTOs.RoomDTO;
namespace HanaHotel.DtoLayer.DTOs.ServiceDTO
{
    public class ResultServiceDTO
    {
        public int Id { get; set; }

        public string ServiceName { get; set; }

        public double Price { get; set; }

        public string Unit { get; set; }

        public string? Description { get; set; }
        public List<ResultRoomDTO> Rooms { get; set; }

    }
}
