using LyricsGoogler.Exceptions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyricsGoogler;

internal static class Program
{
    static void Main()
    {
        var config = Config.Parse("config.yaml");

        using var notifyIcon = new NotifyIconBuilder()
            .WithRunAtStartupMenuItem()
            .WithOpenConfigFileMenuItem()
            .WithExitMenuItem()
            .Build();

        GlobalHotKeyManager.Register(config.HotKey, async () => await Run());
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
