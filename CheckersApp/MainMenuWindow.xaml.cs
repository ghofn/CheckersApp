using System.Windows;
using System.Windows.Controls;
using static CheckersApp.GameEngine;

namespace CheckersApp
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private AIDifficulty GetSelectedDifficulty()
        {
            if (EasyButton.IsChecked == true)
                return AIDifficulty.Easy;
            else if (MediumButton.IsChecked == true)
                return AIDifficulty.Medium;
            else if (HardButton.IsChecked == true)
                return AIDifficulty.Hard;
            else
                return AIDifficulty.Medium; // По умолчанию
        }

        private void PlayVsAIButton_Click(object sender, RoutedEventArgs e)
        {
            var difficulty = GetSelectedDifficulty();
            var gameWindow = new MainWindow(GameMode.PlayerVsAI, difficulty);
            gameWindow.Show();
            this.Close();
        }

        private void PlayTwoPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new MainWindow(GameMode.TwoPlayers);
            gameWindow.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}