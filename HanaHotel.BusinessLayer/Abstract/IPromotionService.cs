using System.Collections.Generic;
using System.Threading.Tasks;
using HanaHotel.DtoLayer;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Abstract
{
    public interface IPromotionService : IGenericService<Promotion>
    {
        Task<List<PromotionListDto>> GetAllAsync();
        Task<PromotionCreateDto?> GetByIdAsync(int id);
        Task AddAsync(PromotionCreateDto dto);
        Task UpdateAsync(int id, PromotionCreateDto dto);
        Task DeleteAsync(int id);
    }
}
