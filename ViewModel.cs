using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;

using YtoMp3.Tools;

using Xamarin.Forms;
using System.Globalization;

namespace YtoMp3
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly VideoToAudio _tool = new VideoToAudio();
        private readonly Page _page;
        private readonly IFolderProvider _folder;
        private Preview _choosingView;

        private bool choosin_;
        public bool Choosing { get => choosin_; set
            {
                SetProperty(ref choosin_, value);
            }
        }

        private bool working_;
        public bool Working { get => working_;
            set
            {
                SetProperty(ref working_, value);
                Execute.ChangeCanExecute();
            }
        }

        private string numberOfResults_ = "10";
        public string NumberOfResults { get => numberOfResults_; set => SetProperty(ref numberOfResults_, value); }
        private int ParsedNumOfResults
        {
            get
            {
                if (int.TryParse(NumberOfResults, out int toRet))
                {
                    if (toRet < 1)
                        toRet = 1;
                    return toRet;
                }
                else
                {
                    return 10;
                }
            }
        }

        private string chosenFolder_;
        public string ChosenFolder { get => chosenFolder_; set => SetProperty(ref chosenFolder_, value); }

        public IList<string> AvailableFolders => new List<string> { MusicFolderNaming, DownloadFolderNaming };

        private string SaveFolder =>
            ChosenFolder.Equals(MusicFolderNaming) ? _folder.MusicFolder : _folder.DownloadFolder;

        private string phrase_;
        public string Phrase { get => phrase_;
            set
            { 
                SetProperty(ref phrase_, value);
                Execute.ChangeCanExecute();
            }
        }

        private string state_;
        public string State { get => state_; set => SetProperty(ref state_, value); }

        private string customName_;
        public string CustomName { get => customName_; set => SetProperty(ref customName_, value); }

        string ExtractId(string url)
        {
            //https://www.youtube.com/watch?v=x2UyRS5cPJk&pbjreload=10
            int begin = url.IndexOf("watch?v=") + "watch?v=".Length;
            url = url.Substring(begin);
            return new string(url.TakeWhile(c => char.IsLetterOrDigit(c)).ToArray());
        }

        public Command Execute { get; }

        private async Task HandleDirectUrl()
        {
            State = "Downloading the file to your music directory...";
            await _tool.GetAudioFile(ExtractId(phrase_), SaveFolder, CustomName);
            State = "Done downloading, saving the file, You can download another!";
        }

        private async Task HandleSearch()
        {
            State = "Searching for videos...";
            var videos = await _tool.GetHeaders(Phrase, ParsedNumOfResults);
            if (videos == null || videos.Count == 0)
            {
                await _page.DisplayAlert("No results!", "Didn't get any result out of this query", "Ok");
                return;
            }
            _choosingView.Init(SaveFolder, string.IsNullOrEmpty(CustomName) ? null : CustomName, 
                videos.Where(v => v.Duration > TimeSpan.FromSeconds(1)).ToList(), () => Choosing = false);
            Choosing = true;
            _choosingView.IsVisible = true;
        }

        private bool AllowExecution()
        {
            if (Working) return false;
            if (IsUrlPassed)
            {
                return !string.IsNullOrEmpty(CustomName);
            }
            else
            {
                return !string.IsNullOrEmpty(Phrase);
            }
        }

        private bool IsUrlPassed =>
            Uri.TryCreate(phrase_, UriKind.Absolute, out Uri u) && phrase_.StartsWith("@https://youtube");

        private Command ExecuteCommand => new Command(async () =>
        {
            try
            {
                Working = true;
                if (IsUrlPassed)
                {
                    await HandleDirectUrl();
                }
                else
                {
                    await HandleSearch();
                }
            }
            catch (System.IO.IOException e)
            {
                await _page.DisplayAlert("Something went wrong!", e.Message, "Ok");
            }
            catch (System.Net.Http.HttpRequestException)
            {
                await _page.DisplayAlert("No internet connection", "You must have available internet connection!", "Ok");
            }
            catch (Exception e)
            {
                App.Debug("Critical error!\nError: " + e.Message + "\nStacktrace: " + e.StackTrace);
            }
            State = "Awaiting user input...";
            Working = false;
        }, AllowExecution);

        public void SetChooser(Preview v)
        {
            _choosingView = v;
        }

        public ViewModel(Page page, IFolderProvider folder)
        {
            Choosing = false;
            State = "Awaiting user input...";
            Execute = ExecuteCommand;
            _page = page;
            _folder = folder;
        }

        #region Const
        private const string DownloadFolderNaming = "Download folder";
        private const string MusicFolderNaming = "Music folder";
        #endregion
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
