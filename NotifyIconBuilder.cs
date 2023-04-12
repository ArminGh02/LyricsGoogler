using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;

namespace LyricsGoogler;

public class NotifyIconBuilder
{
    private ContextMenuStrip? _contextMenu;

    public NotifyIcon Build()
    {
        return new()
        {
            Visible = true,
            Icon = SystemIcons.Application,
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

    public NotifyIconBuilder WithRunAtStartupMenuItem()
    {
        ToolStripMenuItem item = new("Run at Startup", null, (sender, e) =>
        {
            if (sender is not ToolStripMenuItem menuItem)
            {
                return;
            }
            
            menuItem.Checked = !menuItem.Checked;
            
            if (!SetRunAtStartup(menuItem.Checked))
            {
                MessageBox.Show("Could not configure to run at startup.");
            }
        });
        _contextMenu ??= new ContextMenuStrip();
        _contextMenu.Items.Add(item);
        return this;
    }

    private static bool SetRunAtStartup(bool isEnabled)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (key is null)
        {
            return false;
        }

        if (isEnabled)
        {
            key.SetValue("LyricsGoogler", Application.ExecutablePath);
        }
        else
        {
            key.DeleteValue("LyricsGoogler", false);
        }
        return true;
    }
}
