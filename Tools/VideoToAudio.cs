using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YtoMp3.Tools
{
    public class VideoToAudio
    {
        readonly YoutubeClient _yt = new YoutubeClient();

        public async Task<IReadOnlyList<Video>> GetHeaders(string query, int max)
        {
            List<Video> toRet = new List<Video>();
            var videos = _yt.Search.GetVideosAsync(query);
            var iter = videos.GetAsyncEnumerator();
            for (int i = 0; i < max && await iter.MoveNextAsync(); ++i)
            {
                toRet.Add(iter.Current);
            }
            return toRet;
        }

        public async Task<Stream> GetPlayableStream(Video video)
        {
            var streamManifest = await _yt.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = GetMp3Stream(streamManifest);
            if (streamInfo != null)
            {
                return await _yt.Videos.Streams.GetAsync(streamInfo);
            }
            else
            {
                throw new IOException("Couldn't load any stream from the video. Possible extensions were: " +
                    string.Join(", ", streamManifest.GetAudioOnly().Select(w => w.Container.Name)));
            }
        }

        public async Task SaveAudioToFile(Video video, string fullPath)
        {
            var streamManifest = await _yt.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = GetMp3Stream(streamManifest);
            if (streamInfo != null)
            {
                await _yt.Videos.Streams.DownloadAsync(streamInfo, fullPath);
            }
            else
            {
                throw new IOException("Couldn't load any stream from the video. Possible extensions were: " +
                    string.Join(", ", streamManifest.GetAudioOnly().Select(w => w.Container.Name)));
            }
        }

        private IStreamInfo GetMp3Stream(StreamManifest streamManifest)
        {
            return streamManifest.GetAudioOnly().FirstOrDefault(w => w.Container.Name.Equals("mp3")) ??
                streamManifest.GetAudioOnly().FirstOrDefault(w => w.Container.Name.Equals("mp4")) ??
                streamManifest.GetAudioOnly().FirstOrDefault(w => !w.Container.Name.Equals("webm"));
        }

        public async Task GetAudioFile(Video video, string saveFolder, string name = "")
        {
            if (string.IsNullOrEmpty(name)) name = null;
            var streamManifest = await _yt.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = GetMp3Stream(streamManifest);
            if (streamInfo != null)
            {
                string saveName;
                if (name != null)
                {
                    saveName = name + ".mp3";
                }
                else
                {
                    saveName = video.Title + ".mp3";
                }
                saveName = saveName.Replace(Path.DirectorySeparatorChar, ' ');
                await _yt.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(saveFolder, saveName));
            }
            else
            {
                throw new IOException("Couldn't load any stream from the video. Possible extensions were: " +
                    string.Join(", ", streamManifest.GetAudioOnly().Select(w => w.Container.Name)));
            }
        }

        public async Task GetAudioFile(string id, string saveFolder, string name)
        {
            var streamManifest = await _yt.Videos.Streams.GetManifestAsync(id);
            var streamInfo = GetMp3Stream(streamManifest);
            if (streamInfo != null)
            {
                await _yt.Videos.Streams.DownloadAsync(streamInfo, Path.Combine(saveFolder, name + ".mp3"));
            }
            else
            {
                throw new IOException("Couldn't load any stream from the video. Possible extensions were: " +
                    string.Join(", ", streamManifest.GetAudioOnly().Select(w => w.Container.Name)));
            }
        }
    }
}
