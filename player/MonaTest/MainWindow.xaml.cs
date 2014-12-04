using System;
using System.Collections.Generic;
using System.Linq;
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
using MonaFramework.Controls;
using System.IO;

namespace MonaTest
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MonaWindow
    {
        public MainWindow() : base()
        {
            InitializeComponent();
            init();
        }

        public void init()
        {
            /*player.LoadedBehavior = MediaState.Manual;*/
            player.Source = new Uri(Directory.GetCurrentDirectory() + "\\sounds\\kizuna.mp3", UriKind.Absolute);
        }

        private void playMedia(object sender, RoutedEventArgs e)
        {
            player.Play();
        }

        private void pauseMedia(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private void stopMedia(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }
    }
}
