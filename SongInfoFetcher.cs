using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace LyricsGoogler
{
    public static class SongInfoFetcher
    {
        public static async Task<(string artist, string title)> FetchArtistAndTitle()
        {
            var manager = await GetSystemMediaTransportControlsSessionManager();
            if (manager is null)
            {
                throw new MediaTransportControlsSessionException();
            }

            var session = manager.GetCurrentSession();
            if (session is null)
            {
                throw new MediaTransportControlsSessionException();
            }

            var mediaProperties = await GetMediaProperties(session);
            if (mediaProperties is null)
            {
                throw new MediaPropertiesException();
            }

            var artist = mediaProperties.Artist;
            var title = mediaProperties.Title;
            if (artist.All(char.IsWhiteSpace) || title.All(char.IsWhiteSpace))
            {
                throw new EmptySongTitleOrArtistException();
            }

            return (artist, title);
        }

        private static async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() =>
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

        private static async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) =>
            await session.TryGetMediaPropertiesAsync();
    }
}
