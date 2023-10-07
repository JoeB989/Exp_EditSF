using EsfLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareSF
{
	public partial class CompareSF : Form
	{
		public CompareSF(string[] files)
		{
			InitializeComponent();

			InitializeBackgroundWorker();

			OutputListView.RetrieveVirtualItem += OutputListView_RetrieveVirtualItem;
			OutputListView.CacheVirtualItems += OutputListView_CacheVirtualItems;

			CreateSaveFiles(files);
			StatusBar.Text = "Reading files ...";
			backgroundCompare.RunWorkerAsync();
		}

		private List<ListViewItem> Cache = new List<ListViewItem>();
		private int FirstCachedIndex;

		private void OutputListView_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
		{
			if ((e.StartIndex >= FirstCachedIndex) && (e.EndIndex < FirstCachedIndex + Cache.Count))
				return;
			Cache.Clear();
			FirstCachedIndex = e.StartIndex;
			for (int i = e.StartIndex; i <= e.EndIndex; i++)
				Cache.Add(new ListViewItem(Lines[i]));
		}

		private void OutputListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			if ((e.ItemIndex >= FirstCachedIndex) && (e.ItemIndex < FirstCachedIndex + Cache.Count))
				e.Item = Cache[e.ItemIndex - FirstCachedIndex];
			else
				e.Item = new ListViewItem(Lines[e.ItemIndex]);
		}

		private List<string> Lines = new List<string>();

		private void InitializeBackgroundWorker()
		{
			backgroundCompare.DoWork += BackgroundCompare_DoWork;
			backgroundCompare.RunWorkerCompleted += BackgroundCompare_RunWorkerCompleted;
			backgroundCompare.WorkerReportsProgress = true;
			backgroundCompare.ProgressChanged += BackgroundCompare_ProgressChanged;
		}

		private void BackgroundCompare_DoWork(object sender, DoWorkEventArgs e)
		{
			CompareFiles();
			e.Result = true;
		}

		private void BackgroundCompare_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			OutputListView.VirtualListSize = Lines.Count;

			if (e.Error != null)
			{
				StatusBar.Text = "Failed: " + e.Error.ToString();
				MessageBox.Show(e.Error.ToString(), "Compare error");
			}
			else
				StatusBar.Text = "Done.";
		}

		private void BackgroundCompare_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			List<string> results = (List<string>)e.UserState;

			StatusBar.Text = "Comparing ...";
			//OutputListBox.Text += content;
			//OutputListBox.Items.AddRange(results.ToArray());
			//OutputListBox.SelectedIndex = OutputListBox.Items.Count - 1;
			Lines.AddRange(results);

			// Updating this often causes flickering.  With the virtual listview, we could optionally
			// delay updating this to once every few seconds, and then on done or abort
			// TODO: a simple 5-second timer to update VirtualListSize if != Lines.Count 
			// Or every 100 lines

			//OutputListView.SuspendLayout();
			if (Lines.Count > 100 + OutputListView.VirtualListSize)
				OutputListView.VirtualListSize = Lines.Count;
			//OutputListView.ResumeLayout();
		}

		private struct SaveFile
		{
			public string FilePath;
			public string Name;
			public EsfFile Data;
			public ParentNode RootNode;
		}

		private SaveFile[] SaveFiles;

		private void CreateSaveFiles(string[] files)
		{
			int count = files.Length;
			StringBuilder sb = new StringBuilder();

			SaveFiles = new SaveFile[count];

			for (int file = 0; file < files.Length; file++)
			{
				SaveFiles[file] = new SaveFile();
				SaveFiles[file].FilePath = files[file];
				SaveFiles[file].Name = Path.GetFileName(SaveFiles[file].FilePath);

				if (file > 0)
					sb.Append(" ,");
				sb.Append(SaveFiles[file].Name);

				SaveFiles[file].Data = EsfCodecUtil.LoadEsfFile(SaveFiles[file].FilePath);
				SaveFiles[file].RootNode = (ParentNode)SaveFiles[file].Data.RootNode;
			}

			this.Text = "CompareSF:  Comparing " + sb.ToString();
		}

		private void CompareFiles()
		{
			Lines.Clear();
			//OutputListBox.ResetText();
			//int count = SaveFiles.Length;

			//SaveFiles = new SaveFile[count];

			//for (int file = 0; file < files.Length; file++)
			//{
			//	SaveFiles[file] = new SaveFile();
			//	SaveFiles[file].FilePath = files[file];
			//	SaveFiles[file].Name = Path.GetFileName(SaveFiles[file].FilePath);

			//	if (file > 0)
			//		sb.Append(" ,");
			//	sb.Append(SaveFiles[file].Name);

			//	SaveFiles[file].Data = EsfCodecUtil.LoadEsfFile(SaveFiles[file].FilePath);
			//	SaveFiles[file].RootNode = (ParentNode) SaveFiles[file].Data.RootNode;
			//}

			//this.Text = "CompareSF:  Comparing " + sb.ToString();

			//sb.Clear();
			string path = SaveFiles[0].RootNode.Name;
			var roots = (from file in SaveFiles
						 select (EsfNode) file.RootNode).ToList();

			CompareNodeAndChildren(path, roots);

			// TEMP: just output simple text for now
			//int x = sb.Length;
			//var ret = MessageBox.Show(sb.ToString(), "Click OK to copy report to clipboard", MessageBoxButtons.OKCancel);
			//if (ret == DialogResult.OK)
			//{
			//	Clipboard.SetText(sb.ToString());
			//}
		}

		private void CompareNodeAndChildren(string path, IList<EsfNode> fileNodes)
		{
			CompareValues(path, fileNodes);
			CompareChildren(path, fileNodes);
		}

		private void CompareValues(string path, IList<EsfNode> fileNodes)
		{
			List<string> results = new List<string>();
			StringBuilder sb = new StringBuilder();

			// count each record's values
			int[] valueCounts = new int[fileNodes.Count];
			RecordNode[] rNodes = new RecordNode[fileNodes.Count];
			int maxValueCount = 0;
			for (int file = 0; file < fileNodes.Count; file++)
			{
				rNodes[file] = fileNodes[file] as RecordNode;
				valueCounts[file] = (rNodes[file] != null) ? rNodes[file].Values.Count : 0;
				if (valueCounts[file] > maxValueCount)
					maxValueCount = valueCounts[file];
			}

			// HACK: this entry has 288k values, but will take looong time to process.
			// CAMPAIGN_SAVE_GAME\\COMPRESSED_DATA\\CAMPAIGN_ENV\\CAMPAIGN_MODEL\\CAI_INTERFACE\\CAI_HIGH_LEVEL_PATHFINDER
			// So truncate number of values we'll look at.
			int LIMIT = 100;
			if (maxValueCount > LIMIT)
				maxValueCount = LIMIT;

			// compare values
			string[] fileValues = new string[fileNodes.Count];
			bool foundDiff = false;
			for (int v = 0; v < maxValueCount; v++)
			{
				bool diff = false;
				for (int file=0; file < rNodes.Length; file++)
				{
					fileValues[file] = (v < valueCounts[file])
						? rNodes[file].Values[v].ToString()
						: string.Empty;
					if ((!diff) && (file > 0) && (fileValues[file] != fileValues[0]))
						diff = true;
				}
				// accumulate differences
				if (diff)
				{
					if (! foundDiff)
					{
						results.Add(path);
						//sb.AppendLine(path);
						foundDiff = true;
					}
					sb.AppendFormat(" [{0}]:", v);
					for (int file=0; file < fileValues.Length; file++)
					{
						//sb.AppendFormat("  {0}: {1}", SaveFiles[file].Name, fileValues[file]);
						if (file > 0)
							sb.Append(",");
						sb.AppendFormat(" {0}", fileValues[file]);
					}
					//sb.AppendLine();
					results.Add(sb.ToString());
					sb.Clear();
				}
			}
			if (results.Count > 0)
			{
				//System.Diagnostics.Debug.Write(sb.ToString());	// TEMP
				//OutputListBox.Text += (sb.ToString());
				backgroundCompare.ReportProgress(0, results);
			}
		}

		private void CompareChildren(string path, IList<EsfNode> fileNodes)
		{
			// use file[0] as the guide the other files map to
			// Caution: this may hide node additions in files 2+, needs testing
			ParentNode parentNode = fileNodes[0] as ParentNode;
			if (parentNode != null)
			{
				// check children
				int childCount = parentNode.Children.Count;
				List<EsfNode>[] childNodeLists = new List<EsfNode>[childCount];
				for (int child=0; child < childCount; child++)
				{
					childNodeLists[child] = new List<EsfNode>();
					for (int file = 0; file < fileNodes.Count; file++)
					{
						ParentNode pn = fileNodes[file] as ParentNode;
						EsfNode childNode = null;
						if ((pn != null) && (child < pn.Children.Count))
							childNode = pn.Children[child];
						childNodeLists[child].Add(childNode);
					}
				}

				//for (int file=0; file < fileNodes.Count; file++)
				//{
				//	childNodeLists[file] = new List<EsfNode>();
				//	ParentNode pn = fileNodes[file] as ParentNode;
				//	if (pn != null)
				//		childNodeLists[file] = (from child in pn.Children
				//							select child).ToList<EsfNode>();
				//	else
				//		childNodeLists[file] = new List<EsfNode>();
				//}

				// again using file[0]'s nodes as the guide
				for (int child = 0; child < childNodeLists.Length; child++)
				{
					ParentNode childNode = (ParentNode) childNodeLists[child][0];
					string childPath = path + "/" + childNode.Name;

					CompareNodeAndChildren(childPath, childNodeLists[child]);
				}
			}
		}

		private void CopyButton_Click(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string item in Lines)
				sb.AppendLine(item);
			if (sb.Length > 0)	// because SetText throws NullReferenceException if string is "" (which isn't null)
				Clipboard.SetText(sb.ToString());
		}
	}
}
