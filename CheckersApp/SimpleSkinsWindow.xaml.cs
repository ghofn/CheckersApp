using System.Linq;
using System.Windows;

namespace CheckersApp
{
    public partial class SimpleSkinsWindow : Window
    {
        public SimpleSkinsWindow()
        {
            InitializeComponent();
            Loaded += SimpleSkinsWindow_Loaded;
        }

        private void SimpleSkinsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем все скины
            SkinsListBox.ItemsSource = PieceSkin.GetAllSkins();

            // Выделяем текущий скин
            var currentSkin = SkinManager.CurrentSkin;
            SkinsListBox.SelectedItem = PieceSkin.GetAllSkins()
                .FirstOrDefault(s => s.Name == currentSkin.Name);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SkinsListBox.SelectedItem is PieceSkin skin)
            {
                SkinManager.CurrentSkin = skin;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите скин!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}