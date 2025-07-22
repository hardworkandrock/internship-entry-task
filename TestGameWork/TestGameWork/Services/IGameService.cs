using TestGameWork.DTOs;

namespace TestGameWork.Services
{
    public interface IGameService
    {
        Task<List<GameResponse>> GetListAsync(Guid userId);
        Task<GameResponse> MakeMoveAsync(Guid gameId, MakeMoveRequest request, Guid playerId); 
        Task<GameResponse> CreateGameAsync(CreateGameRequest request, Guid playerId);
        Task<GameResponse> GetGameByIdAsync(Guid gameId);
        Task<GameResponse> EnterTheGame(Guid gameId, Guid playerId);
        Task FinishTheGameAsync(Guid gameId, Guid playerId); 
    }
}
