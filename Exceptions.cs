using System;

namespace LyricsGoogler.Exceptions;

public abstract class LyricsGooglerException : Exception
{
    public LyricsGooglerException()
    {
    }
    public LyricsGooglerException(string message)
        : base(message)
    {
    }
    public LyricsGooglerException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class MediaTransportControlsSessionException : LyricsGooglerException
{
    private const string DEFAULT_MESSAGE =
        "Unable to start a media transport controls session. " +
        "Make sure the song is accessible via Windows media transport controls.";

    public MediaTransportControlsSessionException(string message = DEFAULT_MESSAGE)
        : base(message)
    {
    }
    public MediaTransportControlsSessionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class MediaPropertiesException : LyricsGooglerException
{
    private const string DEFAULT_MESSAGE = "Unable to retrieve media properties.";

    public MediaPropertiesException(string message = DEFAULT_MESSAGE)
        : base(message)
    {
    }
    public MediaPropertiesException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class EmptySongTitleOrArtistException : LyricsGooglerException
{
    private const string DEFAULT_MESSAGE = "Song title or artist is empty.";

    public EmptySongTitleOrArtistException(string message = DEFAULT_MESSAGE)
        : base(message)
    {
    }
    public EmptySongTitleOrArtistException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
