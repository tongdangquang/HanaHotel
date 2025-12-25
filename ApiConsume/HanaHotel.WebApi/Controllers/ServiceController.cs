//using Microsoft.AspNetCore.Mvc;
//using HanaHotel.BusinessLayer.Abstract;
//using HanaHotel.EntityLayer.Concrete;

//namespace HanaHotel.WebApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ServiceController : ControllerBase
//    {
//        private readonly IServiceService _serviceService;

//        public ServiceController(IServiceService serviceService)
//        {
//            _serviceService = serviceService;
//        }

//        [HttpGet]
//        public IActionResult GetServices()
//        {
//            var services = _serviceService.TGetList();
//            return Ok(services);
//        }

//        [HttpGet("{id}")]
//        public IActionResult GetService(int id)
//        {
//            var service = _serviceService.TGetByID(id);
//            return Ok(service);
//        }

//        [HttpPut]
//        public IActionResult UpdateService(Service service)
//        {
//            _serviceService.TUpdate(service);
//            return Ok();
//        }

//        [HttpPost]
//        public IActionResult AddService(Service service)
//        {
//            _serviceService.TInsert(service);
//            return Ok();
//        }

//        [HttpDelete("{id}")]
//        public IActionResult DeleteService(int id)
//        {
//            var service = _serviceService.TGetByID(id);
//            if (service == null)
//            {
//                return NotFound();
//            }

//            _serviceService.TDelete(service);
//            return NoContent();
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DtoLayer.DTOs.ServiceDTO;
namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly DataContext _context;

        // Constructor Injection for dependencies
        public ServiceController(IServiceService serviceService, DataContext context)
        {
            _serviceService = serviceService;
            _context = context;
        }

        // ============================ 
        // GET ALL SERVICES
        // ============================ 
        [HttpGet]
        public IActionResult GetServices()
        {
            var services = _context.Services
                .Include(s => s.RoomServices)
                .ThenInclude(rs => rs.Room)
                .Select(s => new
                {
                    s.Id,
                    s.ServiceName,
                    s.Price,
                    s.Unit,
                    s.Description,
                    Rooms = s.RoomServices.Select(r => new { r.Room.Id, r.Room.RoomName }).ToList()
                })
                .ToList();

            return Ok(services);
        }

        // ============================ 
        // GET SERVICE BY ID
        // ============================ 
        [HttpGet("{id}")]
        public IActionResult GetService(int id)
        {
            var service = _context.Services
                .Include(s => s.RoomServices)
                .ThenInclude(rs => rs.Room)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.ServiceName,
                    s.Price,
                    s.Unit,
                    s.Description,
                    Rooms = s.RoomServices.Select(r => new { r.Room.Id, r.Room.RoomName }).ToList()
                })
                .FirstOrDefault();

            if (service == null)
                return NotFound();

            return Ok(service);
        }

        // ============================ 
        // CREATE SERVICE
        // ============================ 
        [HttpPost]
        public IActionResult AddService([FromBody] CreateServiceDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var service = new Service
            {
                ServiceName = dto.ServiceName,
                Price = dto.Price,
                Unit = dto.Unit,
                Description = dto.Description,
                ServiceIcon = dto.ServiceIcon ?? "default-icon.png"
            };

            _context.Services.Add(service);
            _context.SaveChanges();

            // thêm service-detail
            if (dto.RoomIds != null)
            {
                foreach (var roomId in dto.RoomIds)
                {
                    _context.ServiceDetails.Add(new ServiceDetail
                    {
                        ServiceId = service.Id,
                        RoomId = roomId
                    });
                }
                _context.SaveChanges();
            }

            return Ok(service);
        }


        // ============================ 
        // UPDATE SERVICE
        // ============================ 
        [HttpPut("{id}")]
        public IActionResult UpdateService(int id, [FromBody] UpdateServiceDTO dto)
        {
            if (dto == null)
                return BadRequest("Dữ liệu không hợp lệ");

            if (id != dto.Id)
                return BadRequest("ID không khớp");

            var service = _context.Services
                .Include(s => s.RoomServices)
                .FirstOrDefault(s => s.Id == id);

            if (service == null)
                return NotFound("Không tìm thấy dịch vụ");

            // Cập nhật thông tin cơ bản
            service.ServiceName = dto.ServiceName;
            service.Price = dto.Price;
            service.Unit = dto.Unit;
            service.Description = dto.Description;

            // ---- Sửa duy nhất ở đây ----
            // Xóa RoomServices cũ
            if (service.RoomServices != null)
            {
                _context.RemoveRange(service.RoomServices);
            }

            // Thêm danh sách RoomId mới
            if (dto.RoomIds != null)
            {
                foreach (var roomId in dto.RoomIds)
                {
                    _context.Add(new ServiceDetail
                    {
                        RoomId = roomId,
                        ServiceId = service.Id
                    });
                }
            }
            // ---- hết phần sửa ----

            _context.SaveChanges();

            return Ok("Cập nhật dịch vụ thành công");
        }



        // ============================ 
        // DELETE SERVICE
        // ============================ 
        [HttpDelete("{id}")]
        public IActionResult DeleteService(int id)
        {
            var service = _serviceService.TGetByID(id);
            if (service == null)
                return NotFound();

            _serviceService.TDelete(service);
            return NoContent(); // Return 204 No Content after successful delete
        }
    }
}


