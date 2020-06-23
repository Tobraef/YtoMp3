using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using YoutubeExplode.Videos;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;

namespace YtoMp3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Preview : ContentView
    {
        private Tools.IAudioPlayer _player;
        private Action _popper;

        public PreviewViewModel ViewModel { get; private set; }

        void SwitchButton(bool state)
        {
            backButton.IsEnabled = !state;
        }

        public void Init(string folder, string customName, IReadOnlyList<Video> items, Action popper)
        {
            try { 
                _popper = popper;
                _player = new Tools.AudioPlayer();
                var previews = items.Select(v => new PreviewItem(v, _player)).ToList();
                ViewModel = new PreviewViewModel(folder, customName, previews);
                BindingContext = ViewModel;
                ViewModel.WorkChanged += SwitchButton;
                listView.ItemsSource = ViewModel.Items;
                OnBindingContextChanged();
                backButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                App.Debug("Critical error!\nError: " + e.Message + "\nStacktrace: " + e.StackTrace);
            }
        }

        public Preview()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try { 
            IsVisible = false;
            ViewModel.Dispose();
            listView.ItemsSource = null;
            ViewModel.WorkChanged -= SwitchButton;
            ViewModel = null;
            _popper();
            }
            catch (Exception x)
            {
                App.Debug("Critical error!\nError: " + x.Message + "\nStacktrace: " + x.StackTrace);
            }
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            ViewModel.StopTrack();
        }
    }
}