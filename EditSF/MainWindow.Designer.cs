namespace EditSF {
    partial class EditSF {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			addBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			bookmarkSeparator = new System.Windows.Forms.ToolStripSeparator();
			optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			writeLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			showNodeTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			runSingleTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			runTestsStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			reportsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			playerFactionReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			allFactionsEconomicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			allFactionsCharactersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			verifySettlementsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			fixCharacterSkillsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			statusBar = new System.Windows.Forms.StatusStrip();
			progressBar = new System.Windows.Forms.ToolStripProgressBar();
			statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			editEsfComponent = new EsfControl.EditEsfComponent();
			StrengthRankMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			menuStrip1.SuspendLayout();
			statusBar.SuspendLayout();
			SuspendLayout();
			// 
			// menuStrip1
			// 
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, bookmarksToolStripMenuItem, optionsToolStripMenuItem, testToolStripMenuItem, helpToolStripMenuItem, reportsToolStripMenuItem });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			menuStrip1.Size = new System.Drawing.Size(920, 24);
			menuStrip1.TabIndex = 1;
			menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, exitToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			fileToolStripMenuItem.Text = "File";
			// 
			// openToolStripMenuItem
			// 
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			openToolStripMenuItem.Text = "Open";
			openToolStripMenuItem.Click += openToolStripMenuItem_Click;
			// 
			// saveToolStripMenuItem
			// 
			saveToolStripMenuItem.Enabled = false;
			saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			saveToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			saveToolStripMenuItem.Text = "Save";
			saveToolStripMenuItem.Click += saveToolStripMenuItem1_Click;
			// 
			// saveAsToolStripMenuItem
			// 
			saveAsToolStripMenuItem.Enabled = false;
			saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			saveAsToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			saveAsToolStripMenuItem.Text = "Save As...";
			saveAsToolStripMenuItem.Click += saveToolStripMenuItem_Click;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
			// 
			// bookmarksToolStripMenuItem
			// 
			bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addBookmarkToolStripMenuItem, editBookmarkToolStripMenuItem, bookmarkSeparator });
			bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
			bookmarksToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
			bookmarksToolStripMenuItem.Text = "Bookmarks";
			// 
			// addBookmarkToolStripMenuItem
			// 
			addBookmarkToolStripMenuItem.Enabled = false;
			addBookmarkToolStripMenuItem.Name = "addBookmarkToolStripMenuItem";
			addBookmarkToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			addBookmarkToolStripMenuItem.Text = "Add Bookmark";
			addBookmarkToolStripMenuItem.Click += AddBookmark;
			// 
			// editBookmarkToolStripMenuItem
			// 
			editBookmarkToolStripMenuItem.Name = "editBookmarkToolStripMenuItem";
			editBookmarkToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			editBookmarkToolStripMenuItem.Text = "Edit Bookmarks";
			editBookmarkToolStripMenuItem.Click += EditBookmarks;
			// 
			// bookmarkSeparator
			// 
			bookmarkSeparator.Name = "bookmarkSeparator";
			bookmarkSeparator.Size = new System.Drawing.Size(153, 6);
			// 
			// optionsToolStripMenuItem
			// 
			optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { writeLogFileToolStripMenuItem, showNodeTypeToolStripMenuItem });
			optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
			optionsToolStripMenuItem.Text = "Options";
			// 
			// writeLogFileToolStripMenuItem
			// 
			writeLogFileToolStripMenuItem.CheckOnClick = true;
			writeLogFileToolStripMenuItem.Name = "writeLogFileToolStripMenuItem";
			writeLogFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			writeLogFileToolStripMenuItem.Text = "Write Log File";
			// 
			// showNodeTypeToolStripMenuItem
			// 
			showNodeTypeToolStripMenuItem.CheckOnClick = true;
			showNodeTypeToolStripMenuItem.Enabled = false;
			showNodeTypeToolStripMenuItem.Name = "showNodeTypeToolStripMenuItem";
			showNodeTypeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			showNodeTypeToolStripMenuItem.Text = "Show Node Type";
			showNodeTypeToolStripMenuItem.Click += showNodeTypeToolStripMenuItem_Click;
			// 
			// testToolStripMenuItem
			// 
			testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { runSingleTestToolStripMenuItem, runTestsStripMenuItem });
			testToolStripMenuItem.Name = "testToolStripMenuItem";
			testToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
			testToolStripMenuItem.Text = "Tests";
			testToolStripMenuItem.Visible = false;
			// 
			// runSingleTestToolStripMenuItem
			// 
			runSingleTestToolStripMenuItem.Name = "runSingleTestToolStripMenuItem";
			runSingleTestToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			runSingleTestToolStripMenuItem.Text = "Run Load/Save Test";
			runSingleTestToolStripMenuItem.Click += runSingleTestToolStripMenuItem_Click;
			// 
			// runTestsStripMenuItem
			// 
			runTestsStripMenuItem.Name = "runTestsStripMenuItem";
			runTestsStripMenuItem.Size = new System.Drawing.Size(177, 22);
			runTestsStripMenuItem.Text = "Multiple Tests";
			runTestsStripMenuItem.Click += runTestsToolStripMenuItem_Click;
			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutToolStripMenuItem });
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			aboutToolStripMenuItem.Text = "About";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// reportsToolStripMenuItem
			// 
			reportsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { playerFactionReportToolStripMenuItem, allFactionsEconomicsToolStripMenuItem, allFactionsCharactersToolStripMenuItem, StrengthRankMenuItem, toolStripMenuItem1, verifySettlementsToolStripMenuItem, toolStripMenuItem2, fixCharacterSkillsToolStripMenuItem });
			reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
			reportsToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
			reportsToolStripMenuItem.Text = "Reports";
			reportsToolStripMenuItem.Visible = false;
			// 
			// playerFactionReportToolStripMenuItem
			// 
			playerFactionReportToolStripMenuItem.Name = "playerFactionReportToolStripMenuItem";
			playerFactionReportToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			playerFactionReportToolStripMenuItem.Text = "Player Faction Report";
			playerFactionReportToolStripMenuItem.Click += playerFactionReportToolStripMenuItem_Click;
			// 
			// allFactionsEconomicsToolStripMenuItem
			// 
			allFactionsEconomicsToolStripMenuItem.Name = "allFactionsEconomicsToolStripMenuItem";
			allFactionsEconomicsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			allFactionsEconomicsToolStripMenuItem.Text = "All Factions Economics";
			allFactionsEconomicsToolStripMenuItem.Click += allFactionsEconomicsToolStripMenuItem_Click;
			// 
			// allFactionsCharactersToolStripMenuItem
			// 
			allFactionsCharactersToolStripMenuItem.Name = "allFactionsCharactersToolStripMenuItem";
			allFactionsCharactersToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			allFactionsCharactersToolStripMenuItem.Text = "All Factions Characters";
			allFactionsCharactersToolStripMenuItem.Click += allFactionsCharactersToolStripMenuItem_Click;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
			// 
			// verifySettlementsToolStripMenuItem
			// 
			verifySettlementsToolStripMenuItem.Name = "verifySettlementsToolStripMenuItem";
			verifySettlementsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			verifySettlementsToolStripMenuItem.Text = "Verify Settlements";
			verifySettlementsToolStripMenuItem.Click += verifySettlementsToolStripMenuItem_Click;
			// 
			// toolStripMenuItem2
			// 
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			toolStripMenuItem2.Size = new System.Drawing.Size(193, 6);
			// 
			// fixCharacterSkillsToolStripMenuItem
			// 
			fixCharacterSkillsToolStripMenuItem.Name = "fixCharacterSkillsToolStripMenuItem";
			fixCharacterSkillsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
			fixCharacterSkillsToolStripMenuItem.Text = "Fix Character Skills";
			fixCharacterSkillsToolStripMenuItem.Click += fixCharacterSkillsToolStripMenuItem_Click;
			// 
			// statusBar
			// 
			statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { progressBar, statusLabel });
			statusBar.Location = new System.Drawing.Point(0, 889);
			statusBar.Name = "statusBar";
			statusBar.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			statusBar.Size = new System.Drawing.Size(920, 24);
			statusBar.TabIndex = 2;
			// 
			// progressBar
			// 
			progressBar.Name = "progressBar";
			progressBar.Size = new System.Drawing.Size(117, 18);
			// 
			// statusLabel
			// 
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(86, 19);
			statusLabel.Text = "No File Loaded";
			// 
			// editEsfComponent
			// 
			editEsfComponent.Dock = System.Windows.Forms.DockStyle.Fill;
			editEsfComponent.Location = new System.Drawing.Point(0, 24);
			editEsfComponent.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			editEsfComponent.Name = "editEsfComponent";
			editEsfComponent.RootNode = null;
			editEsfComponent.ShowCode = false;
			editEsfComponent.Size = new System.Drawing.Size(920, 865);
			editEsfComponent.TabIndex = 3;
			// 
			// StrengthRankMenuItem
			// 
			StrengthRankMenuItem.Name = "StrengthRankMenuItem";
			StrengthRankMenuItem.Size = new System.Drawing.Size(196, 22);
			StrengthRankMenuItem.Text = "Strength Rank Report";
			StrengthRankMenuItem.Click += StrengthRankMenuItem_Click;
			// 
			// EditSF
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(920, 913);
			Controls.Add(editEsfComponent);
			Controls.Add(statusBar);
			Controls.Add(menuStrip1);
			MainMenuStrip = menuStrip1;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "EditSF";
			Text = "EditSF";
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			statusBar.ResumeLayout(false);
			statusBar.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runTestsStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runSingleTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator bookmarkSeparator;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showNodeTypeToolStripMenuItem;
        private EsfControl.EditEsfComponent editEsfComponent;
		private System.Windows.Forms.ToolStripMenuItem reportsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem playerFactionReportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem allFactionsEconomicsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem allFactionsCharactersToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem verifySettlementsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem fixCharacterSkillsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem StrengthRankMenuItem;
	}
}

