using System.Windows.Media;
using PdfEater;

public static class Globals {
	public enum DrawingType {
		Freehand,
		Ruling,
		Wavy
	}

	public static DrawingType drawingMode = DrawingType.Freehand;
	public static bool IsRuling() => drawingMode == DrawingType.Ruling || drawingMode == DrawingType.Wavy;

	public static int selectedThickness = 2;

	public static DateTime lastSave = new DateTime(2000, 1, 1);
	public static DateTime lastWrite = new DateTime(1999, 1, 1);
	public static SettingsFile settings = new SettingsFile();

	public static List<Color> colorOptions = new List<Color>([ Colors.Black, Colors.Blue, Colors.Red, Colors.Green ]);
	public static SolidColorBrush selectedColor = new SolidColorBrush(Brushes.Black.Color);
	public static bool sketchSnapping = false;
}

