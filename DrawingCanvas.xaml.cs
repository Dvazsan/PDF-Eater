using System.Windows.Controls;
using System.Xml;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace PdfEater;

public partial class DrawingCanvas : Grid {
	private Line? line;
	private DrawingAttributes currentDrawingAttrs =
		new DrawingAttributes {
			Color = Globals.selectedColor.Color,
			Width = Globals.selectedThickness,
			Height = Globals.selectedThickness,
			FitToCurve = true
		};

	public int penSize {
		get => (int) currentDrawingAttrs.Width;
		set {
			currentDrawingAttrs.Width = value;
			currentDrawingAttrs.Height = value; 
		}
	}

	public Color penColor {
		get => currentDrawingAttrs.Color;
		set => currentDrawingAttrs.Color = value;
	}

	private void prepareDrawingMode(bool erasing = false) {
		Globals.lastWrite = DateTime.Now;
		penColor = Globals.selectedColor.Color;
		penSize = Globals.selectedThickness;

		if (Globals.IsRuling()) {
			if (!erasing)
				inkCanvas.EditingMode = InkCanvasEditingMode.None;
		}
	}

	public DrawingCanvas() {
		InitializeComponent();
		inkCanvas.DefaultDrawingAttributes = currentDrawingAttrs;
	}

	private void InkCanvas_TouchAction(object sender, TouchEventArgs e) {
		e.Handled = true;
		inkCanvas.EditingMode = InkCanvasEditingMode.None;
	}

	private void InkCanvas_StylusDown(object sender, StylusDownEventArgs e) {
		prepareDrawingMode(e.StylusDevice.Inverted);
		if (e.StylusDevice.TabletDevice.Type == TabletDeviceType.Touch)
			return;
		if (Globals.IsRuling()) {
			if (!isTouchEvent(e.StylusDevice))
				StartLine(e.GetPosition(this));
		} 
		else
			inkCanvas.EditingMode = e.StylusDevice.Inverted
				? InkCanvasEditingMode.EraseByPoint
				: InkCanvasEditingMode.Ink;
	}

	private void InkCanvas_StylusOutOfRange(object sender, StylusEventArgs e) {
		prepareDrawingMode();
		if (Globals.IsRuling()) 
			FinishLine();
		else
			inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
	}

	private void InkCanvas_StylusMove(object sender, StylusEventArgs e) {
		prepareDrawingMode();
		if (Globals.IsRuling()) 
			if (!isTouchEvent(e.StylusDevice))
				ContinueLine(e.GetPosition(this));
		else {
			if (e.StylusDevice.Inverted)
				inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
		}
	}

	private void InkCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
		prepareDrawingMode();
		if (Globals.IsRuling()) {
			if (!isTouchEvent(e.StylusDevice))
				StartLine(e.GetPosition(this));
		}
		else
			inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
	}

	private void InkCanvas_MouseMove(object sender, MouseEventArgs e) {
		prepareDrawingMode();
		if (Globals.IsRuling() && !isTouchEvent(e.StylusDevice))
			ContinueLine(e.GetPosition(this));
	}

	private void InkCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
		prepareDrawingMode();
		if (Globals.IsRuling() && !isTouchEvent(e.StylusDevice)) 
			FinishLine();
	}

	private bool isTouchEvent(StylusDevice? device) {
		return (device is not null && device.TabletDevice.Type == TabletDeviceType.Touch);
	}

	private void StartLine(Point position) {
		if (line != null) return;

		inkCanvas.EditingMode = InkCanvasEditingMode.None;

		line = new Line();
		line.X1 = position.X;
		line.Y1 = position.Y;
		line.X2 = position.X;
		line.Y2 = position.Y;

		line.Stroke = new SolidColorBrush(this.penColor);
		line.StrokeThickness = this.penSize;
		overlayCanvas.Children.Add(line);
	}

	private void ContinueLine(Point position) {
		if (line == null)
			return;

		inkCanvas.EditingMode = InkCanvasEditingMode.None;

		line.X2 = position.X;
		line.Y2 = position.Y;
	}

	private void FinishLine() {
		if (line == null)
			return;

		inkCanvas.Strokes.Add(
			Globals.drawingMode == Globals.DrawingType.Wavy 
				? LineCreator.CreateWavyLine(line, currentDrawingAttrs)
				: LineCreator.CreateLine(line, currentDrawingAttrs)
		);
		overlayCanvas.Children.Remove(line);
		line = null;
	}

	public MemoryStream RenderToBitmap() {
		//DrawingCanvas selfCopy = (DrawingCanvas) XamlReader.Load(XmlReader.Create(new StringReader(XamlWriter.Save(this))));
		//((StackPanel) this.Parent).Children.Add(selfCopy);
		/*this.Arrange(new Rect(new Size(this.ActualWidth, this.ActualHeight)));
		this.UpdateLayout();

		RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
			(int) this.ActualWidth,
			(int) this.ActualHeight,
			96,                  
			96,                  
			PixelFormats.Pbgra32
		);

		renderBitmap.Render(this);*/

		Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
		double dpi = 96.0;

		RenderTargetBitmap rtb = new RenderTargetBitmap(
			(int)bounds.Width,
			(int)bounds.Height,
			dpi, dpi,
			PixelFormats.Pbgra32
		);

		DrawingVisual dv = new DrawingVisual();
		using (var ctx = dv.RenderOpen()) {
			VisualBrush brush = new VisualBrush(this);
			ctx.DrawRectangle(brush, null, new Rect(new Point(), bounds.Size));
		}
		rtb.Render(dv);

		//((StackPanel) this.Parent).Children.Remove(selfCopy);
		MemoryStream stream = new MemoryStream();
		PngBitmapEncoder encoder = new PngBitmapEncoder();
		encoder.Frames.Add(BitmapFrame.Create(rtb));
		encoder.Save(stream);
		stream.Position = 0;

		return stream;
	}
}

