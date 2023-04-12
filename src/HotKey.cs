using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace LyricsGoogler;

internal record HotKey(HOT_KEY_MODIFIERS Modifiers, Keys Key);

internal static class GlobalHotKeyManager
{
    private static int _hotKeyID = 0;

    /// <summary>
    /// Hidden form that hotkeys are bound to
    /// </summary>
    private static readonly Form _form;

    static GlobalHotKeyManager()
    {
        _form = new Form()
        {
            ShowInTaskbar = false,
            WindowState = FormWindowState.Minimized,
        };
        _form.Load += (sender, e) => { _form.Visible = false; };
    }

    public static void Register(HotKey hotKey, Action action)
    {
        var formHandle = _form.Handle;
        var hotKeyID = Interlocked.Increment(ref _hotKeyID);
        if (!PInvoke.RegisterHotKey(new HWND(formHandle), hotKeyID, hotKey.Modifiers, (uint)hotKey.Key))
        {
            var err = Marshal.GetLastWin32Error();
            MessageBox.Show($"Could not register hotkey. error code: {err}");
            Application.Exit();
        }

        _form.FormClosing += (sender, e) =>
        {
            if (!PInvoke.UnregisterHotKey(new HWND(formHandle), hotKeyID))
            {
                var err = Marshal.GetLastWin32Error();
                MessageBox.Show($"Could not unregister hotkey. error code: {err}");
            }
        };
        Application.AddMessageFilter(new HotkeyMessageFilter(formHandle, action));
    }

    public static void Listen()
    {
        Application.Run(_form);
    }

    private class HotkeyMessageFilter : IMessageFilter
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly IntPtr _formHandle;
        private readonly Action _hotkeyAction;

        public HotkeyMessageFilter(IntPtr formHandle, Action hotkeyAction)
        {
            _formHandle = formHandle;
            _hotkeyAction = hotkeyAction;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _hotKeyID && m.HWnd == _formHandle)
            {
                _hotkeyAction();
                return true;
            }

            return false;
        }
    }
}
