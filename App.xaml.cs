using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace YtoMp3
{
    public partial class App : Application
    {
        public static void Debug(params object[] toPrint)
        {
            Current.MainPage.DisplayAlert("Debug", string.Join(";", toPrint), "Ok");
        }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
