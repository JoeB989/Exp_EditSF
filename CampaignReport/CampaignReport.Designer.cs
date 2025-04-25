
namespace CampaignReportNs
{
	partial class CampaignReport
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CampaignReport));
			label1 = new Label();
			FolderTextBox = new TextBox();
			DefaultFolderButton = new Button();
			label2 = new Label();
			GameListBox = new ListBox();
			BrowseFolderButton = new Button();
			ReportButton = new Button();
			ProgressBar = new ProgressBar();
			label3 = new Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(65, 97);
			label1.Name = "label1";
			label1.Size = new Size(43, 15);
			label1.TabIndex = 1;
			label1.Text = "Folder:";
			label1.TextAlign = ContentAlignment.MiddleRight;
			// 
			// FolderTextBox
			// 
			FolderTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			FolderTextBox.Location = new Point(114, 94);
			FolderTextBox.Name = "FolderTextBox";
			FolderTextBox.ReadOnly = true;
			FolderTextBox.Size = new Size(379, 23);
			FolderTextBox.TabIndex = 2;
			FolderTextBox.TabStop = false;
			// 
			// DefaultFolderButton
			// 
			DefaultFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			DefaultFolderButton.Location = new Point(510, 93);
			DefaultFolderButton.Name = "DefaultFolderButton";
			DefaultFolderButton.Size = new Size(75, 23);
			DefaultFolderButton.TabIndex = 3;
			DefaultFolderButton.Text = "Default";
			DefaultFolderButton.UseVisualStyleBackColor = true;
			DefaultFolderButton.Click += DefaultFolderButton_Click;
			// 
			// label2
			// 
			label2.Location = new Point(8, 153);
			label2.Name = "label2";
			label2.Size = new Size(100, 23);
			label2.TabIndex = 4;
			label2.Text = "Choose game:";
			label2.TextAlign = ContentAlignment.MiddleRight;
			// 
			// GameListBox
			// 
			GameListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			GameListBox.FormattingEnabled = true;
			GameListBox.ItemHeight = 15;
			GameListBox.Location = new Point(114, 153);
			GameListBox.Name = "GameListBox";
			GameListBox.Size = new Size(379, 184);
			GameListBox.TabIndex = 5;
			GameListBox.SelectedIndexChanged += GameListBox_SelectedIndexChanged;
			GameListBox.MouseDoubleClick += GameListBox_MouseDoubleClick;
			// 
			// BrowseFolderButton
			// 
			BrowseFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			BrowseFolderButton.Location = new Point(510, 122);
			BrowseFolderButton.Name = "BrowseFolderButton";
			BrowseFolderButton.Size = new Size(75, 23);
			BrowseFolderButton.TabIndex = 3;
			BrowseFolderButton.Text = "Browse...";
			BrowseFolderButton.UseVisualStyleBackColor = true;
			BrowseFolderButton.Click += BrowseFolderButton_Click;
			// 
			// ReportButton
			// 
			ReportButton.Anchor = AnchorStyles.Bottom;
			ReportButton.Location = new Point(209, 370);
			ReportButton.Name = "ReportButton";
			ReportButton.Size = new Size(181, 23);
			ReportButton.TabIndex = 3;
			ReportButton.Text = "Produce Report";
			ReportButton.UseVisualStyleBackColor = true;
			ReportButton.Click += ReportButton_Click;
			// 
			// ProgressBar
			// 
			ProgressBar.Anchor = AnchorStyles.Bottom;
			ProgressBar.Location = new Point(150, 414);
			ProgressBar.Name = "ProgressBar";
			ProgressBar.Size = new Size(308, 23);
			ProgressBar.TabIndex = 6;
			// 
			// label3
			// 
			label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			label3.Location = new Point(12, 9);
			label3.Name = "label3";
			label3.Size = new Size(580, 64);
			label3.TabIndex = 1;
			label3.Text = resources.GetString("label3.Text");
			// 
			// CampaignReport
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(604, 457);
			Controls.Add(ProgressBar);
			Controls.Add(GameListBox);
			Controls.Add(BrowseFolderButton);
			Controls.Add(ReportButton);
			Controls.Add(DefaultFolderButton);
			Controls.Add(FolderTextBox);
			Controls.Add(label2);
			Controls.Add(label3);
			Controls.Add(label1);
			Name = "CampaignReport";
			Text = "Campaign Report (for TW:Attila based save ganes)";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button ChooseSeriesButton;
		private Label label1;
		private TextBox FolderTextBox;
		private Button DefaultFolderButton;
		private Label label2;
		private ListBox GameListBox;
		private Button BrowseFolderButton;
		private Button ReportButton;
		private ProgressBar ProgressBar;
		private Label label3;
	}
}
