using System;
using System.Collections.Generic;
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
        public MenuFrame()
        {
            InitializeComponent();
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            App.mainWindow.gamePlayerMapGrid.Children.Clear();
            App.mainWindow.gameBuildingMapGrid.Children.Clear();
            App.mainWindow.gameUnitMapGrid.Children.Clear();
            App.mainWindow.StartGame();
        }
    }
}
