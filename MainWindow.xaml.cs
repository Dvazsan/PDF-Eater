using System.Windows;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Microsoft.Win32;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

using ControlsImage = System.Windows.Controls.Image;
using DrawingImage = System.Drawing.Image;
using SharpDoc = PdfSharp.Pdf.PdfDocument;
using PdfiumDoc = PdfiumViewer.PdfDocument;
using Brushes = System.Windows.Media.Brushes;
using ColorConverter = System.Windows.Media.ColorConverter;
using Color = System.Windows.Media.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace PdfEater;

public partial class MainWindow : Window {
	private PdfiumDoc? pdfDoc;
	private SharpDoc? sharpPdfDoc;
	private readonly Dictionary<int, Grid> pageContainers = new();
	private readonly HashSet<int> renderedPages = new();
	private string currentFile = "";
	private Popup colorPopup = new Popup();
	private Popup thicknessPopup = new Popup();

	public MainWindow() {
		InitializeComponent();
		LoadSettings();
		PagesPanelScrollViewer.CanContentScroll = false;
		FlyoutCreator.CreateColorFlyout(ref colorPopup);
		FlyoutCreator.CreateThicknessFlyout(ref thicknessPopup);
		Closing += OnClose;
	}

	private void OnClose(object? sender, CancelEventArgs e) {
		SaveSettings();
		if (Globals.lastWrite > Globals.lastSave) {
			MessageBoxResult result = MessageBox.Show("Would you like to save your work?", "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
			if (result == MessageBoxResult.Yes) {
				SavePDF();
			}
			else if (result == MessageBoxResult.Cancel) {
				e.Cancel = true;
			}
		}
	}

	private void PagesPanel_OnMouseWheel(object sender, MouseWheelEventArgs e) {
		if (Keyboard.Modifiers == ModifierKeys.Control) {
			double zoom = e.Delta > 0 ? 0.1 : -0.1;
			double newScale = Math.Max(0.1, PagesPanelZoomTransform.ScaleX + zoom);
			PagesPanelZoomTransform.ScaleX = newScale;
			PagesPanelZoomTransform.ScaleY = newScale;
			e.Handled = true;
		}
	}

	private void LoadSettings() {
		string[] defaultColors = [ "#ff0000", "#000000", "#00ff00" ];
		string[] loadedColors = (string[]) Globals.settings.GetSetting("COLORS", defaultColors);
		Globals.colorOptions.Clear();
		foreach (string loadedColor in loadedColors) {
			Globals.colorOptions.Add((Color) ColorConverter.ConvertFromString(loadedColor));
		}
	}

	private void SaveSettings() {
		string[] newColorSettings = Globals.colorOptions
			.Select((color) => color.ToString()).ToArray();
		Globals.settings.UpdateSetting("COLORS", newColorSettings);
		Globals.settings.SaveSettings();
	}

	private void LoadPdf(string path) {
		if (!File.Exists(path)) {
			Error.RaiseError($"Error - failed to load PDF ({path}), file doesn't exist.", "Load error");
			return;
		}

		pdfDoc = PdfiumDoc.Load(path);
		sharpPdfDoc = PdfReader.Open(currentFile, PdfDocumentOpenMode.Modify);

		for (int i = 0; i < pdfDoc.PageCount; i++) {
			Grid? pageGrid = null;
			double progBarIncrement = 100.0 / pdfDoc.PageCount;

			Dispatcher.Invoke(() => {
				LoadingMessageProgressBar.Value = i * progBarIncrement;
				pageGrid = new Grid {
					Margin = new Thickness(0, 10, 0, 10)
				};
				ControlsImage img = new ControlsImage {
					Stretch = Stretch.None,
				};
				DrawingCanvas canvas = new DrawingCanvas();
				pageGrid.Children.Add(img);
				pageGrid.Children.Add(canvas);
				PagesPanel.Children.Add(pageGrid);
			});

			if (pageGrid is not null)
				pageContainers[i] = pageGrid;
			RenderPage(i);
		}
	}

	private void BtnColor_Click(object sender, RoutedEventArgs e) {
		colorPopup.IsOpen = true;
		FlyoutCreator.UpdateColorFlyout(ref colorPopup);
	}

	private void BtnThickness_Click(object sender, RoutedEventArgs e) {
		thicknessPopup.IsOpen = true;
	}

	private void BtnRule_Click(object sender, RoutedEventArgs e) {
		Globals.drawingMode = Globals.DrawingType.Ruling;
	}

	private void BtnFreehand_Click(object sender, RoutedEventArgs e) {
		Globals.drawingMode = Globals.DrawingType.Freehand;
	}

	private void BtnRuleWavy_Click(object sender, RoutedEventArgs e) {
		Globals.drawingMode = Globals.DrawingType.Wavy;
	}
	
	private void BtnOpenfile_Click(object sender, RoutedEventArgs e) {
		OpenFileDialog dlg = new OpenFileDialog {
			Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
		};

		if (dlg.ShowDialog() == true)
		{
			ClearPDF();
			currentFile = dlg.FileName;

			LoadingMessage.Visibility = Visibility.Visible;

			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += 
				(object? sender, DoWorkEventArgs e) => 
				 	LoadPdf(currentFile);
			worker.RunWorkerAsync();
			worker.RunWorkerCompleted += 
				((s, eArgs) => {
					LoadingMessage.Visibility = Visibility.Collapsed;
				});
		}
	}

	private void BtnPageNumMinus_Click(object sender, RoutedEventArgs e) {
		int newPage = 1;
		Int32.TryParse(TextBoxPageNum.Text, out newPage);
		if (newPage > 1)
			ScrollToSelectedPage(--newPage);
		TextBoxPageNum.Text = newPage.ToString();
	}

	private void BtnPageNumPlus_Click(object sender, RoutedEventArgs e) {
		int newPage = 1;
		Int32.TryParse(TextBoxPageNum.Text, out newPage);
		if (newPage + 1 < pageContainers.Count)
			ScrollToSelectedPage(++newPage);
		TextBoxPageNum.Text = newPage.ToString();
	}

	private void TextBoxPageNum_KeyUp(object sender, KeyEventArgs e) {
		int newPage = 1;
		Int32.TryParse(TextBoxPageNum.Text, out newPage);
		ScrollToSelectedPage(newPage);
		TextBoxPageNum.Text = newPage.ToString();
	}

	private void PagesPanelScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
		int? currentGridId = null;
		foreach (KeyValuePair<int, Grid> containerPair in pageContainers) {
			Grid pageGrid = containerPair.Value;
			if (!pageGrid.IsVisible)
				continue;

			Rect bounds = pageGrid.TransformToAncestor(this).TransformBounds(new Rect(0, 0, pageGrid.ActualWidth, pageGrid.ActualHeight));
			Rect rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);

			if (rect.Contains(bounds.TopLeft)) {
				currentGridId = containerPair.Key;
				break;
			}
		}

