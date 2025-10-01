using Accessibility;
using EsfLibrary;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
				string gifFileName = _series.Faction + ".gif";
				string gifFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
					gifFileName);

				StringBuilder msg = new StringBuilder();
				msg.AppendFormat("{0} campaign report, {1} turns processed", _series.Faction, _processed);
				if (_duplicates > 0)
					msg.AppendFormat(", {0} duplicate turns ignored", _duplicates);
				msg.AppendLine("\n");
				msg.Append("Pressing OK will copy TSV to clipboard.  Then Paste to Excel");
				if (_exportMapGif)
				{
					msg.AppendFormat("\nAnd animated GIF will be written to Pictures\\{0}", gifFileName);
				}

				MessageBox.Show(this, msg.ToString(), "Report complete");
				Clipboard.SetText(tsv);

				if (_exportMapGif)
				{
					GifBitmapEncoder gifEnc = new GifBitmapEncoder();
					_report.GetMapAnim(gifEnc);
					saveAnimatedGif(gifFilePath, gifEnc);
				}
			}
			else
			{
				MessageBox.Show(this, "No details", "No Report generated");
			}
		}

		private void saveAnimatedGif(string gifFilePath, GifBitmapEncoder gifEnc)
		{
			// This creates a gif that animates only once
			// The .net GifBitmapEncoder is not capable of writing the looping instruction
#if not_good_enough
			using (FileStream fs = new FileStream(gifFilePath, FileMode.Create))
			{
				gifEnc.Save(fs);
			}
#endif // not_good_enough

			// From https://stackoverflow.com/questions/18719302/net-creating-a-looping-gif-using-gifbitmapencoder
			// We can manually inject the looping instruction into the gif using the NETSCAPE2.0 Application Extension,
			// after the gif header.
			using (var ms = new MemoryStream())
			{
				gifEnc.Save(ms);
				var fileBytes = ms.ToArray();
				// This is the NETSCAPE2.0 Application Extension.
				var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
				var newBytes = new List<byte>();
				newBytes.AddRange(fileBytes.Take(13));
				newBytes.AddRange(applicationExtension);
				newBytes.AddRange(fileBytes.Skip(13));
				File.WriteAllBytes(gifFilePath, newBytes.ToArray());
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

		private bool _exportMapGif = true;	// no UI way to specify yet

		private void ReportSelectedSeries()
		{
			var files = _saveGamesLookup[_series.Faction];

			_report = new Report();
			_processed = 0;
			_duplicates = 0;
			for (int i = 0; i < files.Count; i++)
			{
				if (_report.AddSaveGame(files[i].File, ref _duplicates, _exportMapGif))
					_processed++;
				backgroundReport.ReportProgress(0, i+1);
			}
		}

	}
}
