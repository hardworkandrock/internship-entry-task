using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestGameWork.DTOs;
using TestGameWork.Services;

namespace TestGameWork.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GameController : ControllerBase
    { 
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Получить список 
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var userId = GetUserId();
            var games = await _gameService.GetListAsync(userId);
            return Ok(games);
        }

        // Создать новую игру
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            var userId = GetUserId();
            var game = await _gameService.CreateGameAsync(request, userId);
            return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, game);
        }

        // Присоединиться к игре (получить детали игры)
        [HttpGet("{gameId:guid}")]
        public async Task<IActionResult> GetGame(Guid gameId)
        {
            var userId = GetUserId();
            var game = await _gameService.EnterTheGame(gameId, userId);
            if (game == null)
                return NotFound();

            return Ok(game);
        }

        // Завершить игру (игрок сдаётся)
        [HttpPut("{id:guid}/finish")]
        public async Task<IActionResult> FinishTheGame(Guid id)
        {
            var userId = GetUserId();
            await _gameService.FinishTheGameAsync(id, userId);
            return NoContent();
        }

        // Сделать ход
        [HttpPost("{gameId:guid}/moves")]
        public async Task<IActionResult> MakeMove(Guid gameId, [FromBody] MakeMoveRequest request)
        {
            var userId = GetUserId();
            var updatedGame = await _gameService.MakeMoveAsync(gameId, request, userId);
            return Ok(updatedGame);
        }

        // Получить Guid пользователя из JWT
        private Guid GetUserId()
        {
            var nameIdentifierClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (nameIdentifierClaim is null)
                throw new UnauthorizedAccessException("User ID not found in token");

            if (Guid.TryParse(nameIdentifierClaim.Value, out var parsedId))
                return parsedId;

            throw new UnauthorizedAccessException("User ID is not a valid GUID");
        }

    }
}
