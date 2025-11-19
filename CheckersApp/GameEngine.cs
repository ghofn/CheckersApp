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
        private Random _random;
        private AIDifficulty _aiDifficulty = AIDifficulty.Medium;
        public PieceColor CurrentPlayer => _currentPlayer;


        public bool IsGameOver { get; private set; }

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

                SwitchPlayer();
                ClearHighlights();
                _selectedPiece = null;
                CheckGameOver();  // ← ЭТУ СТРОЧКУ ДОБАВЬ
            }
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

                    // Расставляем шашки только на темных клетках
                    if ((row + col) % 2 == 1)
                    {
                        // Черные шашки (верхние 3 ряда)
                        if (row < 3)
                        {
                            cell.PieceColor = Colors.Black;
                            cell.PieceBorderColor = Colors.DarkSlateGray;
                            cell.HasPiece = true;
                        }
                        // Белые шашки (нижние 3 ряда)
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

            // Если кликнули на шашку текущего игрока
            if (cell.HasPiece && cell.PieceColor == GetCurrentPlayerColor())
            {
                SelectPiece(row, col);
            }
            // Если кликнули на пустую клетку и есть выбранная шашка
            else if (_selectedPiece != null && !cell.HasPiece)
            {
                TryMakeMove(row, col);
            }
        }

        private void SelectPiece(int row, int col)
        {
            ClearHighlights();
            _selectedPiece = new Position(row, col);
            _board[row, col].Color = Colors.LightBlue;

            _possibleMoves = CalculatePossibleMoves(row, col);

            // Подсветка возможных ходов
            foreach (var move in _possibleMoves)
            {
                _board[move.To.Row, move.To.Column].Color =
                    move.IsCapture ? Colors.Orange : Colors.LightGreen;
            }
        }

        private void ExecuteMove(Move move)
        {
            // Перемещаем шашку
            var fromCell = _board[move.From.Row, move.From.Column];
            var toCell = _board[move.To.Row, move.To.Column];

            toCell.PieceColor = fromCell.PieceColor;
            toCell.PieceBorderColor = fromCell.PieceBorderColor;
            toCell.HasPiece = true;
            toCell.IsKing = fromCell.IsKing;

            fromCell.HasPiece = false;
            fromCell.IsKing = false;

            // Убираем сбитую шашку (если есть)
            if (move.IsCapture && move.CapturedPiece != null)
            {
                var capturedCell = _board[move.CapturedPiece.Row, move.CapturedPiece.Column];
                capturedCell.HasPiece = false;
                capturedCell.IsKing = false;
            }

            // Проверка на превращение в дамку
            CheckForPromotion(toCell);
        }
        public string GetGameStatus()
        {
            if (IsGameOver)
            {
                string winner = _currentPlayer == PieceColor.White ? "черные" : "белые";
                return $"Игра окончена! Победили {winner}!";
            }

            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();
            if (forcedCaptures.Count > 0)
            {
                return $"Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")} - ОБЯЗАН БИТЬ!";
            }

            return $"Ход: {(_currentPlayer == PieceColor.White ? "белые" : "черные")}";
        }
        private List<Move> CalculatePossibleMoves(int row, int col)
        {
            var cell = _board[row, col];
            var allCaptures = new List<Move>();

            // Сначала проверяем все возможные взятия для текущего игрока
            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();

            // Если есть обязательные взятия, возвращаем только взятия для этой шашки
            if (forcedCaptures.Count > 0)
            {
                return forcedCaptures.Where(m =>
                    m.From.Row == row && m.From.Column == col).ToList();
            }

            // Если взятий нет, возвращаем обычные ходы
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

                // Двигаемся по диагонали, пока не упремся в край доски или другую шашку
                while (IsValidPosition(currentRow, currentCol))
                {
                    if (_board[currentRow, currentCol].HasPiece)
                    {
                        break; // Упёрлись в шашку - дальше нельзя
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
                // Дамка может бить в любом направлении
                captures.AddRange(CalculateKingCaptures(row, col));
            }
            else
            {
                // Обычная шашка может бить ТОЛЬКО ВПЕРЕД
                captures.AddRange(CalculateNormalCaptures(row, col));
            }

            return captures;
        }
        private void CheckGameOver()
        {
            bool hasPieces = false;
            bool hasValidMoves = false;
            Color currentColor = GetCurrentPlayerColor();

            // Проверяем, есть ли у текущего игрока шашки
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col].HasPiece && _board[row, col].PieceColor == currentColor)
                    {
                        hasPieces = true;
                        // Проверяем, есть ли у этой шашки возможные ходы
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

            // Если нет шашек или нет возможных ходов - игра окончена
            if (!hasPieces || !hasValidMoves)
            {
                IsGameOver = true;
            }
        }

        private List<Move> CalculateNormalCaptures(int row, int col)
        {
            var captures = new List<Move>();
            var cell = _board[row, col];

            // Только направления ВПЕРЕД для обычных шашек
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

            // Дамка может бить во всех направлениях
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
                        int positionScore = 0;

                        if (cell.PieceColor == Colors.Black)
                        {
                            blackPieces++;
                            if (cell.IsKing)
                            {
                                blackKings++;
                                positionScore += 35; // Дамки очень ценны

                                // Дамки в центре и на флангах
                                if (col >= 2 && col <= 5) positionScore += 3;
                                if (row >= 2 && row <= 5) positionScore += 2;
                            }
                            else
                            {
                                positionScore += 10;
                                // Сильно поощряем продвижение к дамке
                                positionScore += (7 - row) * 3;
                                // Защищенные пешки (рядом с краем)
                                if (col == 0 || col == 7) positionScore += 2;
                                // Пешки в центре
                                if (col >= 3 && col <= 4) positionScore += 1;
                            }

                            score += positionScore;
                        }
                        else // Белые
                        {
                            whitePieces++;
                            if (cell.IsKing)
                            {
                                whiteKings++;
                                positionScore += 35;
                                if (col >= 2 && col <= 5) positionScore += 3;
                                if (row >= 2 && row <= 5) positionScore += 2;
                            }
                            else
                            {
                                positionScore += 10;
                                positionScore += row * 3; // Для белых чем ниже, тем лучше
                                if (col == 0 || col == 7) positionScore += 2;
                                if (col >= 3 && col <= 4) positionScore += 1;
                            }

                            score -= positionScore;
                        }
                    }
                }
            }

            // Бонус за мобильность
            var blackMoves = GetAllPossibleMovesForColor(Colors.Black);
            var whiteMoves = GetAllPossibleMovesForColor(Colors.White);

            score += blackMoves.Count * 3;
            score -= whiteMoves.Count * 3;

            // Сильный бонус за материальное преимущество
            int materialDiff = (blackPieces * 10 + blackKings * 25) - (whitePieces * 10 + whiteKings * 25);
            score += materialDiff;

            // В эндшпиле короли становятся еще важнее
            int totalPieces = blackPieces + whitePieces;
            if (totalPieces < 6)
            {
                score += blackKings * 15;
                score -= whiteKings * 15;

                // В эндшпиле поощряем централизацию дамок
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        var cell = _board[row, col];
                        if (cell.HasPiece && cell.IsKing)
                        {
                            int centerBonus = 0;
                            if (row >= 3 && row <= 4 && col >= 3 && col <= 4) centerBonus = 5;
                            else if (row >= 2 && row <= 5 && col >= 2 && col <= 5) centerBonus = 3;

                            if (cell.PieceColor == Colors.Black)
                                score += centerBonus;
                            else
                                score -= centerBonus;
                        }
                    }
                }
            }

            return score;
        }

        // Вспомогательный метод для получения ходов по цвету
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


        private int Minimax(int depth, int alpha, int beta, bool maximizingPlayer)
        {
            // Если достигнута максимальная глубина или игра окончена, возвращаем оценку
            if (depth == 0 || IsGameOver)
            {
                return EvaluatePosition();
            }

            var moves = GetAllPossibleMovesForCurrentPlayer();

            if (maximizingPlayer) // ИИ (черные) - максимизируем оценку
            {
                int maxEval = int.MinValue;

                foreach (var move in moves)
                {
                    // Сохраняем состояние до хода
                    var savedBoard = SaveBoardState();

                    // Делаем ход
                    ExecuteMove(move);

                    // Рекурсивно оцениваем позицию
                    int eval = Minimax(depth - 1, alpha, beta, false);

                    // Возвращаем состояние
                    RestoreBoardState(savedBoard);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // Альфа-бета отсечение
                }

                return maxEval;
            }
            else // Игрок (белые) - минимизируем оценку
            {
                int minEval = int.MaxValue;

                foreach (var move in moves)
                {
                    // Сохраняем состояние до хода
                    var savedBoard = SaveBoardState();

                    // Делаем ход
                    ExecuteMove(move);

                    // Рекурсивно оцениваем позицию
                    int eval = Minimax(depth - 1, alpha, beta, true);

                    // Возвращаем состояние
                    RestoreBoardState(savedBoard);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break; // Альфа-бета отсечение
                }

                return minEval;
            }
        }

        // Сохраняем состояние доски
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

        // Восстанавливаем состояние доски
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









        private List<Position> GetForwardDirections(Color pieceColor)
        {
            // Обычные шашки бьют ТОЛЬКО ВПЕРЕД
            if (pieceColor == Colors.White)
            {
                // Белые бьют ВВЕРХ (к меньшим номерам строк)
                return new List<Position>
                {
                    new Position(-1, -1),
                    new Position(-1, 1)
                };
            }
            else
            {
                // Черные бьют ВНИЗ (к большим номерам строк)
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
                // Белые и дамки ходят вверх
                directions.Add(new Position(-1, -1));
                directions.Add(new Position(-1, 1));
            }

            if (cell.IsKing || cell.PieceColor == Colors.Black)
            {
                // Черные и дамки ходят вниз
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

        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }

        private Color GetCurrentPlayerColor()
        {
            return _currentPlayer == PieceColor.White ? Colors.White : Colors.Black;
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

            Move bestMove = null;

            // Разная логика в зависимости от уровня сложности
            switch (_aiDifficulty)
            {
                case AIDifficulty.Easy:
                    // Случайные ходы (как было)
                    bestMove = allMoves[_random.Next(allMoves.Count)];
                    break;

                case AIDifficulty.Medium:
                    bestMove = FindBestMove(4); // Увеличили до 4
                    break;

                case AIDifficulty.Hard:
                    bestMove = FindBestMove(6); // Увеличили до 6
                    break;
            }

            // Если не нашли лучший ход, берем случайный
            if (bestMove == null)
                bestMove = allMoves[_random.Next(allMoves.Count)];

            // Выполняем ход
            ExecuteMove(bestMove);

            // Проверяем продолжение взятия
            if (bestMove.IsCapture)
            {
                var canContinueCapture = CheckAdditionalCaptures(bestMove.To.Row, bestMove.To.Column);
                if (canContinueCapture)
                {
                    MakeAIMove();
                    return;
                }
            }

            SwitchPlayer();
        }

        public enum AIDifficulty
        {
            Easy,    // Случайные ходы
            Medium,  // Минимакс с глубиной 2
            Hard     // Минимакс с глубиной 4
        }

        // НОВЫЙ МЕТОД: Получить все возможные ходы для текущего игрока
        private List<Move> GetAllPossibleMovesForCurrentPlayer()
        {
            var allMoves = new List<Move>();
            var currentColor = GetCurrentPlayerColor();

            // Сначала проверяем обязательные взятия
            var forcedCaptures = GetAllPossibleCapturesForCurrentPlayer();
            if (forcedCaptures.Count > 0)
            {
                return forcedCaptures;
            }

            // Если взятий нет, собираем все обычные ходы
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

        private Move FindBestMove(int depth)
        {
            var allMoves = GetAllPossibleMovesForCurrentPlayer();

            // Сортируем ходы: сначала взятия, потом остальные
            allMoves = allMoves.OrderByDescending(m => m.IsCapture).ThenBy(m => _random.Next()).ToList();

            Move bestMove = null;
            int bestEvaluation = int.MinValue;

            foreach (var move in allMoves)
            {
                var savedBoard = SaveBoardState();
                var savedPlayer = _currentPlayer;
                var savedGameOver = IsGameOver;

                ExecuteMove(move);

                // Если ход со взятием и можно продолжать, делаем это
                if (move.IsCapture)
                {
                    var canContinueCapture = CheckAdditionalCaptures(move.To.Row, move.To.Column);
                    if (canContinueCapture)
                    {
                        // Рекурсивно продолжаем взятие
                        var continuationMoves = GetAllPossibleMovesForCurrentPlayer();
                        if (continuationMoves.Count > 0)
                        {
                            var bestContinuation = FindBestMove(depth);
                            if (bestContinuation != null)
                            {
                                // Восстанавливаем состояние и применяем полную последовательность
                                RestoreBoardState(savedBoard);
                                _currentPlayer = savedPlayer;
                                IsGameOver = savedGameOver;

                                ExecuteMove(move);
                                ExecuteMove(bestContinuation);

                                // ИСПРАВЛЕНИЕ: переименовал переменную
                                int continuationEvaluation = Minimax(depth - 1, int.MinValue, int.MaxValue, false);

                                RestoreBoardState(savedBoard);
                                _currentPlayer = savedPlayer;
                                IsGameOver = savedGameOver;

                                if (continuationEvaluation > bestEvaluation)
                                {
                                    bestEvaluation = continuationEvaluation;
                                    bestMove = move;
                                }
                                continue;
                            }
                        }
                    }
                }

                // ИСПРАВЛЕНИЕ: это основная переменная evaluation
                int moveEvaluation = Minimax(depth - 1, int.MinValue, int.MaxValue, false);

                RestoreBoardState(savedBoard);
                _currentPlayer = savedPlayer;
                IsGameOver = savedGameOver;

                if (moveEvaluation > bestEvaluation)
                {
                    bestEvaluation = moveEvaluation;
                    bestMove = move;
                }
            }

            return bestMove;
        }
    }
}