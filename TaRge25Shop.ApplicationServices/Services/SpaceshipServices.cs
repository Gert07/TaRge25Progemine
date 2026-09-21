using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Data;
using Microsoft.EntityFrameworkCore;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class SpaceshipServices : ISpaceshipServices
    {
        private readonly TaRge25ShopContext _context;
        private readonly IFileServices _fileServices;
        public SpaceshipServices
            (
                TaRge25ShopContext context,
                IFileServices fileServices
            )
        {
            _context = context;
            _fileServices = fileServices;
        }
        //see meetod on vaja controlleris esile kutsuda
        //peab lisama interface, et kutsuda see meetod välja

        public async Task<Spaceship>Create(SpaceshipDto dto)
        {
            //siin peab tegema vahe instansi dto ja domaini vahel, et saaks domaini salvestada andmebaasi
            Spaceship spaceship = new Spaceship
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ShipType = dto.ShipType,
                Crew = dto.Crew,
                EnginePower = dto.EnginePower,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _fileServices.FilesToApi(dto, spaceship);

            //Andmete salvestamine andmebaasi (näiteks Entity Frameworki abil)
            await _context.Spaceships.AddAsync(spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }

        //Teha update meetod, mis võtab vastu dto ja uuendab olemasolevat kosmoselaeva
        public async Task<Spaceship>Update(SpaceshipDto dto)
        {
            Spaceship spaceship = new Spaceship
            {
                Id = dto.Id,
                Name = dto.Name,
                ShipType = dto.ShipType,
                Crew = dto.Crew,
                EnginePower = dto.EnginePower,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = DateTime.Now
            };
            
            _context.Spaceships.Update(spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }

        public async Task<Spaceship>DetailAsync(Guid id)
        {
            var spaceship = await _context.Spaceships
                .FirstOrDefaultAsync(x => x.Id == id);

            return spaceship;
        }

        public async Task<Spaceship>Delete(Guid id)
        {
            var result = await _context.Spaceships
                .FirstOrDefaultAsync(x => x.Id == id);

            _context.Spaceships.Remove(result);
            await _context.SaveChangesAsync();

            return result;
        }
    }
}
