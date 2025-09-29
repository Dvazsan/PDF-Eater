using System.Windows;

namespace PdfEater;

public class Error {
	public static void RaiseError(string error, string title) {
		MessageBoxResult result = MessageBox.Show(error, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
	}
}
