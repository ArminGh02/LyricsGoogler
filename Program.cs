using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyricsGoogler
{
    public static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;

        public static void Main()
        {
            using NotifyIcon notifyIcon = new();
            notifyIcon.Icon = SystemIcons.Application;
            notifyIcon.Visible = true;

            using Form form = new();
            form.ShowInTaskbar = false;
            form.WindowState = FormWindowState.Minimized;
            form.Load += (sender, e) => { form.Visible = false; };

            ToolStripMenuItem exitMenuItem = new("Exit", null, (sender, e) => { Application.Exit(); });
            ToolStripMenuItem runAtStartupMenuItem = new("Run at Startup", null, (sender, e) =>
            {
                if (sender is not ToolStripMenuItem menuItem)
                {
                    return;
                }
                menuItem.Checked = !menuItem.Checked;
                SetRunAtStartup(menuItem.Checked);
            });

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add(runAtStartupMenuItem);
            notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);

            var formHandle = form.Handle;
            var hotkeyModifiers = (uint)(Keys.Control | Keys.Shift | Keys.Alt);
            var hotkeyKey = (uint)Keys.L;
            
            if (RegisterHotKey(formHandle, HOTKEY_ID, hotkeyModifiers, hotkeyKey))
            {
                form.FormClosing += (sender, e) => { UnregisterHotKey(formHandle, HOTKEY_ID); };
                Application.AddMessageFilter(new HotkeyMessageFilter(formHandle, async () => await Run()));
            }

            Application.Run(form);
        }

        private static async Task Run()
        {
            string artist, title;
            try
            {
                (artist, title) = await SongInfoFetcher.FetchArtistAndTitle();
            }
            catch (LyricsGooglerException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            var query = $"\"{artist}\" \"{title}\" lyrics";
            var url = $"https://www.google.com/search?q={query}";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }

        private static void SetRunAtStartup(bool isEnabled)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key is null)
            {
                return;
            }
            if (isEnabled)
            {
                key.SetValue("LyricsGoogler", Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue("LyricsGoogler", false);
            }
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
                if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID && m.HWnd == _formHandle)
                {
                    _hotkeyAction?.Invoke();
                    return true;
                }

                return false;
            }
        }
    }
}