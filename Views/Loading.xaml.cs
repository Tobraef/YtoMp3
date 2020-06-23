using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace YtoMp3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Loading : ContentView
    {
        private readonly int _xOffset = 200;
        private const int distanceBetween = 100;
        private bool _spins;

        private Task RightToLeft(BoxView box)
        {
            int v = 10;
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                int k = v--;
                if (k > 0)
                {
                    AbsoluteLayout.SetLayoutBounds(box, new Rectangle
                    {
                        X = _xOffset + k*10,
                        Y = 0,
                        Width = 40,
                        Height = 40
                    });
                    return true;
                }
                return false;
            });
            return Task.Delay(1000);
        }

        private Task LeftToMid(BoxView box)
        {
            int v = 10;
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                int k = v--;
                if (k > 0)
                {
                    AbsoluteLayout.SetLayoutBounds(box, new Rectangle
                    {
                        X = _xOffset + (distanceBetween/2) - k * 5,
                        Y = distanceBetween - k * 10,
                        Width = 40,
                        Height = 40
                    });
                    return true;
                }
                return false;
            });
            return Task.Delay(1000);
        }

        private Task MidToRight(BoxView box)
        {
            int v = 10;
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                int k = v--;
                if (k > 0)
                {
                    AbsoluteLayout.SetLayoutBounds(box, new Rectangle
                    {
                        X = _xOffset + distanceBetween - k * 5,
                        Y = k * 10,
                        Width = 40,
                        Height = 40
                    });
                    return true;
                }
                return false;
            });
            return Task.Delay(1000);
        }

        private async Task SpinOnce()
        {
            if (IsVisible && !_spins)
            {
                _spins = true;
                await Task.WhenAll(LeftToMid(first), RightToLeft(second), MidToRight(third));
                await Task.WhenAll(MidToRight(first), LeftToMid(second), RightToLeft(third));
                await Task.WhenAll(RightToLeft(first), MidToRight(second), LeftToMid(third));
                _spins = false;
                await SpinOnce();
            }
        }

        public Loading()
        {
            InitializeComponent();
            PropertyChanged += Loading_PropertyChanged;
        }

        private async void Loading_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await SpinOnce();
        }
    }
}