using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using GitLurker.UI.Models;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input;

namespace GitLurker.UI.Services
{
    public class SurfaceDialService : IDisposable
    {
        #region Fields

        private RadialController _controller;

        #endregion

        #region events

        public event EventHandler ButtonClicked;

        public event EventHandler RotatedLeft;

        public event EventHandler RotatedRight;

        #endregion

        #region Methods

        public async Task Initialize(Window window)
        {
            var interop = (IRadialControllerInterop)System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMarshal.GetActivationFactory(typeof(RadialController));
            var guid = typeof(RadialController).GetInterface("IRadialController").GUID;
            var handle = new WindowInteropHelper(window).Handle;
            _controller = interop.CreateForWindow(handle, ref guid);

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var pngPath = Path.Combine(Path.GetDirectoryName(path), "GitLurker.png");
            var file = await StorageFile.GetFileFromPathAsync(pngPath);
            _controller.Menu.Items.Add(RadialControllerMenuItem.CreateFromIcon("Git Lurker", RandomAccessStreamReference.CreateFromFile(file)));

            _controller.ButtonClicked += Controller_ButtonClicked;
            _controller.RotationChanged += Controller_RotationChanged;

            SetDefaultItems(handle);
        }

        private void Controller_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            if (args.RotationDeltaInDegrees > 0)
            {
                RotatedRight?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                RotatedLeft?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Controller_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args) 
            => ButtonClicked?.Invoke(this, EventArgs.Empty);

        private void SetDefaultItems(IntPtr hwnd)
        {
            RadialControllerConfiguration config;
            var radialControllerConfigInterop = (IRadialControllerConfigurationInterop)System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMarshal.GetActivationFactory(typeof(RadialControllerConfiguration));
            var guid = typeof(RadialControllerConfiguration).GetInterface("IRadialControllerConfiguration").GUID;

            config = radialControllerConfigInterop.GetForWindow(hwnd, ref guid);
            config.SetDefaultMenuItems(new RadialControllerSystemMenuItemKind[0]);
        }

        public void Dispose()
        {
            _controller.ButtonClicked -= Controller_ButtonClicked;
            _controller.RotationChanged -= Controller_RotationChanged;
        }

        #endregion
    }
}
