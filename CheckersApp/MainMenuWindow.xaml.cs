using System.Windows;

namespace CheckersApp
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void PlayVsAIButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new MainWindow(GameMode.PlayerVsAI); // Должен быть PlayerVsAI
            gameWindow.Show();
            this.Close();
        }

        private void PlayTwoPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new MainWindow(GameMode.TwoPlayers); // Должен быть TwoPlayers
            gameWindow.Show();
            this.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки будут добавлены в следующем обновлении!", "Настройки",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}