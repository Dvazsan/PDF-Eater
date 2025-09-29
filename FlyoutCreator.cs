using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PdfEater;

public class FlyoutCreator {
	static public void CreateColorFlyout(ref Popup parentPopup) {
		var panel = new StackPanel {
			Orientation = Orientation.Horizontal,
			Background = Brushes.White,
			Margin = new Thickness(4)
		};

		Popup parentPopupVal = parentPopup;

		foreach (Color col in Globals.colorOptions) {
			AddColorButton(col, ref panel);
		}

		AddCustomColorButton(ref panel);

		parentPopup = new Popup {
			Child = panel,
			Placement = PlacementMode.MousePoint,
			StaysOpen = false
		};
	}

	static void AddColorButton(Color c, ref StackPanel panel) {
		Grid btnGrid = new Grid();
		Button btn = new Button {
			Width = 40,
			Height = 40,
			Margin = new Thickness(1),
			Background = new SolidColorBrush(c),
			BorderBrush = Brushes.Gray,
			BorderThickness = new Thickness(1),
		};
		Button revokeBtn = 
			new Button {
				Width = 10,
				Height = 10,
				BorderThickness = new Thickness(0),
				Padding = new Thickness(-4),
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Right,
				FontWeight = FontWeights.Bold,
				Foreground = Brushes.White,
				Background = Brushes.Red,
				Margin = new Thickness(0, 2, 2, 0),
				Content = "x"
			};
		btnGrid.Children.Add(btn);
		btnGrid.Children.Add(revokeBtn);

		btn.Click += (object s, RoutedEventArgs e) => {
			Globals.selectedColor = (SolidColorBrush) btn.Background;
			((Popup) ((StackPanel) ((Grid) btn.Parent).Parent).Parent).IsOpen = false;
		};
		revokeBtn.Click += (object s, RoutedEventArgs e) => {
			Globals.colorOptions.Remove(((SolidColorBrush) btn.Background).Color);
			Popup parentPopup = ((Popup) ((StackPanel) ((Grid) btn.Parent).Parent).Parent);
			UpdateColorFlyout(ref parentPopup);
		};
		panel.Children.Add(btnGrid);
	}

	static private void AddCustomColorButton(ref StackPanel panel) {
		Button addBtn = new Button {
			Width = 40,
			Height = 40,
			Margin = new Thickness(1),
			Background = Brushes.Lime,
			BorderBrush = Brushes.Gray,
			BorderThickness = new Thickness(1),
			Content = new TextBlock {
				Text = "+",
				FontWeight = FontWeights.Bold,
				FontSize = 40,
				Foreground = Brushes.White,
				Margin = new Thickness(2.5, -10, 0, 0)
			},
		};

		addBtn.Click += (object s, RoutedEventArgs e) => {
			ColorpickerWindow colWin = new ColorpickerWindow();
			colWin.ShowDialog();
		};

		panel.Children.Add(addBtn);
	}

	static public void UpdateColorFlyout(ref Popup parentPopup) {
		StackPanel panel = (StackPanel) parentPopup.Child;
		panel.Children.Clear();
		foreach (Color col in Globals.colorOptions) {
			AddColorButton(col, ref panel);
		}
		AddCustomColorButton(ref panel);
	}

	static public void CreateThicknessFlyout(ref Popup parentPopup) {
		var panel = new StackPanel {
			Orientation = Orientation.Horizontal,
			Background = Brushes.White,
			Margin = new Thickness(4)
		};

		void AddButton(int dotThickness) {
			var btn = new Button {
				Width = 30,
				Height = 30,
				Margin = new Thickness(2),
				Background = Brushes.White,
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1),
				Content = new Ellipse {
					Width = dotThickness,
					Height = dotThickness,
					Fill = Brushes.Black,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
				}
			};
			btn.Click += (object s, RoutedEventArgs e) => {
				Globals.selectedThickness = (int) Math.Floor(((Ellipse) btn.Content).Width / 2);
				((Popup)((StackPanel) btn.Parent).Parent).IsOpen = false;
			};
			panel.Children.Add(btn);
		}

		AddButton(4);
		AddButton(6);
		AddButton(10);
		AddButton(16);

		parentPopup = new Popup {
			Child = panel,
			Placement = PlacementMode.MousePoint,
			StaysOpen = false
		};
	}
}

