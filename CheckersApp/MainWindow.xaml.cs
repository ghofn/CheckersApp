using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CheckersApp.GameEngine;

namespace CheckersApp
{
    public partial class MainWindow : Window
    {
        private GameEngine _gameEngine;
        private GameMode _gameMode;

        public MainWindow(GameMode gameMode = GameMode.TwoPlayers, AIDifficulty aiDifficulty = AIDifficulty.Medium)
        {
            InitializeComponent();
            _gameMode = gameMode;
            StartNewGame(aiDifficulty);

            if (_gameMode == GameMode.PlayerVsAI)
                Title += $" (против ИИ - {aiDifficulty})";
        }

        private void StartNewGame(AIDifficulty aiDifficulty = AIDifficulty.Medium)
        {
            _gameEngine = new GameEngine(_gameMode, aiDifficulty);
            UpdateBoard();
            UpdateStatus();
        }

        private void StartNewGame()
        {
            _gameEngine = new GameEngine(_gameMode);
            UpdateBoard();
            UpdateStatus();
        }

        private void UpdateBoard()
        {
            BoardItemsControl.ItemsSource = _gameEngine.GetBoardCells();
        }

        private void UpdateStatus()
        {
            StatusText.Text = _gameEngine.GetGameStatus();

            if (_gameEngine.IsGameOver)
            {
                StatusText.Foreground = new SolidColorBrush(Colors.Gold);
                StatusText.FontWeight = FontWeights.Bold;
            }
            else if (_gameEngine.GetGameStatus().Contains("ОБЯЗАН БИТЬ"))
            {
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                StatusText.FontWeight = FontWeights.Bold;
            }
            else
            {
                StatusText.Foreground = new SolidColorBrush(Colors.White);
                StatusText.FontWeight = FontWeights.Normal;
            }
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_gameMode == GameMode.PlayerVsAI && _gameEngine.CurrentPlayer == PieceColor.Black)
                return;

            if (sender is Border border && border.DataContext is BoardCell cell)
            {
                _gameEngine.HandleCellClick(cell.Row, cell.Column);
                UpdateBoard();
                UpdateStatus();

                if (_gameEngine.IsGameOver)
                {
                    MessageBox.Show(
                        $"Игра окончена!\n{_gameEngine.GetGameStatus()}",
                        "Поздравляем!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    StartNewGame();
                }
                else if (_gameMode == GameMode.PlayerVsAI && _gameEngine.CurrentPlayer == PieceColor.Black)
                {
                    await Task.Delay(500);
                    MakeAIMove();
                }
            }
        }

        private void MakeAIMove()
        {
            _gameEngine.MakeAIMove();
            UpdateBoard();
            UpdateStatus();

            if (_gameEngine.IsGameOver)
            {
                MessageBox.Show(
                    $"Игра окончена!\n{_gameEngine.GetGameStatus()}",
                    "Поздравляем!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                StartNewGame();
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenu = new MainMenuWindow();
            mainMenu.Show();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameEngine.IsGameOver ||
                (_gameMode == GameMode.PlayerVsAI && _gameEngine.CurrentPlayer == PieceColor.Black))
                return;

            ShowPossibleMovesHint();
        }

        private async void ShowPossibleMovesHint()
        {
            var allMoves = _gameEngine.GetAllPossibleMovesForCurrentPlayer();

            // Подсвечиваем все возможные ходы
            foreach (var move in allMoves)
            {
                // Подсвечиваем конечные позиции возможных ходов
                var cells = _gameEngine.GetBoardCells();
                foreach (var cell in cells)
                {
                    if (cell.Row == move.To.Row && cell.Column == move.To.Column)
                    {
                        cell.IsHighlighted = true;
                    }
                }
            }

            UpdateBoard();

            // Через 3 секунды убираем подсказку
            await Task.Delay(3000);

            _gameEngine.ClearHints();
            UpdateBoard();
        }
    }
}