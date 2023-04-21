using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LyricsGoogler;

public class NotifyIconBuilder
{
    private ContextMenuStrip? _contextMenu;
    private const string RUN_REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public NotifyIcon Build()
    {
        return new()
        {
            Text = "Lyrics Googler",
            Visible = true,
            Icon = new Icon("icon.ico"),
            ContextMenuStrip = _contextMenu,
        };
    }

    public NotifyIconBuilder WithExitMenuItem()
    {
        ToolStripMenuItem item = new("Exit", null, (sender, e) =>
        {
            Application.Exit();
        });
        _contextMenu ??= new ContextMenuStrip();
        _contextMenu.Items.Add(item);
        return this;
    }

    public NotifyIconBuilder WithOpenConfigFileMenuItem()
    {
        ToolStripMenuItem item = new("Open Config File", null, (sender, e) =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "config.yaml",
                UseShellExecute = true,
            });
        });
        _contextMenu ??= new ContextMenuStrip();
        _contextMenu.Items.Add(item);
        return this;
    }

    public NotifyIconBuilder WithRunAtStartupMenuItem()
    {
        ToolStripMenuItem item = new("Run at Startup", null, (sender, e) =>
        {
            if (sender is not ToolStripMenuItem menuItem)
            {
                return;
            }

            menuItem.Checked = !menuItem.Checked;

            SetRunAtStartup(menuItem.Checked);
        })
        {
            Checked = RunsAtStartup()
        };
        _contextMenu ??= new ContextMenuStrip();
        _contextMenu.Items.Add(item);
        return this;
    }

    private static void SetRunAtStartup(bool isEnabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RUN_REGISTRY_KEY, true)
            ?? throw new RegistryKeyNotFoundException(RUN_REGISTRY_KEY);

        if (isEnabled)
        {
            key.SetValue("LyricsGoogler", Application.ExecutablePath);
        }
        else
        {
            key.DeleteValue("LyricsGoogler", false);
        }
    }

    private static bool RunsAtStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RUN_REGISTRY_KEY, true)
            ?? throw new RegistryKeyNotFoundException(RUN_REGISTRY_KEY);

        return Application.ExecutablePath.Equals(key.GetValue("LyricsGoogler"));
    }

    public class RegistryKeyNotFoundException : Exception
    {
        public RegistryKeyNotFoundException(string key)
            : base($"{key} registry key not found.")
        {
        }
    }
}