		if (currentGridId != null)
			TextBoxPageNum.Text = (currentGridId + 1).ToString();
	}

	private void ScrollToSelectedPage(int selectedPage) {
		bool Success = true;

		try {
			if (selectedPage > 0 && selectedPage <= pageContainers.Count) {
				Grid targetPageGrid = pageContainers[selectedPage - 1];
				Rect bounds = targetPageGrid.TransformToAncestor(PagesPanel).TransformBounds(new Rect(0, 0, targetPageGrid.ActualWidth, targetPageGrid.ActualHeight));
				PagesPanelScrollViewer.ScrollToVerticalOffset(bounds.Top * PagesPanelZoomTransform.ScaleX);
			}
			else 
				Success = false;
		}
		catch {
			Success = false;
		}

		if (!Success)
			TextBoxPageNum.BorderBrush = Brushes.Red;
		else
			TextBoxPageNum.ClearValue(TextBox.StyleProperty);
	}

	private void ClearPDF() {
		PagesPanel.Children.Clear();
		pageContainers.Clear();
		renderedPages.Clear();
	}

	private void SavePDF() {
		if (sharpPdfDoc is null){ 
			Error.RaiseError($"Error - failed to save PDF ({currentFile}), no PDF is currently loaded.", "Save error");
			return;
		}

		SaveFileDialog saveDialog = new SaveFileDialog {
			Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
		};

		if (saveDialog.ShowDialog() == true) {
			String out_path = saveDialog.FileName;

			void SavePDFPage(int index) {
				PdfPage? SharpPage = null;
				MemoryStream? drawingCanvasBmp = null;

				this.Dispatcher.Invoke((Action)(() => {
					drawingCanvasBmp = ((DrawingCanvas) pageContainers[index].Children[1]).RenderToBitmap();
					SharpPage = sharpPdfDoc.Pages[index];
				}));

				if (drawingCanvasBmp is not null && SharpPage is not null) {
					using (XGraphics PageGraphics = XGraphics.FromPdfPage(SharpPage)) {
						using (XImage page_image = XImage.FromStream(drawingCanvasBmp)) {
							PageGraphics.DrawImage(page_image, SharpPage.MediaBox.X1, -1 * SharpPage.MediaBox.Y1, SharpPage.MediaBox.Width, SharpPage.MediaBox.Height);
						}
					}
				}
			}

			LoadingWindow saveProgress = new LoadingWindow(SavePDFPage, sharpPdfDoc.PageCount);
			saveProgress.ShowDialog();

			sharpPdfDoc.Save(out_path);
			Globals.lastSave = DateTime.Now;
		}
		else {
			Error.RaiseError("Error - unable to save your current file.", "Save error");
		}
	}

	private void BtnSave_Click(object sender, RoutedEventArgs e) {
		SavePDF();
	}

	private void RenderPage(int pageNumber) {
		if (pdfDoc == null || sharpPdfDoc == null || renderedPages.Contains(pageNumber)) {
			Error.RaiseError($"Error - failed to render page {pageNumber}.", "Render error");
			return;
		}

		PdfPage sharpPage = sharpPdfDoc.Pages[pageNumber];
		int pageWidth = (int) Math.Floor(sharpPage.Width.Point) * 2;
		int pageHeight = (int) Math.Floor(sharpPage.Height.Point) * 2;
		using (DrawingImage img = pdfDoc.Render(pageNumber, pageWidth, pageHeight, 96, 96, false)) {
			BitmapImage bmp = BitmapToImageSource(new Bitmap(img));

			Dispatcher.Invoke(() => {
				Grid grid = pageContainers[pageNumber];
				ControlsImage imageControl = (ControlsImage)grid.Children[0];
				DrawingCanvas canvas = (DrawingCanvas) grid.Children[1];
				imageControl.Source = bmp;

				canvas.Width = bmp.Width;
				canvas.Height = bmp.Height;
			});

			renderedPages.Add(pageNumber);
		}
	}

	private BitmapImage BitmapToImageSource(Bitmap bitmap) {
		using (MemoryStream memory = new MemoryStream()) {
			bitmap.Save(memory, ImageFormat.Png);
			memory.Position = 0;

			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memory;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.EndInit();
			bitmapImage.Freeze();
			return bitmapImage;
		}
	}
}

