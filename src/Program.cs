using LyricsGoogler.Exceptions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS;

namespace LyricsGoogler;

internal static class Program
{
    static void Main()
    {
        using var notifyIcon = new NotifyIconBuilder()
            .WithRunAtStartupMenuItem()
            .WithExitMenuItem()
            .Build();

        GlobalHotKeyManager.Register(MOD_WIN | MOD_SHIFT, Keys.L, async () => await Run());
        GlobalHotKeyManager.Listen();
    }

    private static async Task Run()
    {
        string artist, title;
        try
        {
            (artist, title) = await CurrentSongInfo.GetArtistAndTitle();
        }
        catch (LyricsGooglerException e)
        {
            MessageBox.Show(e.Message);
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
}
