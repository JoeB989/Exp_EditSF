
namespace CompareSFns
{
	partial class CompareSF
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			backgroundCompare = new System.ComponentModel.BackgroundWorker();
			statusStrip1 = new System.Windows.Forms.StatusStrip();
			StatusBar = new System.Windows.Forms.ToolStripStatusLabel();
			CopyButton = new System.Windows.Forms.Button();
			OutputListView = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			fileListView = new System.Windows.Forms.ListView();
			addButton = new System.Windows.Forms.Button();
			startButton = new System.Windows.Forms.Button();
			clearButton = new System.Windows.Forms.Button();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusBar });
			statusStrip1.Location = new System.Drawing.Point(0, 497);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			statusStrip1.Size = new System.Drawing.Size(933, 22);
			statusStrip1.TabIndex = 1;
			statusStrip1.Text = "statusStrip1";
			// 
			// StatusBar
			// 
			StatusBar.Name = "StatusBar";
			StatusBar.Size = new System.Drawing.Size(0, 17);
			// 
			// CopyButton
			// 
			CopyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			CopyButton.Location = new System.Drawing.Point(724, 494);
			CopyButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			CopyButton.Name = "CopyButton";
			CopyButton.Size = new System.Drawing.Size(146, 27);
			CopyButton.TabIndex = 2;
			CopyButton.Text = "Copy to Clipboard";
			CopyButton.UseVisualStyleBackColor = true;
			CopyButton.Click += CopyButton_Click;
			// 
			// OutputListView
			// 
			OutputListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			OutputListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
			OutputListView.FullRowSelect = true;
			OutputListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			OutputListView.Location = new System.Drawing.Point(14, 45);
			OutputListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			OutputListView.Name = "OutputListView";
			OutputListView.ShowGroups = false;
			OutputListView.Size = new System.Drawing.Size(905, 441);
			OutputListView.TabIndex = 3;
			OutputListView.UseCompatibleStateImageBehavior = false;
			OutputListView.View = System.Windows.Forms.View.Details;
			OutputListView.VirtualMode = true;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Differences";
			columnHeader1.Width = 1000;
			// 
			// fileListView
			// 
			fileListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			fileListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			fileListView.Location = new System.Drawing.Point(14, 5);
			fileListView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			fileListView.Name = "fileListView";
			fileListView.ShowGroups = false;
			fileListView.Size = new System.Drawing.Size(684, 36);
			fileListView.TabIndex = 4;
			fileListView.UseCompatibleStateImageBehavior = false;
			fileListView.View = System.Windows.Forms.View.SmallIcon;
			// 
			// addButton
			// 
			addButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			addButton.Location = new System.Drawing.Point(771, 7);
			addButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			addButton.Name = "addButton";
			addButton.Size = new System.Drawing.Size(52, 31);
			addButton.TabIndex = 5;
			addButton.Text = "Add...";
			addButton.UseVisualStyleBackColor = true;
			addButton.Click += addButton_Click;
			// 
			// startButton
			// 
			startButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			startButton.BackColor = System.Drawing.SystemColors.Info;
			startButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			startButton.Location = new System.Drawing.Point(831, 7);
			startButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			startButton.Name = "startButton";
			startButton.Size = new System.Drawing.Size(89, 31);
			startButton.TabIndex = 5;
			startButton.Text = "Compare";
			startButton.UseVisualStyleBackColor = false;
			startButton.Click += startButton_Click;
			// 
			// clearButton
			// 
			clearButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			clearButton.Location = new System.Drawing.Point(706, 7);
			clearButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			clearButton.Name = "clearButton";
			clearButton.Size = new System.Drawing.Size(52, 31);
			clearButton.TabIndex = 5;
			clearButton.Text = "Clear";
			clearButton.UseVisualStyleBackColor = true;
			clearButton.Click += clearButton_Click;
			// 
			// CompareSF
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(933, 519);
			Controls.Add(startButton);
			Controls.Add(clearButton);
			Controls.Add(addButton);
			Controls.Add(fileListView);
			Controls.Add(OutputListView);
			Controls.Add(CopyButton);
			Controls.Add(statusStrip1);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "CompareSF";
			Text = "CompareSF";
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		private System.ComponentModel.BackgroundWorker backgroundCompare;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel StatusBar;
		private System.Windows.Forms.Button CopyButton;
		private System.Windows.Forms.ListView OutputListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ListView fileListView;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Button startButton;
		private System.Windows.Forms.Button clearButton;
	}
}

