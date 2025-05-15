using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CineLingo.Page
{
    public partial class FullscreenWindow : Window
    {
        public LibVLC _libVLC;
        public LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        public FullscreenWindow(LibVLC libVLC, LibVLCSharp.Shared.MediaPlayer mediaPlayer)
        {
            InitializeComponent();

            _libVLC = libVLC;
            _mediaPlayer = mediaPlayer;

            FullscreenPlayer.MediaPlayer = _mediaPlayer;
        }

        private void ExitFullscreen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
                _mediaPlayer.Pause();
            else
                _mediaPlayer.Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            this.Close();
        }
    }
}
