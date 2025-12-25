using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace HanaHotel.BusinessLayer.Concrete
{
    public class PromotionService : IPromotionService
    {
        private readonly DataContext _context;

        public PromotionService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<PromotionListDto>> GetAllAsync()
        {
            var promotions = await _context.Promotions
                .Include(p => p.PromotionDetails)
                .ThenInclude(d => d.Room)        
                .AsNoTracking()
                .ToListAsync();

            return promotions.Select(p => new PromotionListDto
            {
                PromotionId = p.PromotionID,
                PromotionName = p.PromotionName,
                DiscountAmount = p.DiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Details = p.PromotionDetails.Select(d => new PromotionDetailListDto
                {
                    PromotionDetailId = d.PromotionDetailID,
                    RoomId = d.RoomId,
                    RoomName = d.Room.RoomName,  
                    DiscountPercent = d.DiscountPercent
                }).ToList()
            }).ToList();
        }


        public async Task<PromotionCreateDto?> GetByIdAsync(int id)
        {
            var p = await _context.Promotions
                .Include(p => p.PromotionDetails)
                .ThenInclude(pd => pd.Room)
                .FirstOrDefaultAsync(x => x.PromotionID == id);

            if (p == null) return null;

            return new PromotionCreateDto
            {
                PromotionName = p.PromotionName,
                DiscountAmount = p.DiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Details = p.PromotionDetails.Select(d => new PromotionDetailCreateDto
                {
                    RoomId = d.RoomId
                }).ToList()
            };
        }

        public async Task AddAsync(PromotionCreateDto dto)
        {
            // basic validation
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.StartDate > dto.EndDate) throw new ArgumentException("StartDate must be <= EndDate");

            // ensure Details not null
            var details = dto.Details ?? new List<PromotionDetailCreateDto>();

            var promotion = new Promotion
            {
                PromotionName = dto.PromotionName,
                DiscountAmount = dto.DiscountAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PromotionDetails = new List<PromotionDetail>()
            };

            // compute percent per-room
            foreach (var d in details)
            {
                var room = await _context.Rooms.FindAsync(d.RoomId);
                if (room == null) continue; // skip if room missing

                decimal percent = 0m;
                if (room.Price > 0)
                {
                    percent = Math.Round((dto.DiscountAmount / room.Price) * 100m, 2);
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100; // cap to 100
                }

                promotion.PromotionDetails.Add(new PromotionDetail
                {
                    RoomId = d.RoomId,
                    DiscountPercent = percent
                });
            }

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, PromotionCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.StartDate > dto.EndDate) throw new ArgumentException("StartDate must be <= EndDate");

            var existing = await _context.Promotions
                .Include(p => p.PromotionDetails)
                .FirstOrDefaultAsync(p => p.PromotionID == id);

            if (existing == null) throw new KeyNotFoundException("Promotion not found");

            existing.PromotionName = dto.PromotionName;
            existing.DiscountAmount = dto.DiscountAmount;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;

            // remove old details safely
            if (existing.PromotionDetails.Any())
            {
                _context.PromotionDetails.RemoveRange(existing.PromotionDetails);
            }
            existing.PromotionDetails = new List<PromotionDetail>();

            var details = dto.Details ?? new List<PromotionDetailCreateDto>();
            foreach (var d in details)
            {
                var room = await _context.Rooms.FindAsync(d.RoomId);
                if (room == null) continue;

                decimal percent = 0m;
                if (room.Price > 0)
                {
                    percent = Math.Round((dto.DiscountAmount / room.Price) * 100m, 2);
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;
                }

                existing.PromotionDetails.Add(new PromotionDetail
                {
                    RoomId = d.RoomId,
                    DiscountPercent = percent
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.Promotions
                .Include(p => p.PromotionDetails)
                .FirstOrDefaultAsync(x => x.PromotionID == id);

            if (existing == null) throw new KeyNotFoundException("Promotion not found");

            if (existing.PromotionDetails.Any())
                _context.PromotionDetails.RemoveRange(existing.PromotionDetails);

            _context.Promotions.Remove(existing);
            await _context.SaveChangesAsync();
        }

        // optional generic implementations
        public void TDelete(Promotion entity) { _context.Promotions.Remove(entity); _context.SaveChanges(); }
        public Promotion TGetByID(int id) => _context.Promotions.Find(id)!;
        public List<Promotion> TGetList() => _context.Promotions.ToList();
        public void TInsert(Promotion entity) { _context.Promotions.Add(entity); _context.SaveChanges(); }
        public void TUpdate(Promotion entity) { _context.Promotions.Update(entity); _context.SaveChanges(); }
    }
}
