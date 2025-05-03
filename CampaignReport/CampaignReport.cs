using Accessibility;
using EsfLibrary;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Windows.Forms.LinkLabel;

namespace CampaignReportNs
{
	public partial class CampaignReport : Form
	{
		public CampaignReport()
		{
			InitializeComponent();

			InitializeBackgroundWorker();

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
			StartReport();
		}

		private void ReportButton_Click(object sender, EventArgs e)
		{
			StartReport();
		}

		private void StartReport()
		{
			_series = GameListBox.SelectedItem as SeriesName;
			if (_series != null)
			{
				this.ProgressBar.Minimum = 0;
				this.ProgressBar.Maximum = _saveGamesLookup[_series.Faction].Count;

				this.Cursor = Cursors.WaitCursor;
				backgroundReport.RunWorkerAsync();
			}
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

		private void InitializeBackgroundWorker()
		{
			backgroundReport.DoWork += BackgroundReport_DoWork;
			backgroundReport.RunWorkerCompleted += BackgroundReport_RunWorkerCompleted;
			backgroundReport.WorkerReportsProgress = true;
			backgroundReport.ProgressChanged += BackgroundReport_ProgressChanged;
		}

		private void BackgroundReport_DoWork(object sender, DoWorkEventArgs e)
		{
			ReportSelectedSeries();
			e.Result = true;
		}

		private void BackgroundReport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.Cursor = Cursors.Default;

			string tsv = _report.GetTSV();
			if (tsv.Length > 0)  // because SetText throws NullReferenceException if string is "" (which isn't null)
			{
				StringBuilder msg = new StringBuilder();
				msg.AppendFormat("{0} campaign report, {1} turns processed", _series.Faction, _processed);
				if (_duplicates > 0)
					msg.AppendFormat(", {0} duplicate turns ignored", _duplicates);
				msg.AppendLine("\n");
				msg.Append("Pressing OK will copy TSV to clipboard.  Then Paste to Excel");

				MessageBox.Show(this, msg.ToString(), "Report complete");
				Clipboard.SetText(tsv);
			}
			else
			{
				MessageBox.Show(this, "No details", "No Report generated");
			}
		}

		private void BackgroundReport_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.ProgressBar.Value = (int)e.UserState;
		}

		private SeriesName _series;
		private Report _report;
		private int _processed;
		private int _duplicates;

		private void ReportSelectedSeries()
		{
			var files = _saveGamesLookup[_series.Faction];

			_report = new Report();
			_processed = 0;
			_duplicates = 0;
			for (int i = 0; i < files.Count; i++)
			{
				if (_report.AddSaveGame(files[i].File, ref _duplicates))
					_processed++;
				backgroundReport.ReportProgress(0, i+1);
			}
		}

	}
}
