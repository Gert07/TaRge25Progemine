using TaRge25Shop.Core.ServiceInterface;
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;
using TaRge25Shop.Data;

namespace TaRge25Shop.ApplicationServices.Services
{
    public class SpaceshipServices : ISpaceshipServices
    {
        private readonly TaRge25ShopContext _context;
        public SpaceshipServices(TaRge25ShopContext context)
        {
            _context = context;
        }
        //see meetod on vaja controlleris esile kutsuda
        //peab lisama interface, et kutsuda see meetod välja

        public async Task<Spaceship>Create(SpaceshipDto dto)
        {
            //siin peab tegema vahe instansi dto ja domaini vahel, et saaks domaini salvestada andmebaasi
            Spaceship spaceship = new Spaceship
            {
                Id = dto.Id,
                Name = dto.Name,
                ShipType = dto.ShipType,
                Crew = dto.Crew,
                EnginePower = dto.EnginePower,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
            //Andmete salvestamine andmebaasi (näiteks Entity Frameworki abil)
            await _context.Spaceships.AddAsync(spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }
    }
}
