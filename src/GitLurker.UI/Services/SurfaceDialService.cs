using System;
using System.IO;
using System.Linq;
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
        private bool _holding;
        private bool _ignoreNextClick;

        #endregion

        #region events

        public event EventHandler ButtonClicked;

        public event EventHandler ButtonHolding;

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

            var root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = Path.Combine(Path.GetDirectoryName(root), "GitLurker.png");
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            _controller.Menu.Items.Add(RadialControllerMenuItem.CreateFromIcon("Git Lurker", RandomAccessStreamReference.CreateFromFile(file)));

            _controller.ButtonClicked += Controller_ButtonClicked;
            _controller.RotationChanged += Controller_RotationChanged;
            _controller.ButtonHolding += Controller_ButtonHolding;
            _controller.ButtonReleased += Controller_ButtonReleased;

            RemoveSystemItems(handle);
        }

        private void Controller_ButtonReleased(RadialController sender, RadialControllerButtonReleasedEventArgs args)
        {
            if (_holding)
            {
                _holding = false;
                _ignoreNextClick = true;
                ButtonHolding?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Controller_ButtonHolding(RadialController sender, RadialControllerButtonHoldingEventArgs args)
            => _holding = true;

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
        {
            if (_ignoreNextClick)
            {
                _ignoreNextClick = false;
                return;
            }

            ButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveSystemItems(IntPtr hwnd)
        {
            RadialControllerConfiguration config;
            var radialControllerConfigInterop = (IRadialControllerConfigurationInterop)System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeMarshal.GetActivationFactory(typeof(RadialControllerConfiguration));
            var guid = typeof(RadialControllerConfiguration).GetInterface("IRadialControllerConfiguration").GUID;

            config = radialControllerConfigInterop.GetForWindow(hwnd, ref guid);
            config.IsMenuSuppressed = true;
            config.SetDefaultMenuItems(Enumerable.Empty<RadialControllerSystemMenuItemKind>());
        }

        public void Dispose()
        {
            _controller.ButtonClicked -= Controller_ButtonClicked;
            _controller.RotationChanged -= Controller_RotationChanged;
        }

        #endregion
    }
}
