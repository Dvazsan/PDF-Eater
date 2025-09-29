using System.Windows;
using System.ComponentModel;

namespace PdfEater;

public partial class LoadingWindow : Window {
	private int taskCount;
	private Action<int> taskFunction;

	public LoadingWindow(Action<int> taskFunction, int taskCount) {
		this.taskFunction = taskFunction;
		this.taskCount = taskCount;
		InitializeComponent();
	}

	private void Window_ContentRendered(object sender, EventArgs e) {
		BackgroundWorker worker = new BackgroundWorker();
		worker.WorkerReportsProgress = true;
		worker.DoWork += worker_DoWork;
		worker.ProgressChanged += worker_ProgressChanged;
		worker.RunWorkerAsync();
		worker.RunWorkerCompleted += 
			(s, eArgs) => this.Close();
	}

	void worker_DoWork(object? sender, DoWorkEventArgs e) {
		for(int i = 0; i < taskCount; i++) {
			if (sender is not null) {
				BackgroundWorker? senderWorker = sender as BackgroundWorker;
				if (senderWorker is not null)
					senderWorker.ReportProgress(i);
			}
			taskFunction(i);
		}
	}

	void worker_ProgressChanged(object? sender, ProgressChangedEventArgs e) {
		progressBar.Value = e.ProgressPercentage;
	}
}
