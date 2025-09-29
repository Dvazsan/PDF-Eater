using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PdfEater;

public partial class ColorpickerWindow : Window {
	private GradientStop SaturationColorStop = new GradientStop(Brushes.Red.Color, 1.0);
	private GradientStop ValueColorStop = new GradientStop(Brushes.Red.Color, 1.0);

	private struct HSVColor {
		public HSVColor(double H, double S, double V) {
			this.H = H;
			this.S = S;
			this.V = V;
		}

		public double H;
		public double S;
		public double V;
	}

	public ColorpickerWindow() {
		InitializeComponent();
		PrepareSliders();
		UpdateColors();
	}

	private void PrepareSliders() {
		HueSlider.Background = 
			new LinearGradientBrush(new GradientStopCollection(
				[
					new GradientStop((Color)ColorConverter.ConvertFromString("#FFFF0000"), 0.0), 
					new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFF00"), 0.17),
					new GradientStop((Color)ColorConverter.ConvertFromString("#FF00FF00"), 0.33),
					new GradientStop((Color)ColorConverter.ConvertFromString("#FF00FFFF"), 0.50),
					new GradientStop((Color)ColorConverter.ConvertFromString("#FF0000FF"), 0.67),
					new GradientStop((Color)ColorConverter.ConvertFromString("#FFFF00FF"), 0.83),
					new GradientStop((Color)ColorConverter.ConvertFromString("#FFFF0000"), 1.0)
				]
			), new Point(0, 0.5), new Point(1, 0.5));

		SaturationSlider.Background = 
			new LinearGradientBrush(new GradientStopCollection(
				[
					new GradientStop(Brushes.White.Color, 0.0),
					SaturationColorStop
				]
			), new Point(0, 0.5), new Point(1, 0.5));

		ValueSlider.Background = 
			new LinearGradientBrush(new GradientStopCollection(
				[
					new GradientStop(Brushes.Black.Color, 0.0),
					ValueColorStop
				]
			), new Point(0, 0.5), new Point(1, 0.5));

		HueSlider.ValueChanged += Slider_ValueChanged;
		SaturationSlider.ValueChanged += Slider_ValueChanged;
		ValueSlider.ValueChanged += Slider_ValueChanged;
	}

	private void BtnAccept_Click(object sender, RoutedEventArgs e) {
		Color newCol = HsvToRgb(HueSlider.Value, SaturationSlider.Value, ValueSlider.Value);
		Globals.colorOptions.Add(newCol);
		Close();
	}

	private void BtnDeny_Click(object sender, RoutedEventArgs e) {
		Close();
	}

	private void HexBox_KeyUp(object sender, KeyEventArgs e) {
		try {
			Color newCol = (Color)ColorConverter.ConvertFromString(HexBox.Text);
			HexBox.Text = newCol.ToString();
			HSVColor colHSV = ColorToHSV(newCol);
			HueSlider.Value = colHSV.H;
			SaturationSlider.Value = colHSV.S;
			ValueSlider.Value = colHSV.V;
			PreviewRect.Fill = new SolidColorBrush(newCol);
			SaturationColorStop.Color = HsvToRgb(colHSV.H, 1, colHSV.V);
			ValueColorStop.Color = HsvToRgb(colHSV.H, colHSV.S, 1);
			HexBox.ClearValue(TextBox.StyleProperty);
		}
		catch {
			HexBox.BorderBrush = Brushes.Red;
			HexBox.BorderThickness = new Thickness(4);
		}
	}

	private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
		UpdateColors();
	}

	private void UpdateColors() {
		double h = HueSlider.Value;
		double s = SaturationSlider.Value;
		double v = ValueSlider.Value;

		Color rgb = HsvToRgb(h, s, v);

		PreviewRect.Fill = new SolidColorBrush(rgb);
		SaturationColorStop.Color = HsvToRgb(h, 1, v);
		ValueColorStop.Color = HsvToRgb(h, s, 1);
		HexBox.Text = rgb.ToString();
	}

	private static Color HsvToRgb(double h, double s, double v) {
		double c = v * s;
		double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
		double m = v - c;

		double r = 0, g = 0, b = 0;

		if (h < 60) { r = c; g = x; b = 0; }
		else if (h < 120) { r = x; g = c; b = 0; }
		else if (h < 180) { r = 0; g = c; b = x; }
		else if (h < 240) { r = 0; g = x; b = c; }
		else if (h < 300) { r = x; g = 0; b = c; }
		else { r = c; g = 0; b = x; }

		return Color.FromRgb(
			(byte)((r + m) * 255),
			(byte)((g + m) * 255),
			(byte)((b + m) * 255));
	}

	private static HSVColor ColorToHSV(Color color) {
		double r = color.R / 255.0;
		double g = color.G / 255.0;
		double b = color.B / 255.0;

		double cmax = Math.Max(r, Math.Max(g, b));
		double cmin = Math.Min(r, Math.Min(g, b));
		double diff = cmax - cmin;

		double h = -1;
		double s = -1;
		double v = cmax;

		if (cmax == cmin) h = 0;

		if (cmax == r) h = (60 * ((g - b) / diff) + 360) % 360;
		if (cmax == g) h = (60 * ((b - r) / diff) + 120) % 360;
		if (cmax == b) h = (60 * ((r - g) / diff) + 240) % 360;

		if (cmax == 0) 
			s = 0;
		else 
			s = (diff / cmax);

		return new HSVColor(h, s, v);
	}
}

