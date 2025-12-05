using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CheckersApp
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
            Loaded += MainMenuWindow_Loaded;
        }

        private void MainMenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SoundManager.StartBackgroundMusic();
            UpdateMusicButton();
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
                return AIDifficulty.Medium;
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

        private void SkinsButton_Click(object sender, RoutedEventArgs e)
        {
            var skinsWindow = new SimpleSkinsWindow();
            skinsWindow.Owner = this;
            skinsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (skinsWindow.ShowDialog() == true)
            {
                // Показываем сообщение об успехе
                SkinsButton.Content = "✓ Скин изменен";
                SkinsButton.Background = new SolidColorBrush(Color.FromRgb(46, 204, 113));

                // Через 2 секунды возвращаем обратно
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        SkinsButton.Content = "🎨 Скины шашек";
                        SkinsButton.Background = new SolidColorBrush(Color.FromRgb(74, 111, 165));
                    });
                });
            }
        }

        private void MusicToggleButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.ToggleSounds();
            UpdateMusicButton();
        }

        private void UpdateMusicButton()
        {
            if (SoundManager.SoundsEnabled)
            {
                MusicToggleButton.Content = "🎵 Звуки: ВКЛ";
                MusicToggleButton.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else
            {
                MusicToggleButton.Content = "🎵 Звуки: ВЫКЛ";
                MusicToggleButton.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}