using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using Plugin.SimpleAudioPlayer;

namespace YtoMp3.Tools
{
    interface IAudioPlayer
    {
        event Action PlaybackFinished;
        void Play(string file);
        void Stop();
        void Forward(double secs);
    }

    class AudioPlayer : IAudioPlayer
    {
        private ISimpleAudioPlayer _impl;
        private Stream _currentStream;

        public event Action PlaybackFinished;

        ~AudioPlayer()
        {
            _currentStream?.Close();
        }

        public void Forward(double secs)
        {
            var toSeek = _impl.CurrentPosition + secs;
            _impl.Seek(toSeek);
        }

        public void Play(string file)
        {
            if (_impl.IsPlaying)
            {
                _impl.Stop();
                _currentStream?.Close();
                _currentStream = null;
            }
            _currentStream = File.OpenRead(file);
            _impl.Load(_currentStream);
            _impl.Play();
        }

        public void Stop()
        {
            _currentStream?.Close();
            _currentStream = null;
            _impl.Stop();
        }

        public AudioPlayer()
        {
            _impl = CrossSimpleAudioPlayer.Current;
            _impl.Loop = false;
            _impl.PlaybackEnded += (o, e) => PlaybackFinished?.Invoke();
        }
    }
}
