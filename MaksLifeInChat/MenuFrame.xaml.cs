using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MaksLifeInChat
{
    /// <summary>
    /// Логика взаимодействия для MenuFrame.xaml
    /// </summary>
    public partial class MenuFrame : UserControl
    {
        bool isSetting = false;
        public MenuFrame()
        {
            InitializeComponent();
            LoadSettings();
        }

        void LoadSettings()
        {
            string[] settings = File.ReadAllLines("settings");
            App.mainWindow.Background = new SolidColorBrush(Color.FromRgb((byte)Convert.ToInt16(settings[0]), (byte)Convert.ToInt16(settings[1]), (byte)Convert.ToInt16(settings[2])));
            App.volumeSettings = double.Parse(settings[3].Replace('.', ','));
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            App.mainWindow.StartGame();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            isSetting = true;
            volumeSlider.Value = App.volumeSettings;
            playButton.Visibility = Visibility.Collapsed;
            settingsButton.Visibility = Visibility.Collapsed;
            infoButton.Visibility = Visibility.Collapsed;
            exitButton.Visibility = Visibility.Collapsed;
            backButton.Visibility = Visibility.Visible;
            saveButton.Visibility = Visibility.Visible;
            volumeLabel.Visibility = Visibility.Visible;
            volumeSlider.Visibility = Visibility.Visible;
            greenSlider.Visibility = Visibility.Visible;
            blueSlider.Visibility = Visibility.Visible;
            redSlider.Visibility = Visibility.Visible;
            fonLabel.Visibility = Visibility.Visible;
            settingsLabel.Visibility = Visibility.Visible;
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            App.mainWindow.Close();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            playButton.Visibility = Visibility.Visible;
            settingsButton.Visibility = Visibility.Visible;
            infoButton.Visibility = Visibility.Visible;
            exitButton.Visibility = Visibility.Visible;
            backButton.Visibility = Visibility.Collapsed;
            saveButton.Visibility = Visibility.Collapsed;
            volumeLabel.Visibility = Visibility.Collapsed;
            volumeSlider.Visibility = Visibility.Collapsed;
            greenSlider.Visibility = Visibility.Collapsed;
            blueSlider.Visibility = Visibility.Collapsed;
            redSlider.Visibility = Visibility.Collapsed;
            fonLabel.Visibility = Visibility.Collapsed;
            settingsLabel.Visibility = Visibility.Collapsed;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            App.mainWindow.Background = new SolidColorBrush(Color.FromRgb((byte)redSlider.Value, (byte)greenSlider.Value, (byte)blueSlider.Value));
            App.volumeSettings = volumeSlider.Value;
            List<string> settings = [Convert.ToInt16(redSlider.Value).ToString(), Convert.ToInt16(greenSlider.Value).ToString(), Convert.ToInt16(blueSlider.Value).ToString(), volumeSlider.Value.ToString()];
            File.WriteAllLines("settings", settings);
            backButton_Click(sender, e);
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetting)
                this.Background = new SolidColorBrush( Color.FromRgb((byte)redSlider.Value, (byte)greenSlider.Value, (byte)blueSlider.Value));
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App.mainWindow._mediaPlayer.Volume = volumeSlider.Value;
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Информация:\r\nПриветствуйте новичков, стукайте чаттерсов, делайте шаурму, восстанавливайте человечность и грабьте караваны, чтобы подготовиться к битве с модерами!\r\r\nУправление:\r\nWASD - двигаться\r\nЛКМ - атаковать / перемещать предметы\r\nПКМ - приветствовать / расщеплять предметы\r\nShift - бег\r\nCtrl - режим кары\r\nF11 - полноэкранный режим\r\nSpace - перекат\r\nEsc - пауза\r\nTab - показать/скрыть дополнительную информацию", "Справка");
        }
    }
}
