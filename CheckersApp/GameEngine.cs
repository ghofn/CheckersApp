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
        private Stack<GameState> _gameHistory;
        private int _maxHistorySize = 10;
        public int MoveCount { get; private set; }
        private bool _isMoveInProgress;

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
            _gameHistory = new Stack<GameState>();
            MoveCount = 0;
            _isMoveInProgress = false;
        }

        private class GameState
        {
            public BoardCell[,] Board { get; set; }
            public PieceColor CurrentPlayer { get; set; }
            public Move LastMove { get; set; }
            public int MoveCount { get; set; }
            public bool IsMoveInProgress { get; set; }
        }

        public bool UndoMove()
        {
            if (_gameHistory.Count == 0 || IsGameOver)
                return false;

            var state = _gameHistory.Pop();
            RestoreBoardState(state.Board);
            _currentPlayer = state.CurrentPlayer;
            _lastMove = state.LastMove;
            MoveCount = state.MoveCount;
            _isMoveInProgress = state.IsMoveInProgress;

            ClearHighlights();
            _selectedPiece = null;

            return true;
        }

        private GameState SaveGameState()
        {
            var state = new GameState
            {
                Board = SaveBoardState(),
                CurrentPlayer = _currentPlayer,
                LastMove = _lastMove,
                MoveCount = MoveCount,
                IsMoveInProgress = _isMoveInProgress
            };
            return state;
        }

        private void SaveStateToHistory()
        {
            if (_gameHistory.Count >= _maxHistorySize)
            {
                // Преобразуем в список, удаляем самый старый и обратно в стек
                var tempList = _gameHistory.ToList();
                tempList.RemoveAt(0);
                _gameHistory = new Stack<GameState>(tempList);
            }
            _gameHistory.Push(SaveGameState());
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
                        IsKing = false
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

            if (cell.HasPiece && cell.PieceColor == GetCurrentPlayerColor())
            {
                SelectPiece(row, col);
            }
            else if (_selectedPiece != null && !cell.HasPiece)
            {
                // Сохраняем состояние только при начале хода, а не при каждом клике
                if (!_isMoveInProgress)
                {
                    SaveStateToHistory();
                    _isMoveInProgress = true;
                }
                TryMakeMove(row, col);
            }
        }

        private void SelectPiece(int row, int col)
        {
            ClearMoveHighlights();
            _selectedPiece = new Position(row, col);
            _board[row, col].Color = Colors.LightBlue;

            _possibleMoves = CalculatePossibleMoves(row, col);

            foreach (var move in _possibleMoves)
            {
                _board[move.To.Row, move.To.Column].Color =
                    move.IsCapture ? Colors.Orange : Colors.LightGreen;
            }
        }

        private void TryMakeMove(int toRow, int toCol)
        {
            var move = _possibleMoves.FirstOrDefault(m =>
                m.To.Row == toRow && m.To.Column == toCol);

            if (move != null)
            {
                ExecuteMove(move);

                if (move.IsCapture)
                {
                    var canContinueCapture = CheckAdditionalCaptures(move.To.Row, move.To.Column);
                    if (canContinueCapture)
                    {
                        SelectPiece(move.To.Row, move.To.Column);
                        return;
                    }
                }

                // Ход завершен
                _isMoveInProgress = false;
                SwitchPlayer();
                ClearHighlights();
                _selectedPiece = null;
                CheckGameOver();
            }
        }

        private void ExecuteMove(Move move)
        {
            // Очищаем подсветку предыдущего хода
            if (_lastMove != null)
            {
                _board[_lastMove.From.Row, _lastMove.From.Column].Color =
                    (_lastMove.From.Row + _lastMove.From.Column) % 2 == 0 ? Colors.LightGray : Colors.DarkGray;
                _board[_lastMove.To.Row, _lastMove.To.Column].Color =
                    (_lastMove.To.Row + _lastMove.To.Column) % 2 == 0 ? Colors.LightGray : Colors.DarkGray;
            }

            var fromCell = _board[move.From.Row, move.From.Column];
            var toCell = _board[move.To.Row, move.To.Column];

            // Перемещаем шашку
            toCell.PieceColor = fromCell.PieceColor;
            toCell.PieceBorderColor = fromCell.PieceBorderColor;
            toCell.HasPiece = true;
            toCell.IsKing = fromCell.IsKing;

            fromCell.HasPiece = false;
            fromCell.IsKing = false;

            // Если это взятие - убираем съеденную шашку
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

            CheckForPromotion(toCell);

            // Устанавливаем новый последний ход
            _lastMove = move;
            _board[move.From.Row, move.From.Column].Color = Color.FromArgb(255, 255, 215, 0);
            _board[move.To.Row, move.To.Column].Color = Color.FromArgb(255, 255, 215, 0);
        }

        public void MakeAIMove()
        {
            if (_gameMode != GameMode.PlayerVsAI || _currentPlayer != PieceColor.Black)
                return;

            // Сохраняем состояние перед ходом ИИ
            SaveStateToHistory();

            var allMoves = GetAllPossibleMovesForCurrentPlayer();

            if (allMoves.Count == 0)
            {
                IsGameOver = true;
                return;
            }

            Move bestMove = null;

            switch (_aiDifficulty)
            {
                case AIDifficulty.Easy:
                    bestMove = allMoves[_random.Next(allMoves.Count)];
                    break;

                case AIDifficulty.Medium:
                    bestMove = FindBestMove(3);
                    break;

                case AIDifficulty.Hard:
                    bestMove = FindBestMove(5);
                    break;
            }

            if (bestMove == null)
                bestMove = allMoves[_random.Next(allMoves.Count)];

            ExecuteMove(bestMove);

            // Проверяем возможность продолжения взятия
            if (bestMove.IsCapture)
            {
                var canContinueCapture = CheckAdditionalCaptures(bestMove.To.Row, bestMove.To.Column);
                if (canContinueCapture)
                {
                    // Рекурсивно продолжаем взятие
                    MakeAIMove();
                    return;
                }
            }

            SwitchPlayer();
            CheckGameOver();
        }

        // Остальные методы остаются без изменений...
        // [Здесь должны быть все остальные методы из твоего кода]

        public List<Move> GetAllPossibleMovesForCurrentPlayer()
        {
            var allMoves = new List<Move>();
            var currentColor = GetCurrentPlayerColor();

            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();
            if (forcedCaptures.Count > 0)
            {
                return forcedCaptures;
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece && _board[row, col].PieceColor == currentColor)
                    {
                        var moves = _board[row, col].IsKing ?
                            CalculateKingMoves(row, col) :
                            CalculateNormalMoves(row, col);
                        allMoves.AddRange(moves);
                    }
                }
            }

            return allMoves;
        }

        private List<Move> GetAllPossibleMovesForColor(Color color)
        {
            var moves = new List<Move>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece && _board[row, col].PieceColor == color)
                    {
                        moves.AddRange(CalculatePossibleMoves(row, col));
                    }
                }
            }
            return moves;
        }

        private List<Move> CalculatePossibleMoves(int row, int col)
        {
            var cell = _board[row, col];
            var allCaptures = new List<Move>();

            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();

            if (forcedCaptures.Count > 0)
            {
                return forcedCaptures.Where(m =>
                    m.From.Row == row && m.From.Column == col).ToList();
            }

            if (cell.IsKing)
            {
                return CalculateKingMoves(row, col);
            }
            else
            {
                return CalculateNormalMoves(row, col);
            }
        }

        private List<Move> GetAllPossibleCapturesForCurrentPlayer()
        {
            var allCaptures = new List<Move>();
            var currentColor = GetCurrentPlayerColor();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece && _board[row, col].PieceColor == currentColor)
                    {
                        var captures = CalculateCaptures(row, col);
                        allCaptures.AddRange(captures);
                    }
                }
            }

            return allCaptures;
        }

        private List<Move> CalculateNormalMoves(int row, int col)
        {
            var moves = new List<Move>();
            var cell = _board[row, col];
            var directions = GetMoveDirections(row, col);

            foreach (var dir in directions)
            {
                int newRow = row + dir.Row;
                int newCol = col + dir.Column;

                if (IsValidPosition(newRow, newCol) && !_board[newRow, newCol].HasPiece)
                {
                    moves.Add(new Move(
                        new Position(row, col),
                        new Position(newRow, newCol)
                    ));
                }
            }

            return moves;
        }

        private List<Move> CalculateKingMoves(int row, int col)
        {
            var moves = new List<Move>();
            var directions = new List<Position>
            {
                new Position(-1, -1), new Position(-1, 1),
                new Position(1, -1), new Position(1, 1)
            };

            foreach (var dir in directions)
            {
                int currentRow = row + dir.Row;
                int currentCol = col + dir.Column;

                while (IsValidPosition(currentRow, currentCol))
                {
                    if (_board[currentRow, currentCol].HasPiece)
                    {
                        break;
                    }

                    moves.Add(new Move(
                        new Position(row, col),
                        new Position(currentRow, currentCol)
                    ));

                    currentRow += dir.Row;
                    currentCol += dir.Column;
                }
            }

            return moves;
        }

        private List<Move> CalculateCaptures(int row, int col)
        {
            var captures = new List<Move>();
            var cell = _board[row, col];

            if (cell.IsKing)
            {
                captures.AddRange(CalculateKingCaptures(row, col));
            }
            else
            {
                captures.AddRange(CalculateNormalCaptures(row, col));
            }

            return captures;
        }

        private List<Move> CalculateNormalCaptures(int row, int col)
        {
            var captures = new List<Move>();
            var cell = _board[row, col];
            var directions = GetForwardDirections(cell.PieceColor);

            foreach (var dir in directions)
            {
                int enemyRow = row + dir.Row;
                int enemyCol = col + dir.Column;
                int targetRow = row + 2 * dir.Row;
                int targetCol = col + 2 * dir.Column;

                if (IsValidPosition(enemyRow, enemyCol) &&
                    IsValidPosition(targetRow, targetCol) &&
                    _board[enemyRow, enemyCol].HasPiece &&
                    _board[enemyRow, enemyCol].PieceColor != cell.PieceColor &&
                    !_board[targetRow, targetCol].HasPiece)
                {
                    captures.Add(new Move(
                        new Position(row, col),
                        new Position(targetRow, targetCol))
                    {
                        IsCapture = true,
                        CapturedPiece = new Position(enemyRow, enemyCol)
                    });
                }
            }

            return captures;
        }

        private List<Move> CalculateKingCaptures(int row, int col)
        {
            var captures = new List<Move>();
            var cell = _board[row, col];
            var directions = new List<Position>
            {
                new Position(-1, -1), new Position(-1, 1),
                new Position(1, -1), new Position(1, 1)
            };

            foreach (var dir in directions)
            {
                int enemyRow = row + dir.Row;
                int enemyCol = col + dir.Column;
                int targetRow = row + 2 * dir.Row;
                int targetCol = col + 2 * dir.Column;

                if (IsValidPosition(enemyRow, enemyCol) &&
                    IsValidPosition(targetRow, targetCol) &&
                    _board[enemyRow, enemyCol].HasPiece &&
                    _board[enemyRow, enemyCol].PieceColor != cell.PieceColor &&
                    !_board[targetRow, targetCol].HasPiece)
                {
                    captures.Add(new Move(
                        new Position(row, col),
                        new Position(targetRow, targetCol))
                    {
                        IsCapture = true,
                        CapturedPiece = new Position(enemyRow, enemyCol)
                    });
                }
            }

            return captures;
        }

        private List<Position> GetForwardDirections(Color pieceColor)
        {
            if (pieceColor == Colors.White)
            {
                return new List<Position>
                {
                    new Position(-1, -1),
                    new Position(-1, 1)
                };
            }
            else
            {
                return new List<Position>
                {
                    new Position(1, -1),
                    new Position(1, 1)
                };
            }
        }

        private List<Position> GetMoveDirections(int row, int col)
        {
            var cell = _board[row, col];
            var directions = new List<Position>();

            if (cell.IsKing || cell.PieceColor == Colors.White)
            {
                directions.Add(new Position(-1, -1));
                directions.Add(new Position(-1, 1));
            }

            if (cell.IsKing || cell.PieceColor == Colors.Black)
            {
                directions.Add(new Position(1, -1));
                directions.Add(new Position(1, 1));
            }

            return directions;
        }

        private bool CheckAdditionalCaptures(int row, int col)
        {
            var additionalCaptures = CalculateCaptures(row, col);
            return additionalCaptures.Count > 0;
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
                    _board[row, col].Color = (row + col) % 2 == 0 ? Colors.LightGray : Colors.DarkGray;
                    _board[row, col].IsHighlighted = false;
                }
            }
        }

        private void CheckForPromotion(BoardCell cell)
        {
            if (!cell.IsKing &&
                ((cell.PieceColor == Colors.White && cell.Row == 0) ||
                 (cell.PieceColor == Colors.Black && cell.Row == 7)))
            {
                cell.IsKing = true;
            }
        }

        private void CheckGameOver()
        {
            bool hasPieces = false;
            bool hasValidMoves = false;
            Color currentColor = GetCurrentPlayerColor();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece && _board[row, col].PieceColor == currentColor)
                    {
                        hasPieces = true;
                        var moves = CalculatePossibleMoves(row, col);
                        if (moves.Count > 0)
                        {
                            hasValidMoves = true;
                            break;
                        }
                    }
                }
                if (hasValidMoves) break;
            }

            if (!hasPieces || !hasValidMoves)
            {
                IsGameOver = true;
                Winner = _currentPlayer == PieceColor.White ? "черные" : "белые";
                SoundManager.PlayVictorySound();
            }
        }

        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            MoveCount++;
        }

        private Color GetCurrentPlayerColor()
        {
            return _currentPlayer == PieceColor.White ? Colors.White : Colors.Black;
        }

        public string GetGameStatus()
        {
            if (IsGameOver)
            {
                return $"Игра окончена! Победили {Winner}! Ходов: {MoveCount}";
            }

            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();
            if (forcedCaptures.Count > 0)
            {
                return $"Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")} - ОБЯЗАН БИТЬ! Ходов: {MoveCount}";
            }

            return $"Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")}. Ходов: {MoveCount}";
        }

        // Методы для ИИ
        private int EvaluatePosition()
        {
            int score = 0;
            int blackPieces = 0;
            int whitePieces = 0;
            int blackKings = 0;
            int whiteKings = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = _board[row, col];
                    if (cell.HasPiece)
                    {
                        int pieceValue = cell.IsKing ? 30 : 10;

                        if (cell.PieceColor == Colors.Black)
                        {
                            blackPieces++;
                            if (cell.IsKing) blackKings++;
                            score += pieceValue;
                            if (!cell.IsKing) score += (7 - row) * 2;
                        }
                        else
                        {
                            whitePieces++;
                            if (cell.IsKing) whiteKings++;
                            score -= pieceValue;
                            if (!cell.IsKing) score -= row * 2;
                        }
                    }
                }
            }

            var blackMoves = GetAllPossibleMovesForColor(Colors.Black);
            var whiteMoves = GetAllPossibleMovesForColor(Colors.White);

            score += blackMoves.Count * 2;
            score -= whiteMoves.Count * 2;

            int materialDiff = (blackPieces + blackKings * 2) - (whitePieces + whiteKings * 2);
            score += materialDiff * 5;

            return score;
        }

        private Move FindBestMove(int depth)
        {
            var allMoves = GetAllPossibleMovesForCurrentPlayer();
            Move bestMove = null;
            int bestEvaluation = int.MinValue;

            foreach (var move in allMoves)
            {
                var savedBoard = SaveBoardState();

                ExecuteMove(move);
                int moveEvaluation = Minimax(depth - 1, int.MinValue, int.MaxValue, false);
                RestoreBoardState(savedBoard);

                if (moveEvaluation > bestEvaluation)
                {
                    bestEvaluation = moveEvaluation;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int Minimax(int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || IsGameOver)
            {
                return EvaluatePosition();
            }

            var moves = GetAllPossibleMovesForCurrentPlayer();

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;

                foreach (var move in moves)
                {
                    var savedBoard = SaveBoardState();

                    ExecuteMove(move);
                    int eval = Minimax(depth - 1, alpha, beta, false);
                    RestoreBoardState(savedBoard);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                foreach (var move in moves)
                {
                    var savedBoard = SaveBoardState();

                    ExecuteMove(move);
                    int eval = Minimax(depth - 1, alpha, beta, true);
                    RestoreBoardState(savedBoard);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }

                return minEval;
            }
        }

        private BoardCell[,] SaveBoardState()
        {
            var savedBoard = new BoardCell[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var original = _board[row, col];
                    savedBoard[row, col] = new BoardCell
                    {
                        Row = original.Row,
                        Column = original.Column,
                        Color = original.Color,
                        HasPiece = original.HasPiece,
                        PieceColor = original.PieceColor,
                        PieceBorderColor = original.PieceBorderColor,
                        IsKing = original.IsKing
                    };
                }
            }

            return savedBoard;
        }

        private void RestoreBoardState(BoardCell[,] savedBoard)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var saved = savedBoard[row, col];
                    _board[row, col] = new BoardCell
                    {
                        Row = saved.Row,
                        Column = saved.Column,
                        Color = saved.Color,
                        HasPiece = saved.HasPiece,
                        PieceColor = saved.PieceColor,
                        PieceBorderColor = saved.PieceBorderColor,
                        IsKing = saved.IsKing
                    };
                }
            }
        }

        public void ClearHints()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    _board[row, col].IsHighlighted = false;
                }
            }
        }

        public void ClearMoveHighlights()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].Color == Colors.LightBlue ||
                        _board[row, col].Color == Colors.Orange ||
                        _board[row, col].Color == Colors.LightGreen)
                    {
                        _board[row, col].Color = (row + col) % 2 == 0 ? Colors.LightGray : Colors.DarkGray;
                    }
                }
            }
        }
        public void ShowHint()
        {
            ClearHints();

            var allMoves = GetAllPossibleMovesForCurrentPlayer();

            foreach (var move in allMoves)
            {
                _board[move.From.Row, move.From.Column].IsHighlighted = true;

                _board[move.To.Row, move.To.Column].IsHighlighted = true;
            }
        }
    }
}