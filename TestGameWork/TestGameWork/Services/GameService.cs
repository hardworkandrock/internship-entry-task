using Newtonsoft.Json;
using System.Collections.Generic;
using TestGameWork.DTOs;
using TestGameWork.Models;
using TestGameWork.Repositories;

namespace TestGameWork.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gRepo;
        private readonly IPlayerRepository _pRepo;
        private readonly IConfiguration _config;

        public GameService(IGameRepository repo1, IPlayerRepository repo2, IConfiguration config)
        {
            _gRepo = repo1;
            _pRepo = repo2;
            _config = config;
        }

        public async Task<GameResponse> MakeMoveAsync(Guid gameId, MakeMoveRequest request, Guid playerId)
        {
            var game = await _gRepo.GetByIdAsync(gameId);
            var player1 = await _pRepo.GetByIdAsync(game.Player1);
            var player2 = await _pRepo.GetByIdAsync(game.Player2);
            var condition = game.WinCondition;

            if (game == null) throw new Exception("Game not found");

            
            var board = JsonConvert.DeserializeObject<List<List<string>>>(game.BoardState);

            var moveNumber = game.BoardState.Count(c => c == 'X' || c == 'O') + 1;

            if (request.Row < 0 || request.Row >= game.BoardSize ||
                request.Column < 0 || request.Column >= game.BoardSize)
                throw new ArgumentException("Invalid move");

            if (board[request.Row][request.Column] != null)
                throw new InvalidOperationException("Cell already taken");

            // check step is correct
            if ((game.Step == 1 && game.Player1 != playerId) || (game.Step == 2 && game.Player2 != playerId))
                throw new ArgumentException($"Invalid move. Player Guid does not match the current player who is moving. " +
                    $"\ngame.Player1 = {game.Player1}" + $"\ngame.Player2 = {game.Player2}" + $"\nplayerId = {playerId}");

            // actualSymbol Player1 == Step == 1 == "X"
            var actualSymbol = game.Step == 1 ? "X" : "O";

            // 10% chance every 3rd move
            var random = new Random();
            if (moveNumber % 3 == 0 && random.NextDouble() < 0.1)
                actualSymbol = actualSymbol == "X" ? "O" : "X";

            board[request.Row][request.Column] = actualSymbol;

            // Check win
            if (CheckWin(board, actualSymbol, game.BoardSize, game.BoardSize, condition))
                game.Status = actualSymbol == "X" ? GameStatus.XWin : GameStatus.OWin;
            else if (IsBoardFull(board))
                game.Status = GameStatus.Finished;
            else // Next Player step 
                game.Step = game.Step == 1 ? 2 : 1; 

            game.BoardState = JsonConvert.SerializeObject(board);
            
            await _gRepo.UpdateAsync(game);

            return MapToResponse(game);
        }

        public async Task<List<GameResponse>> GetListAsync(Guid playerId)
        {
            var games = await _gRepo.GetAllAsync(playerId);
            return games.Select(MapToResponse).ToList();
        } 

        public async Task<GameResponse> CreateGameAsync(CreateGameRequest request, Guid playerId)
        {
            var boardSize = request.BoardSize ?? _config.GetValue<int>("AppSettings:BoardSize");
            var winCondition = request.WinCondition ?? _config.GetValue<int>("AppSettings:WinCondition");

            var board = new string[boardSize, boardSize]; // List<List<string>>()
            var jsonBoard = JsonConvert.SerializeObject(board);

            var game = new Game
            {
                Id = Guid.NewGuid(),
                BoardSize = boardSize,
                BoardState = jsonBoard,
                Player1 = playerId,
                WinCondition = winCondition,
                Status = GameStatus.Active
            };

            await _gRepo.AddAsync(game);
            await _gRepo.UpdateAsync(game);

            return MapToResponse(game);
        }

        public async Task<GameResponse> GetGameByIdAsync(Guid gameId)
        {
            var game = await _gRepo.GetByIdAsync(gameId);
            if (game == null)
                return null;

            return MapToResponse(game);
        }

        public async Task<GameResponse> EnterTheGame(Guid gameId, Guid playerId)
        {
            var game = await _gRepo.GetByIdAsync(gameId);
            if (game == null)
                return null;

            if (game.Player1 == playerId || game.Player2 == playerId)
                return MapToResponse(game);

            if (game.Player1 == Guid.Empty) game.Player1 = playerId;
            else if (game.Player2 == Guid.Empty) game.Player2 = playerId;
            else throw new ArgumentException("All seats are taken");

            await _gRepo.UpdateAsync(game);
             
            return MapToResponse(game);
        }

        public async Task FinishTheGameAsync(Guid gameId, Guid playerId)
        {
            var game = await _gRepo.GetByIdAsync(gameId);
            if (game == null || game.Status != GameStatus.Active)
                throw new ArgumentException("Game not found or already finished");

            // Проверка, что игрок участвует в игре
            if (game.Player1 != playerId && game.Player2 != playerId)
                throw new UnauthorizedAccessException("Player is not part of this game");

            // Изменение статуса игры
            game.Status = GameStatus.Finished;
            await _gRepo.UpdateAsync(game);
        }

        private GameResponse MapToResponse(Game game)
        {
            var player1 = _pRepo.GetById(game.Player1);
            var player2 = _pRepo.GetById(game.Player2);
            List<List<string>> board;
            if (game.BoardState != null)
                board = JsonConvert.DeserializeObject<List<List<string>>>(game.BoardState);
            else
                board = CreateEmptyBoard(game.BoardSize); 

            // var board = JsonConvert.DeserializeObject<string[,]>(game.BoardState);
            return new GameResponse
            {
                Id = game.Id,
                BoardSize = game.BoardSize,
                Board = board,
                Player1 = player1?.Name ?? "",
                Player2 = player2?.Name ?? "",
                Step = game.Step == 1 ? (player1?.Name ?? "") : (player2?.Name ?? ""),
                Status = game.Status.ToString()
            };
        }
        private List<List<string>> CreateEmptyBoard(int size)
        {
            var board = new List<List<string>>();
            for (int i = 0; i < size; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < size; j++)
                {
                    row.Add(null);
                }
                board.Add(row);
            }
            return board;
        }
        private string InitBoardState(int boardSize) =>
            "[[null, null, null],[null, null, null],[null, null, null]]";
        private bool IsBoardFull(List<List<string>> board)
        {
            foreach (var row in board)
            {
                foreach (var cell in row)
                {
                    if (cell == null)
                        return false;
                }
            }
            return true;
        }

        private bool CheckWin(List<List<string>> board, string player, int rows, int cols, int winCondition)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (CheckLine(board, player, r, c, 1, 0, rows, cols, winCondition) ||  // Horizontal
                        CheckLine(board, player, r, c, 0, 1, rows, cols, winCondition) ||  // Vertical
                        CheckLine(board, player, r, c, 1, 1, rows, cols, winCondition) ||  // Diagonal \
                        CheckLine(board, player, r, c, 1, -1, rows, cols, winCondition))   // Diagonal /
                        return true;
                }
            }
            return false;
        }

        private bool CheckLine(List<List<string>> board, string player, int r, int c, int dr, int dc, int rows, int cols, int winCondition)
        {
            int count = 0;
            while (r < rows && c < cols && r >= 0 && c >= 0 &&
                   board[r][c] == player)
            {
                count++;
                r += dr;
                c += dc;
            }
            return count >= winCondition;
        }

    }
}
