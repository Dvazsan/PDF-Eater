using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Shapes;

namespace PdfEater;

public class LineCreator {
	const double wavelength = 10;
	const double amplitude = 2;
	const double step = 1; // sampling resolution (smaller = smoother)

	public static Stroke CreateWavyLine(Line line, DrawingAttributes drawingAttrs) {
		double dx = line.X2 - line.X1;
		double dy = line.Y2 - line.Y1;
		double length = Math.Sqrt(dx * dx + dy * dy);

		double angle = Math.Atan2(dy, dx);

		StylusPointCollection points = new StylusPointCollection();

		double ux = Math.Cos(angle);
		double uy = Math.Sin(angle);
		double px = -uy;
		double py = ux;

		for (double t = 0; t <= length; t += step) {
			double bx = line.X1 + ux * t;
			double by = line.Y1 + uy * t;

			double offset = Math.Sin((2 * Math.PI / wavelength) * t) * amplitude;
			double cx = bx + px * offset;
			double cy = by + py * offset;

			points.Add(new StylusPoint(cx, cy));
		}

		return new Stroke(points, drawingAttrs.Clone());
	}

	public static Stroke CreateLine(Line line, DrawingAttributes drawingAttrs) {
		StylusPointCollection points = new StylusPointCollection {
			new StylusPoint(line.X1, line.Y1),
			new StylusPoint(line.X2, line.Y2)
		};

		return new Stroke(points, drawingAttrs.Clone());
	}
}
