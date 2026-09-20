using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Data;
using Microsoft.EntityFrameworkCore;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class KindergardenServices : IKindergardenServices
    {
        private readonly TaRge25ShopContext _context;
        public KindergardenServices
            (
                TaRge25ShopContext context
            )
        {
            _context = context;
        }

        public async Task<Kindergarden>Create(KindergardenDto dto)
        {
            Kindergarden kindergarden = new Kindergarden
            {
                Id = Guid.NewGuid(),
                GroupName = dto.GroupName,
                ChildrenCount = dto.ChildrenCount,
                KindergardenName = dto.KindergardenName,
                TeacherName = dto.TeacherName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Kindergardens.AddAsync(kindergarden);
            await _context.SaveChangesAsync();

            return kindergarden;
        }

        public async Task<Kindergarden>Update(KindergardenDto dto)
        {
            Kindergarden kindergarden = new Kindergarden
            {
                Id = dto.Id,
                GroupName = dto.GroupName,
                ChildrenCount = dto.ChildrenCount,
                KindergardenName = dto.KindergardenName,
                TeacherName = dto.TeacherName,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            _context.Kindergardens.Update(kindergarden);
            await _context.SaveChangesAsync();

            return kindergarden;
        }

        public async Task<Kindergarden>DetailAsync(Guid id)
        {
            var kindergarden = await _context.Kindergardens
                .FirstOrDefaultAsync(x => x.Id == id);

            return kindergarden;
        }

        public async Task<Kindergarden>Delete(Guid id)
        {
            var result = await _context.Kindergardens
                .FirstOrDefaultAsync(x => x.Id == id);

            _context.Kindergardens.Remove(result);
            await _context.SaveChangesAsync();

            return result;
        }
    }
}
