
using TaRge25Shop.Core.Domain;
using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IFileServices
    {

        void FilesToApi(SpaceshipDto dto, Spaceship domain);
        
    }
}
