using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform;

namespace WebPreview.Fluent.Plugin
{
    public class WebView : NativeControlHost
    {
        private IntPtr _webViewWindow;
        private bool _initiated;
        private static readonly object WebViewLock = new();

        private static readonly StyledProperty<string> AddressProperty =
            AvaloniaProperty.Register<WebView, string>(nameof(Address), defaultBindingMode: BindingMode.TwoWay);

        private IntPtr _webviewGetWindow;

        /// <summary>
        /// The address of the webview
        /// </summary>
        public string Address
        {
            get => GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLong64b(IntPtr hWnd, int nIndex, IntPtr value);

        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr webview_create(int debug, IntPtr window);

        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern void webview_navigate(IntPtr webview, [MarshalAs(UnmanagedType.LPStr)] string url);

        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern void webview_destroy(IntPtr webview);


        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr webview_get_window(IntPtr webview);

        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern void webview_set_size(
            IntPtr webview,
            int width,
            int height,
            int hint);

        [DllImport("webview", CallingConvention = CallingConvention.Cdecl)]
        private static extern void webview_set_title(IntPtr webview, [MarshalAs(UnmanagedType.LPStr)] string title);

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (!_initiated)
            {
                lock (WebViewLock)
                {
                    if (!_initiated)
                    {
                        IntPtr intPtr = IntPtr.Zero;
                        _webViewWindow = webview_create(0, intPtr);
                        webview_set_title(_webViewWindow, "bla12345");
                        webview_set_size(_webViewWindow, 1, 1, 0);
                        webview_navigate(_webViewWindow, Address);
                        _webviewGetWindow = webview_get_window(_webViewWindow);
                        SetWindowLong64b(_webviewGetWindow, -16,
                            new IntPtr(0x800000 | 0x10000000 | 0x40000000 | 0x800000 | 0x10000 | 0x0004));
                        _initiated = true;
                        return new PlatformHandle(_webviewGetWindow, "HWND");
                    }
                }
            }

            return new PlatformHandle(_webviewGetWindow, "HWND");
        }


        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            lock (WebViewLock)
            {
                webview_destroy(_webViewWindow);
                _initiated = false;
                _webviewGetWindow = IntPtr.Zero;
                _webViewWindow = IntPtr.Zero;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            webview_set_size(_webViewWindow, (int)finalSize.Width - 10, (int)finalSize.Height - 10, 0);
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
#pragma warning disable 8631
            base.OnPropertyChanged(change);
#pragma warning restore 8631
            if (!_initiated) return;
            lock (WebViewLock)
            {
                if (!_initiated) return;
                if (change.Property == AddressProperty && _webViewWindow != IntPtr.Zero)
                {
                    try
                    {
                        webview_navigate(_webViewWindow, Address);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}