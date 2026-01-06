using System;
using System.Windows.Media;

namespace BinaryWPF.ViewModels.Tools
{
    public sealed class SwatcherViewModel : ViewModelBase
    {
        private int _red;
        private int _green;
        private int _blue;
        private string _paintSwatch = "0";
        private string _saturation = "0";
        private string _brightness = "0";

        public int Red
        {
            get => _red;
            set { if (SetField(ref _red, value)) Update(); }
        }

        public int Green
        {
            get => _green;
            set { if (SetField(ref _green, value)) Update(); }
        }

        public int Blue
        {
            get => _blue;
            set { if (SetField(ref _blue, value)) Update(); }
        }

        public string PaintSwatch
        {
            get => _paintSwatch;
            private set => SetField(ref _paintSwatch, value);
        }

        public string Saturation
        {
            get => _saturation;
            private set => SetField(ref _saturation, value);
        }

        public string Brightness
        {
            get => _brightness;
            private set => SetField(ref _brightness, value);
        }

        public SolidColorBrush PreviewBrush => new(System.Windows.Media.Color.FromRgb((byte)Red, (byte)Green, (byte)Blue));

        private void Update()
        {
            float red = Red / 255f;
            float green = Green / 255f;
            float blue = Blue / 255f;

            float hue = 0;
            float max = Math.Max(red, Math.Max(green, blue));
            float min = Math.Min(red, Math.Min(green, blue));
            float brt = max;
            float dif = max - min;
            float sat = max == 0 ? 0 : dif / max;

            if (max == min)
            {
                hue = 0;
            }
            else if (max == red)
            {
                hue = ((60 * ((green - blue) / dif)) + 360) % 360;
            }
            else if (max == green)
            {
                hue = ((60 * ((blue - red) / dif)) + 120) % 360;
            }
            else if (max == blue)
            {
                hue = ((60 * ((red - green) / dif)) + 240) % 360;
            }

            hue = 90 - (hue / 4);

            PaintSwatch = ((int)hue).ToString();
            Saturation = sat.ToString();
            Brightness = brt.ToString();
            OnPropertyChanged(nameof(PreviewBrush));
        }
    }
}
