using Accessibility;
using EsfLibrary;
using System.Runtime.CompilerServices;
using System.Text;

namespace CampaignReportNs
{
	public partial class CampaignReport : Form
	{
		public CampaignReport()
		{
			InitializeComponent();

			SetDefaultSavegameFolder();
		}

		private void DefaultFolderButton_Click(object sender, EventArgs e)
		{
			SetDefaultSavegameFolder();
		}

		private void BrowseFolderButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.InitialDirectory = _saveGamePath;

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				_saveGamePath = dlg.SelectedPath;
				SetPathDisplayName();
				FillSaveGames();
			}
		}

		private void GameListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.ReportButton.Enabled = this.GameListBox.SelectedIndex >= 0;
			this.ProgressBar.Value = 0;
		}

		private void GameListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ReportSelectedSeries();
		}

		private void ReportButton_Click(object sender, EventArgs e)
		{
			ReportSelectedSeries();
		}

		private string _saveGamePath;
		private string _saveGameHeader;
		private void SetDefaultSavegameFolder()
		{
			const string game = "Attila";
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_saveGameHeader = $@"{appDataPath}\The Creative Assembly\{game}";
			_saveGamePath = _saveGameHeader + $@"\save_games";
			SetPathDisplayName();
			FillSaveGames();
		}

		private void SetPathDisplayName()
		{
			if ((_saveGameHeader.Length > 0) &&
				(_saveGamePath.Substring(0, _saveGameHeader.Length) == _saveGameHeader))
				this.FolderTextBox.Text = _saveGamePath.Substring(_saveGameHeader.Length);
			else
				this.FolderTextBox.Text = _saveGamePath;
		}

		private class SeriesName
		{
			public string Faction;
			public int FileCount;
			public override string ToString()
			{
				return string.Format("{0} ({1} save-{2})", Faction, FileCount,
					FileCount == 1 ? "game" : "games");
			}
		}
		private class SeriesTemplate
		{
			public string Faction;
			public int Year;
			public FileInfo File;
		}
		private Dictionary<string, List<SeriesTemplate>> _saveGamesLookup;

		//	class SeriesTurn
		//	class SeriesTurnFaction

		private void FillSaveGames()
		{
			this.GameListBox.Items.Clear();
			_saveGamesLookup = new Dictionary<string, List<SeriesTemplate>>();

			DirectoryInfo saveDir = new DirectoryInfo(_saveGamePath);
			var files = saveDir.GetFiles("*.save");

			foreach (var file in files)
			{
				SeriesTemplate template = TemplateFromFilename(file.Name);

				if (template != null)
				{
					template.File = file;
					List<SeriesTemplate> list;
					if (!_saveGamesLookup.TryGetValue(template.Faction, out list))
					{
						list = new List<SeriesTemplate>();
						_saveGamesLookup.Add(template.Faction, list);
					}
					list.Add(template);
				}
			}

			this.GameListBox.Items.Clear();
			var sorted = from template in _saveGamesLookup
						 //where template.Value.Count > 1
						 orderby template.Value.Count descending
						 select template;
			foreach (var template in sorted)
			{
				this.GameListBox.Items.Add(new SeriesName()
				{
					Faction = template.Key,
					FileCount = template.Value.Count
				});
			}

			this.ReportButton.Enabled = false;
			this.ProgressBar.Value = 0;
		}

		private SeriesTemplate TemplateFromFilename(string fileName)
		{
			var words = fileName.Split(' ');
			int year;
			foreach (var word in words)
			{
				if (int.TryParse(word, out year))
				{
					string faction = fileName.Substring(0, fileName.IndexOf(word)).Trim();
					return new SeriesTemplate() { Faction = faction, Year = year };
				}
			}
			return null;
		}

		private void ReportSelectedSeries()
		{
			var series = GameListBox.SelectedItem as SeriesName;
			if (series != null)
			{
				var files = _saveGamesLookup[series.Faction];
				this.ProgressBar.Minimum = 0;
				this.ProgressBar.Maximum = files.Count;

				Report report = new Report();
				int processed = 0;
				int duplicates = 0;
				for (int i = 0; i < files.Count; i++)
				{
					this.ProgressBar.Value = i;

					if (report.AddSaveGame(files[i].File, ref duplicates))
						processed++;
				}
				this.ProgressBar.Value = files.Count;

				string tsv = report.GetTSV();
				if (tsv.Length > 0)  // because SetText throws NullReferenceException if string is "" (which isn't null)
				{
					Clipboard.SetText(tsv);

					StringBuilder msg = new StringBuilder();
					msg.AppendFormat("{0} campaign report, {1} turns processed", series.Faction, processed);
					if (duplicates > 0)
						msg.AppendFormat(", {0} duplicate turns ignored", duplicates);
					msg.AppendLine("\n");
					msg.Append("TSV copied to clipboard.  Now Paste to Excel");

					MessageBox.Show(msg.ToString(), "Report complete");
				}
				else
				{
					MessageBox.Show("No details", "No Report generated");
				}
			}
		}

	}
}
