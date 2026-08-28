using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace MaksLifeInChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow mainWindow; 
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint TimeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint TimeEndPeriod(uint uPeriod);
        protected override void OnStartup(StartupEventArgs e)
        {
            TimeBeginPeriod(1);
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            TimeEndPeriod(1);
            base.OnExit(e);
        }
    }

}
