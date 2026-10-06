using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Data;
using Microsoft.EntityFrameworkCore;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class RealEstateServices : IRealEstateServices
    {
        private readonly TaRge25ShopContext _context;
        private readonly IFileServices _fileServices;

        public RealEstateServices
            (
                TaRge25ShopContext context,
                IFileServices fileServices
            )
        {
            _context = context;
            _fileServices = fileServices;
        }

        public async Task<RealEstate> Create(RealEstateDto dto)
        {
            RealEstate realEstate = new RealEstate
            {
                Id = Guid.NewGuid(),
                Area = dto.Area,
                Location = dto.Location,
                RoomNumber = dto.RoomNumber,
                BuildingType = dto.BuildingType,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            if (dto.Files != null && dto.Files.Count > 0)
            {
                _fileServices.UploadFilesToDatabase(dto, realEstate);
            }

            await _context.RealEstates.AddAsync(realEstate);
            await _context.SaveChangesAsync();

            return realEstate;
        }

        public async Task<RealEstate?> Update(RealEstateDto dto)
        {
            RealEstate realEstate = new RealEstate
            {
                Id = dto.Id,
                Area = dto.Area,
                Location = dto.Location,
                RoomNumber = dto.RoomNumber,
                BuildingType = dto.BuildingType,
                CreatedAt = dto.CreatedAt,
                ModifiedAt = DateTime.Now
            };

            if (dto.Files != null && dto.Files.Count > 0)
            {
                _fileServices.UploadFilesToDatabase(dto, realEstate);
            }

            _context.RealEstates.Update(realEstate);
            await _context.SaveChangesAsync();

            return realEstate;
        }

        public async Task<RealEstate?> DetailAsync(Guid id)
        {
            var realEstate = await _context.RealEstates
                .FirstOrDefaultAsync(x => x.Id == id);

            return realEstate;
        }

        public async Task<RealEstate?> Delete(Guid id)
        {
            var result = await _context.RealEstates
                .FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
            {
                return null;
            }

            var images = await _context.FileToDatabases
                .Where(x => x.RealEstateId == id)
                .ToListAsync();

            _context.FileToDatabases.RemoveRange(images);
            _context.RealEstates.Remove(result);
            await _context.SaveChangesAsync();

            return result;
        }
    }
}
