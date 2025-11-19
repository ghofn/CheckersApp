using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

namespace CheckersApp
{
    public partial class MainWindow : Window
    {
        private GameEngine _gameEngine;
        private GameMode _gameMode;

        // Обновленный конструктор с параметром режима
        public MainWindow(GameMode gameMode = GameMode.TwoPlayers)
        {
            InitializeComponent();

            if (_gameMode == GameMode.PlayerVsAI)
                TitleText.Text += " (против ИИ)";
            _gameMode = gameMode;
            StartNewGame();

            // Обновляем заголовок в зависимости от режима
            if (_gameMode == GameMode.PlayerVsAI)
                Title += " (против ИИ)";
        }

        private void StartNewGame()
        {
            _gameEngine = new GameEngine(_gameMode);
            UpdateBoard();э
            UpdateStatus();
        }

        private void UpdateBoard()
        {
            BoardItemsControl.ItemsSource = _gameEngine.GetBoardCells();
        }

        private void UpdateStatus()
        {
            StatusText.Text = _gameEngine.GetGameStatus();

            // Добавим отладочную информацию
            DebugText.Text = $"Режим: {_gameMode} | Игрок: {_gameEngine.CurrentPlayer}";

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
            // Блокируем ввод во время хода ИИ
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
                    // Ход ИИ с небольшой задержкой для естественности
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
    }
}