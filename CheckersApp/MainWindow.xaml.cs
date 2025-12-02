using System;
using System.Linq;
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
        private AIDifficulty _aiDifficulty;

        public MainWindow(GameMode gameMode = GameMode.TwoPlayers, AIDifficulty aiDifficulty = AIDifficulty.Medium)
        {
            InitializeComponent();
            _gameMode = gameMode;
            _aiDifficulty = aiDifficulty;

            // Подписываемся на событие смены скина
            SkinManager.OnSkinChanged += OnSkinChanged;

            StartNewGame();
            SoundManager.StartBackgroundMusic();
        }

        private void StartNewGame()
        {
            _gameEngine = new GameEngine(_gameMode, _aiDifficulty);
            UpdateBoard();
            UpdateStatus();
            UpdateStatistics();
            SoundManager.ResumeBackgroundMusic();
        }

        private void UpdateBoard()
        {
            BoardItemsControl.ItemsSource = null;
            BoardItemsControl.ItemsSource = _gameEngine.GetBoardCells();
        }

        private void UpdateStatus()
        {
            StatusText.Text = _gameEngine.GetGameStatus();

            if (_gameEngine.IsGameOver)
            {
                StatusText.Foreground = Brushes.Gold;
                StatusText.FontWeight = FontWeights.Bold;
            }
            else if (_gameEngine.GetGameStatus().Contains("Обязаны бить"))
            {
                StatusText.Foreground = Brushes.Red;
                StatusText.FontWeight = FontWeights.Bold;
            }
            else
            {
                StatusText.Foreground = Brushes.White;
                StatusText.FontWeight = FontWeights.Normal;
            }
        }

        private void UpdateStatistics()
        {
            StatisticsText.Text = $" | Белые: {_gameEngine.WhitePieces} | Черные: {_gameEngine.BlackPieces} | Ходы: {_gameEngine.MoveCount}";
        }

        // Метод обновления скина
        private void OnSkinChanged()
        {
            UpdateBoard();
        }

        private void SkinsButton_Click(object sender, RoutedEventArgs e)
        {
            var skinsWindow = new SimpleSkinsWindow();
            skinsWindow.Owner = this;
            skinsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (skinsWindow.ShowDialog() == true)
            {
                // Показываем сообщение об успехе
                StatusText.Text = "✓ Скин изменен!";
                StatusText.Foreground = Brushes.LightGreen;

                // Через 2 секунды возвращаем статус
                Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => UpdateStatus());
                });
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
                UpdateStatistics();

                if (_gameEngine.IsGameOver)
                {
                    await ShowGameOverMessage();
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
            UpdateStatistics();

            if (_gameEngine.IsGameOver)
            {
                ShowGameOverMessage();
            }
        }

        private async Task ShowGameOverMessage()
        {
            await Task.Delay(1000);

            MessageBox.Show(
                $"Игра окончена!\n{_gameEngine.GetGameStatus()}",
                "Поздравляем!",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            StartNewGame();
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
                _gameEngine.ClearHints();

                var cells = _gameEngine.GetBoardCells().ToList();

                var fromCell = cells.FirstOrDefault(c =>
                    c.Row == bestMove.From.Row && c.Column == bestMove.From.Column);

                var toCell = cells.FirstOrDefault(c =>
                    c.Row == bestMove.To.Row && c.Column == bestMove.To.Column);

                if (fromCell != null)
                {
                    fromCell.IsSelected = true;
                }

                if (toCell != null)
                {
                    toCell.IsPossibleMove = true;
                }

                UpdateBoard();

                string originalStatus = StatusText.Text;
                string fromPos = $"{(char)('A' + bestMove.From.Column)}{8 - bestMove.From.Row}";
                string toPos = $"{(char)('A' + bestMove.To.Column)}{8 - bestMove.To.Row}";

                StatusText.Text = $"Подсказка: {fromPos} → {toPos}";
                StatusText.Foreground = Brushes.LightGreen;

                await Task.Delay(3000);

                _gameEngine.ClearHints();
                UpdateBoard();
                StatusText.Text = originalStatus;
                UpdateStatus();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                MessageBox.Show("Отмена хода в разработке",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (e.Key == Key.H)
            {
                HintButton_Click(null, null);
            }
            else if (e.Key == Key.F5)
            {
                RestartButton_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                BackToMenuButton_Click(null, null);
            }
            base.OnKeyDown(e);
        }

        private void SoundToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.ToggleSounds();
            var button = sender as Button;
            if (button != null)
            {
                button.Content = SoundManager.SoundsEnabled ? "🔊 Звуки" : "🔇 Звуки";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            SoundManager.StopBackgroundMusic();
            SkinManager.OnSkinChanged -= OnSkinChanged;
            base.OnClosed(e);
        }
    }
}