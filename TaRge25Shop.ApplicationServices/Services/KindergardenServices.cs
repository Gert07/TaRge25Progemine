using Microsoft.EntityFrameworkCore;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class KindergardenServices : IKindergardenServices
    {
        private readonly TaRge25ShopContext _context;
        private readonly IFileServices _fileServices;

        public KindergardenServices(TaRge25ShopContext context, IFileServices fileServices)
        {
            _context = context;
            _fileServices = fileServices;
        }

        public async Task<Kindergarden> Create(KindergardenDto dto)
        {
            var kindergarden = new Kindergarden
            {
                Id = Guid.NewGuid(),
                GroupName = dto.GroupName,
                ChildrenCount = dto.ChildrenCount,
                KindergardenName = dto.KindergardenName,
                TeacherName = dto.TeacherName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _fileServices.UploadFilesToDatabase(dto, kindergarden);
            await _context.Kindergardens.AddAsync(kindergarden);
            await _context.SaveChangesAsync();
            return kindergarden;
        }

        public async Task<Kindergarden?> Update(KindergardenDto dto)
        {
            var kindergarden = await _context.Kindergardens
                .FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (kindergarden == null)
            {
                return null;
            }

            kindergarden.GroupName = dto.GroupName;
            kindergarden.ChildrenCount = dto.ChildrenCount;
            kindergarden.KindergardenName = dto.KindergardenName;
            kindergarden.TeacherName = dto.TeacherName;
            kindergarden.UpdatedAt = DateTime.Now;

            _fileServices.UploadFilesToDatabase(dto, kindergarden);
            await _context.SaveChangesAsync();
            return kindergarden;
        }

        public Task<Kindergarden?> DetailAsync(Guid id)
        {
            return _context.Kindergardens.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Kindergarden?> Delete(Guid id)
        {
            var kindergarden = await DetailAsync(id);
            if (kindergarden == null)
            {
                return null;
            }

            var images = await _context.FileToDatabases
                .Where(x => x.KindergardenId == id)
                .ToListAsync();
            _context.FileToDatabases.RemoveRange(images);
            _context.Kindergardens.Remove(kindergarden);
            await _context.SaveChangesAsync();
            return kindergarden;
        }
    }
}
