using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CheckersApp
{
    public class GameEngine
    {
        private BoardCell[,] _board;
        private PieceColor _currentPlayer;
        private Position _selectedPiece;
        private List<Move> _possibleMoves;
        private GameMode _gameMode;
        private AIDifficulty _aiDifficulty;
        private Random _random;
        private Move _lastMove;

        public int MoveCount { get; private set; }
        public int WhitePieces { get; private set; }
        public int BlackPieces { get; private set; }
        public bool IsGameOver { get; private set; }
        public string Winner { get; private set; }
        public PieceColor CurrentPlayer => _currentPlayer;

        public GameEngine(GameMode gameMode = GameMode.TwoPlayers, AIDifficulty aiDifficulty = AIDifficulty.Medium)
        {
            _gameMode = gameMode;
            _aiDifficulty = aiDifficulty;
            _random = new Random();
            InitializeBoard();
            _currentPlayer = PieceColor.White;
            _selectedPiece = null;
            _possibleMoves = new List<Move>();
            IsGameOver = false;
            Winner = "";
            MoveCount = 0;
            CountPieces();
        }

        private void InitializeBoard()
        {
            _board = new BoardCell[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = new BoardCell
                    {
                        Row = row,
                        Column = col,
                        Color = (row + col) % 2 == 0 ? Colors.LightGray : Colors.DarkGray,
                        HasPiece = false,
                        IsKing = false,
                        IsSelected = false,
                        IsPossibleMove = false,
                        IsLastMove = false
                    };

                    if ((row + col) % 2 == 1)
                    {
                        if (row < 3)
                        {
                            cell.PieceColor = Colors.Black;
                            cell.PieceBorderColor = Colors.DarkSlateGray;
                            cell.HasPiece = true;
                        }
                        else if (row > 4)
                        {
                            cell.PieceColor = Colors.White;
                            cell.PieceBorderColor = Colors.Gray;
                            cell.HasPiece = true;
                        }
                    }

                    _board[row, col] = cell;
                }
            }
        }

        private void CountPieces()
        {
            WhitePieces = 0;
            BlackPieces = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece)
                    {
                        if (_board[row, col].PieceColor == Colors.White)
                            WhitePieces++;
                        else if (_board[row, col].PieceColor == Colors.Black)
                            BlackPieces++;
                    }
                }
            }
        }

        public IEnumerable<BoardCell> GetBoardCells()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    yield return _board[row, col];
                }
            }
        }

        public void HandleCellClick(int row, int col)
        {
            if (IsGameOver) return;

            var cell = _board[row, col];

            if (cell.HasPiece && GetPieceColor(cell) == _currentPlayer)
            {
                SelectPiece(row, col);
            }
            else if (_selectedPiece != null)
            {
                var move = _possibleMoves.FirstOrDefault(m =>
                    m.To.Row == row && m.To.Column == col);

                if (move != null)
                {
                    ExecuteMove(move);
                    CountPieces();

                    if (move.IsCapture)
                    {
                        var moreCaptures = CalculateCaptures(move.To.Row, move.To.Column);
                        if (moreCaptures.Count > 0)
                        {
                            SelectPiece(move.To.Row, move.To.Column);
                            return;
                        }
                    }

                    SwitchPlayer();
                    ClearHighlights();
                    _selectedPiece = null;
                    CheckGameOver();
                }
            }
        }

        private void SelectPiece(int row, int col)
        {
            ClearHighlights();
            _selectedPiece = new Position(row, col);
            _board[row, col].IsSelected = true;

            _possibleMoves = CalculatePossibleMoves(row, col);

            foreach (var move in _possibleMoves)
            {
                _board[move.To.Row, move.To.Column].IsPossibleMove = true;
            }
        }

        private void ExecuteMove(Move move)
        {
            var fromCell = _board[move.From.Row, move.From.Column];
            var toCell = _board[move.To.Row, move.To.Column];

            toCell.PieceColor = fromCell.PieceColor;
            toCell.PieceBorderColor = fromCell.PieceBorderColor;
            toCell.HasPiece = true;
            toCell.IsKing = fromCell.IsKing;

            fromCell.HasPiece = false;
            fromCell.IsKing = false;
            fromCell.IsSelected = false;

            if (move.IsCapture && move.CapturedPiece != null)
            {
                var capturedCell = _board[move.CapturedPiece.Row, move.CapturedPiece.Column];
                capturedCell.HasPiece = false;
                capturedCell.IsKing = false;

                SoundManager.PlayCaptureSound();
            }
            else
            {
                SoundManager.PlayMoveSound();
            }

            if (CheckForPromotion(toCell))
            {
                SoundManager.PlayKingSound();
            }

            if (_lastMove != null)
            {
                _board[_lastMove.From.Row, _lastMove.From.Column].IsLastMove = false;
                _board[_lastMove.To.Row, _lastMove.To.Column].IsLastMove = false;
            }

            _lastMove = move;
            _board[move.From.Row, move.From.Column].IsLastMove = true;
            _board[move.To.Row, move.To.Column].IsLastMove = true;
        }

        public void MakeAIMove()
        {
            if (_gameMode != GameMode.PlayerVsAI || _currentPlayer != PieceColor.Black)
                return;

            var allMoves = GetAllPossibleMovesForCurrentPlayer();

            if (allMoves.Count == 0)
            {
                IsGameOver = true;
                return;
            }

            Move bestMove = allMoves[_random.Next(allMoves.Count)];

            ExecuteMove(bestMove);
            CountPieces();

            if (bestMove.IsCapture)
            {
                var moreCaptures = CalculateCaptures(bestMove.To.Row, bestMove.To.Column);
                if (moreCaptures.Count > 0)
                {
                    MakeAIMove();
                    return;
                }
            }

            SwitchPlayer();
            CheckGameOver();
        }

        private List<Move> CalculatePossibleMoves(int row, int col)
        {
            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();

            if (forcedCaptures.Count > 0)
            {
                return forcedCaptures.Where(m =>
                    m.From.Row == row && m.From.Column == col).ToList();
            }

            var cell = _board[row, col];
            if (cell.IsKing)
            {
                return CalculateKingMoves(row, col);
            }
            else
            {
                return CalculateNormalMoves(row, col);
            }
        }

        private List<Move> CalculateNormalMoves(int row, int col)
        {
            var moves = new List<Move>();
            var cell = _board[row, col];
            var directions = GetMoveDirections(cell);

            foreach (var dir in directions)
            {
                int newRow = row + dir.Row;
                int newCol = col + dir.Column;

                if (IsValidPosition(newRow, newCol) && !_board[newRow, newCol].HasPiece)
                {
                    moves.Add(new Move(new Position(row, col), new Position(newRow, newCol)));
                }
            }

            return moves;
        }

        private List<Move> CalculateKingMoves(int row, int col)
        {
            var moves = new List<Move>();
            var directions = new Position[]
            {
                new Position(-1, -1), new Position(-1, 1),
                new Position(1, -1), new Position(1, 1)
            };

            foreach (var dir in directions)
            {
                int newRow = row + dir.Row;
                int newCol = col + dir.Column;

                if (IsValidPosition(newRow, newCol) && !_board[newRow, newCol].HasPiece)
                {
                    moves.Add(new Move(new Position(row, col), new Position(newRow, newCol)));
                }
            }

            return moves;
        }

        private List<Move> CalculateCaptures(int row, int col)
        {
            var captures = new List<Move>();
            var cell = _board[row, col];
            var directions = GetMoveDirections(cell);

            foreach (var dir in directions)
            {
                int enemyRow = row + dir.Row;
                int enemyCol = col + dir.Column;
                int targetRow = row + 2 * dir.Row;
                int targetCol = col + 2 * dir.Column;

                if (IsValidPosition(enemyRow, enemyCol) &&
                    IsValidPosition(targetRow, targetCol) &&
                    _board[enemyRow, enemyCol].HasPiece &&
                    GetPieceColor(_board[enemyRow, enemyCol]) != GetPieceColor(cell) &&
                    !_board[targetRow, targetCol].HasPiece)
                {
                    captures.Add(new Move(
                        new Position(row, col),
                        new Position(targetRow, targetCol),
                        new Position(enemyRow, enemyCol)
                    ));
                }
            }

            return captures;
        }

        private List<Move> GetAllPossibleMovesForCurrentPlayer()
        {
            var allMoves = new List<Move>();
            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();

            if (forcedCaptures.Count > 0)
                return forcedCaptures;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece &&
                        GetPieceColor(_board[row, col]) == _currentPlayer)
                    {
                        allMoves.AddRange(CalculatePossibleMoves(row, col));
                    }
                }
            }

            return allMoves;
        }

        private List<Move> GetAllPossibleCapturesForCurrentPlayer()
        {
            var allCaptures = new List<Move>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece &&
                        GetPieceColor(_board[row, col]) == _currentPlayer)
                    {
                        allCaptures.AddRange(CalculateCaptures(row, col));
                    }
                }
            }

            return allCaptures;
        }

        private List<Position> GetMoveDirections(BoardCell cell)
        {
            var directions = new List<Position>();

            if (cell.IsKing || GetPieceColor(cell) == PieceColor.White)
            {
                directions.Add(new Position(-1, -1));
                directions.Add(new Position(-1, 1));
            }

            if (cell.IsKing || GetPieceColor(cell) == PieceColor.Black)
            {
                directions.Add(new Position(1, -1));
                directions.Add(new Position(1, 1));
            }

            return directions;
        }

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        private void ClearHighlights()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    _board[row, col].IsSelected = false;
                    _board[row, col].IsPossibleMove = false;
                }
            }
        }

        private bool CheckForPromotion(BoardCell cell)
        {
            if (!cell.IsKing &&
                ((GetPieceColor(cell) == PieceColor.White && cell.Row == 0) ||
                 (GetPieceColor(cell) == PieceColor.Black && cell.Row == 7)))
            {
                cell.IsKing = true;
                return true;
            }
            return false;
        }

        private void CheckGameOver()
        {
            var currentMoves = GetAllPossibleMovesForCurrentPlayer();

            if (currentMoves.Count == 0)
            {
                IsGameOver = true;
                Winner = _currentPlayer == PieceColor.White ? "Черные" : "Белые";

                if (Winner == "Белые")
                    SoundManager.PlayWhiteVictorySound();
                else
                    SoundManager.PlayBlackVictorySound();
            }
        }

        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            MoveCount++;
        }

        private PieceColor GetPieceColor(BoardCell cell)
        {
            return cell.PieceColor == Colors.White ? PieceColor.White : PieceColor.Black;
        }

        public string GetGameStatus()
        {
            if (IsGameOver)
                return $"Победа {Winner}! Ходов: {MoveCount}";

            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();
            if (forcedCaptures.Count > 0)
                return $"Обязаны бить! Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")}";

            return $"Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")}. Ходов: {MoveCount}";
        }

        public string GetStatistics()
        {
            return $"Белые: {WhitePieces} | Черные: {BlackPieces}";
        }

        public Move GetBestMoveHint()
        {
            var allMoves = GetAllPossibleMovesForCurrentPlayer();
            if (allMoves.Count == 0) return null;

            var captures = allMoves.Where(m => m.IsCapture).ToList();
            if (captures.Count > 0)
                return captures[_random.Next(captures.Count)];

            return allMoves[_random.Next(allMoves.Count)];
        }

        public void ClearHints()
        {
            ClearHighlights();
        }

        public void UpdateSkinColors()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = _board[row, col];
                    if (cell.HasPiece)
                    {
                    }
                }
            }
        }
    }

    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}