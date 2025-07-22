using TestGameWork.DTOs;

namespace TestGameWork.Services
{
    public interface IAutorizationService
    { 
        Task<AutorizationResponse> RegisterAsync(RegisterModel model);
        Task<AutorizationResponse> LoginAsync(LoginModel model);
    }
}
