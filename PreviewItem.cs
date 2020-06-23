using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

using YoutubeExplode.Videos;
using Xamarin.Forms;

using YtoMp3.Tools;

namespace YtoMp3
{
    public class PreviewItem : INotifyPropertyChanged, IDisposable
    {
        private readonly Video _video;
        private readonly VideoToAudio _tools = new VideoToAudio();
        private readonly IAudioPlayer _player;
        private static int id = 0;

        private string _cachedFile;
        private static string NextTempFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "_temp" + (id++).ToString() + ".mp3");

        public string Title => _video.Title;
        public string Duration => "Duration: " + _video.Duration.ToString();
        public string Author => "Author: " + _video.Author;
        private string FullFileSaveName(string saveFolder, string customName)
            => Path.Combine(saveFolder, (customName ?? _video.Title) + ".mp3");

        private string _playButton;
        public string PlayButton { get => _playButton; set => SetProperty(ref _playButton, value); }

        public bool IsPlaying => PlayButton.Equals("forward");

        private bool CacheExists => File.Exists(_cachedFile) && _cachedFile != null;

        private async Task LoadTempFile()
        {
            _cachedFile = NextTempFile;
            await _tools.SaveAudioToFile(_video, _cachedFile);
        }

        public Task Download(string saveFolder, string customName = null)
        {
            if (CacheExists)
            {
                if (File.Exists(FullFileSaveName(saveFolder, customName)))
                {
                    File.Delete(FullFileSaveName(saveFolder, customName));
                }
                return Task.Run(() => File.Move(_cachedFile, FullFileSaveName(saveFolder, customName)));
            }
            return _tools.GetAudioFile(_video, saveFolder, customName);
        }

        private void HandlePlaybackFinished() => PlayButton = "play";

        public void StopSample()
        {
            _player.PlaybackFinished -= HandlePlaybackFinished;
            PlayButton = "play";
            _player.Stop();
        }

        public void Forward(int secs)
        {
            _player.Forward(secs);
        }

        public async Task PlaySample()
        {
            try
            {
                PlayButton = "forward";
                if (!CacheExists)
                {
                    await LoadTempFile();
                }
                _player.Play(_cachedFile);
                _player.PlaybackFinished += HandlePlaybackFinished;
            } catch (Exception e)
            {
                App.Debug("Critical error!\nError: " + e.Message + "\nStacktrace: " + e.StackTrace);
            }
        }

        public void Dispose()
        {
            if (CacheExists)
            {
                File.Delete(_cachedFile);
            }
        }

        internal PreviewItem(Video video, IAudioPlayer player)
        {
            _video = video;
            _player = player;
            PlayButton = "play";
        }

        #region Binding
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T toUpdate, T val, [CallerMemberName] string name = "")
        {
            toUpdate = val;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
