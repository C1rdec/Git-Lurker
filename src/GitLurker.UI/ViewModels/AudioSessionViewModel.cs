namespace GitLurker.UI.ViewModels;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using GitLurker.UI.Models;
using GitLurker.UI.Services;
using Lurker.Audio;
using MahApps.Metro.Controls;

public class AudioSessionViewModel : ItemViewModelBase
{
    private int _currentVolume;
    private ProgressBar _progressBar;
    private MousePosition _currentMousePosition;
    private MouseService _mouseService;
    private AudioSession _session;

    public AudioSessionViewModel(AudioSession session, MouseService mouseService)
    {
        _session = session;
        _mouseService = mouseService;
    }

    public BitmapSource IconSource
    {
        get
        {
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
            _session.Icon.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }
    }

    public bool VolumeVisible => _currentMousePosition != null;

    public string SessionName => _session.Name;

    public int Volume 
    {
        get => _session.Volume;
        set
        {
            _session.Volume = value;
            NotifyOfPropertyChange();
        }
    }

    public override string Id => _session.Id;


    public void MouseDown(MouseButtonEventArgs e)
    {
        var position = e.GetPosition(e.Source as UIElement);
        _progressBar = e.Source as ProgressBar;

        if (_progressBar != null)
        {
            var volume = position.X / _progressBar.ActualWidth * 100;
            Volume = Convert.ToInt32(volume);
            _currentVolume = Volume;
        }

        _currentMousePosition = _mouseService.GetCurrentPosition();
        _mouseService.MousePositionChanged += MouseService_MousePositionChanged;
        NotifyOfPropertyChange(() => VolumeVisible);
    }

    public async void OnMouseLeave()
    {
        _mouseService.MousePositionChanged -= MouseService_MousePositionChanged;
        _currentMousePosition = null;
        _currentVolume = 0;

        await Task.Delay(800);
        NotifyOfPropertyChange(() => VolumeVisible);
    }

    private void MouseService_MousePositionChanged(object sender, MousePosition e)
    {
        if (_currentMousePosition == null)
        {
            return;
        }

        var difference = e.X - _currentMousePosition.X;
        var i = difference / _progressBar.ActualWidth * 100;

        Volume = _currentVolume + Convert.ToInt32(i);
    }
}
