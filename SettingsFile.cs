using System.IO;

namespace PdfEater;

public class SettingsFile {
	private class Setting {
		public Setting(string key, object val) {
			this.key = key;
			this.val = val;
		}

		public string key;
		public object val;
	}

	public string filePath = "";
	private Setting[] settings = [];

	public SettingsFile() {
		filePath = GetSettingsFilePath();
		if (!File.Exists(filePath)) {
			GenerateFile();
		}

		ParseSettings();
	}

	public object GetSetting(string settingKey, object defaultVal) {
		foreach (Setting setting in settings) {
			if (setting.key == settingKey)
				return setting.val;
		}
		
		return defaultVal;
	}

	public void UpdateSetting(string settingKey, object newVal) {
		for (int s = 0; s < settings.Count(); s++) {
			Setting setting = settings[s];
			if (setting.key == settingKey)
				setting.val = newVal;
		}
	}

	public void SaveSettings() {
		string newSettingsFileString = String.Join('\n', settings.Select(
				(setting) => {
					if (setting.val is string[])
						return setting.key + "=" + String.Join(',', (string[]) setting.val);
					else
						return setting.key + "=" + setting.val;
				}
			).ToArray());
		File.WriteAllText(filePath, newSettingsFileString);
	}

	private string GetSettingsFilePath() {
		string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string myAppFolder = Path.Combine(appData, "PdfEater", "settings.config");
		return myAppFolder;
	}

	private void GenerateFile() {
		string? settingsDir = Path.GetDirectoryName(filePath);
		if (settingsDir is not null) {
			Directory.CreateDirectory(settingsDir);
			File.WriteAllText(filePath, "COLORS=#000000,#ff0000,#00ff00");
		}
	}
	
	private void ParseSettings() {
		Setting[] settings = 
			File.ReadLines(filePath)
				.Where(line => line.Trim().Length > 0 && line.IndexOf('=') > 0)
				.Select(
					line => new Setting(line[0..line.IndexOf('=')], line[(line.IndexOf('=') + 1)..])
				).ToArray();

		for (int s = 0; s < settings.Count(); s++) {
			string valString = ((string) settings[s].val);
			if (valString.Contains(',')) {
				settings[s].val = valString.Split(',');
			}
		}

		this.settings = settings;
	}
}
