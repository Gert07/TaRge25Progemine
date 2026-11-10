using TaRge25Shop.Core.Dto;

namespace TaRge25Shop.Core.ServiceInterface
{
    public interface IEmailServices
    {
        void SendEmail(EmailDto dto);
    }
}
