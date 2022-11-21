using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Ecm.CaptureViewer
{
    public partial class MediaPlayer
    {
        public static readonly DependencyProperty FilePathProperty =
           DependencyProperty.Register("FilePath", typeof(string), typeof(MediaPlayer));

        public MediaPlayer()
        {
            InitializeComponent();

            IsPlaying(false);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += TimerTick;
        }

        public string FilePath
        {
            get { return GetValue(FilePathProperty) as string; }
            set { SetValue(FilePathProperty, value); }
        }

        private void IsPlaying(bool value)
        {
            btnStop.IsEnabled = value;
            btnPlayPause.IsEnabled = value;
            sldSeek.IsEnabled = value;
            mediaElement.Visibility = Visibility.Visible;

            if (!value)
            {
                sldSeek.Value = 0;
                btnPlayPause.IsChecked = false;
                mediaElement.Visibility = Visibility.Hidden;
            }

            if (!mediaElement.HasVideo || !value)
            {
                mediaContainer.Background = TryFindResource("PlayerPreviewBrush") as ImageBrush;
                if (mediaContainer.Background == null)
                {
                    var image = new BitmapImage(new Uri("pack://application:,,,/CaptureViewer;component/Resources/playerpreview.jpg"));
                    mediaContainer.Background = new ImageBrush(image);
                }
            }
            else
            {
                mediaContainer.Background = Brushes.Black;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!_isDragging)
            {
                _seekByTimer = true;
                sldSeek.Value = mediaElement.Position.TotalSeconds;
            }
        }

        private void MEDIAElementLoaded(object sender, RoutedEventArgs e)
        {
            btnPlayPause.IsChecked = true;
            BtnPlayPauseClick(null, null);
        }

        private void MEDIAElementMediaOpened(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed;
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = mediaElement.NaturalDuration.TimeSpan;
                sldSeek.Maximum = ts.TotalSeconds;
                sldSeek.SmallChange = 1;
                sldSeek.LargeChange = Math.Min(10, ts.Seconds / 10);

                btnPlayPause.IsChecked = true;
                mediaElement.Play();
                IsPlaying(true);
            }

            _timer.Start();
        }

        private void MEDIAElementMediaEnded(object sender, RoutedEventArgs e)
        {
            BtnStopClick(null, null);
        }

        private void MEDIAElementMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Visible;
            IsPlaying(false);
        }

        private void MEDIAElementUnloaded(object sender, RoutedEventArgs e)
        {
            mediaElement.Close();
        }

        private void BtnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (btnPlayPause.IsChecked == true)
            {
                mediaElement.Play();
            }
            else
            {
                mediaElement.Pause();
            }

            IsPlaying(true);
        }

        private void BtnStopClick(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            IsPlaying(false);
            btnPlayPause.IsEnabled = true;
        }

        private void SldSeekDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SldSeekDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(sldSeek.Value);
            _isDragging = false;
        }

        private void SldSeekValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_seekByTimer && !_isDragging) // User click on a position on slider to move the bar
            {
                mediaElement.Position = TimeSpan.FromSeconds(sldSeek.Value);
            }

            _seekByTimer = false;
        }

        private readonly DispatcherTimer _timer;
        private bool _isDragging;
        private bool _seekByTimer = true;
    }
}
