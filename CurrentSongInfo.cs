using LyricsGoogler.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace LyricsGoogler;

public static class CurrentSongInfo
{
    /// <summary>
    /// Gets the artist name and title of the song playing now in Windows
    /// It does this using system media transport controls, so if you use a third-party
    /// player which is not controllable by these, then the function won't work
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MediaTransportControlsSessionException">
    /// Throws when media transport controls are not found (e.g. your player does not support them)
    /// </exception>
    /// <exception cref="MediaPropertiesException">
    /// Throws when unable to retrieve media properties
    /// </exception>
    /// <exception cref="EmptySongTitleOrArtistException">
    /// Throws when the song metadata does not include title or artist or they are empty strings
    /// </exception>
    public static async Task<(string artist, string title)> GetArtistAndTitle()
    {
        var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync() 
            ?? throw new MediaTransportControlsSessionException();

        var session = manager.GetCurrentSession() 
            ?? throw new MediaTransportControlsSessionException();

        var mediaProperties = await session.TryGetMediaPropertiesAsync() 
            ?? throw new MediaPropertiesException();

        var artist = mediaProperties.Artist;
        var title = mediaProperties.Title;
        if (artist.All(char.IsWhiteSpace) || title.All(char.IsWhiteSpace))
        {
            throw new EmptySongTitleOrArtistException();
        }

        return (artist, title);
    }
}
