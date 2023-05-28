using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for RenderSurfaceView.xaml
    /// </summary>
    public partial class RenderSurfaceView : UserControl, IDisposable
    {
        private enum Win32Msg
        {
            WM_SIZING = 0x0214,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_SIZE = 0x0005
        }
        private RenderSurfaceHost? _host;
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderSurfaceView"/> class.
        /// </summary>
        public RenderSurfaceView()
        {
            InitializeComponent();
            Loaded += OnRenderSurfaceViewLoaded;
        }
        private void OnRenderSurfaceViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnRenderSurfaceViewLoaded;
            _host = new RenderSurfaceHost(ActualWidth, ActualHeight);
            _host.MessageHook += new HwndSourceHook(HostMsgFilter);
            Content = _host;
        }
        private IntPtr HostMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Win32Msg)msg)
            {
                case Win32Msg.WM_SIZING: throw new Exception();
                case Win32Msg.WM_ENTERSIZEMOVE: throw new Exception();
                case Win32Msg.WM_EXITSIZEMOVE: throw new Exception();
                case Win32Msg.WM_SIZE: break;
                default: break;
            }
            return IntPtr.Zero;
        }
        #region IDisposable support
        private bool disposedValue;
        /// <summary>
        /// Disposes off the resources used by the <see cref="RenderSurfaceView"/> class.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            if (disposing) _host?.Dispose();
            disposedValue = true;
        }
        /// <summary>
        /// Releases all unmanaged and managed resources used by this object.
        /// </summary>
        /// <remarks>
        /// This method is called by the `using` statement and the `Dispose()` method of other objects.
        /// Do not override this method.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
