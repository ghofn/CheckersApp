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
                Title += $" (против компьютера - {aiDifficulty})";
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

        private async void HintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameEngine.IsGameOver ||
                (_gameMode == GameMode.PlayerVsAI && _gameEngine.CurrentPlayer == PieceColor.Black))
                return;

            var bestMove = _gameEngine.GetBestMoveHint();

            if (bestMove != null)
            {
                // Показываем подсказку ИИ
                _gameEngine.ShowBestMoveHint(bestMove);
                UpdateBoard();

                // Временно меняем статус на подсказку
                string originalStatus = StatusText.Text;
                StatusText.Text = $"Совет: {GetMoveDescription(bestMove)}";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(156, 39, 176));

                // Ждем 5 секунд и убираем подсказку
                await Task.Delay(5000);

                _gameEngine.ClearHints();
                UpdateBoard();
                StatusText.Text = originalStatus;
                StatusText.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                StatusText.Text = "Нет возможных ходов";
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
                await Task.Delay(2000);
                UpdateStatus();
            }
        }

        private string GetMoveDescription(Move move)
        {
            string from = $"{(char)('A' + move.From.Column)}{8 - move.From.Row}";
            string to = $"{(char)('A' + move.To.Column)}{8 - move.To.Row}";

            if (move.IsCapture)
            {
                return $"Взять с {from} на {to}";
            }
            else
            {
                return $"Сходить с {from} на {to}";
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                UndoLastMove();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void UndoLastMove()
        {
            if (_gameEngine.UndoMove())
            {
                UpdateBoard();
                UpdateStatus();

                if (_gameMode == GameMode.PlayerVsAI && _gameEngine.CurrentPlayer == PieceColor.White)
                {
                    _gameEngine.UndoMove();
                    UpdateBoard();
                    UpdateStatus();
                }
            }
            else
            {
                MessageBox.Show("Нельзя отменить ход", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}