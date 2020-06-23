using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace YtoMp3
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public ViewModel ViewModel { get; }

        public MainPage()
        {
            ViewModel = new ViewModel(this, DependencyService.Get<IFolderProvider>());
            BindingContext = ViewModel;
            InitializeComponent();
            picker.SelectedItem = picker.Items.First();
            ViewModel.SetChooser(choosingView);
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewModel.ChosenFolder = picker.SelectedItem.ToString();
        }
    }

    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

}
