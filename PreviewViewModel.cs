using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace YtoMp3
{
    public class PreviewViewModel : IDisposable, INotifyPropertyChanged
    {
        private const int _forwardValue = 10;
        private readonly string _customName;
        private readonly string _folder;

        private string _state;
        public string State { get => _state; set => SetProperty(ref _state, value); }

        public event Action<bool> WorkChanged;

        private bool _working;
        public bool Working
        {
            get => _working;
            set
            {
                SetProperty(ref _working, value);
                WorkChanged?.Invoke(value);
                PlaySound.ChangeCanExecute();
                Download.ChangeCanExecute();
            }
        }

        public ObservableCollection<PreviewItem> Items { get; private set; } = new ObservableCollection<PreviewItem>();

        public static void ClearTemporaryFiles()
        {
            foreach (var f in System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            {
                if (System.IO.Path.GetFileName(f).StartsWith("_temp") && f.EndsWith("mp3"))
                {
                    System.IO.File.Delete(f);
                }
            }
                
        }

        public void Dispose()
        {
            Items.FirstOrDefault(i => i.IsPlaying)?.StopSample();
            foreach (var i in Items)
            {
                i.Dispose();
            }
            Items.Clear();
        }

        public Command<object> PlaySound { get; private set; }

        public Command<object> Download { get; private set; }

        public void InitializeCommands()
        {
            PlaySound = new Command<object>(async m =>
            {
                try
                {
                    Working = true;
                    State = "Setting up the music file...";
                    var focused = (PreviewItem)m;
                    var currentlyPlaying = Items.FirstOrDefault(i => i.IsPlaying);
                    if (currentlyPlaying == focused)
                    {
                        currentlyPlaying.Forward(_forwardValue);
                    }
                    else
                    {
                        if (currentlyPlaying != null)
                        {
                            currentlyPlaying.StopSample();
                        }
                        await focused.PlaySample();
                    }
                    State = "Awaiting user input...";
                    Working = false;
                } catch (Exception e)
                {
                    App.Debug("Critical error!\nError: " + e.Message + "\nStacktrace: " + e.StackTrace);
                }
        }, e => !Working);

            Download = new Command<object>(async m =>
            {
                try
                {
                    State = "Downloading chosen file...";
                    Working = true;
                    var focused = (PreviewItem)m;
                    if (focused.IsPlaying)
                    {
                        focused.StopSample();
                    }
                    await focused.Download(_folder, _customName);
                    await App.Current.MainPage.DisplayAlert("Download successful!", "The requested file is in the desired folder", "Ok");
                    Items.Remove(focused);
                    State = "Awaiting user input...";
                    Working = false;
                } catch (Exception e)
                {
                    App.Debug("Critical error!\nError: " + e.Message + "\nStacktrace: " + e.StackTrace);
                }
            }, e => !Working);
        }

        public void StopTrack() => Items.FirstOrDefault(w => w.IsPlaying)?.StopSample();

        public PreviewViewModel(string folder, string customName, List<PreviewItem> items)
        {
            State = "Awaiting user input...";
            InitializeCommands();
            _folder = folder;
            _customName = customName;
            items.ForEach(i => Items.Add(i));
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
