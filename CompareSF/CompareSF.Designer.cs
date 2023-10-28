
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
			this.backgroundCompare = new System.ComponentModel.BackgroundWorker();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.StatusBar = new System.Windows.Forms.ToolStripStatusLabel();
			this.CopyButton = new System.Windows.Forms.Button();
			this.OutputListView = new CompareSFns.MyListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.fileListView = new System.Windows.Forms.ListView();
			this.addButton = new System.Windows.Forms.Button();
			this.startButton = new System.Windows.Forms.Button();
			this.clearButton = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusBar});
			this.statusStrip1.Location = new System.Drawing.Point(0, 428);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(800, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// StatusBar
			// 
			this.StatusBar.Name = "StatusBar";
			this.StatusBar.Size = new System.Drawing.Size(0, 17);
			// 
			// CopyButton
			// 
			this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyButton.Location = new System.Drawing.Point(621, 428);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.Size = new System.Drawing.Size(125, 23);
			this.CopyButton.TabIndex = 2;
			this.CopyButton.Text = "Copy to Clipboard";
			this.CopyButton.UseVisualStyleBackColor = true;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// OutputListView
			// 
			this.OutputListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.OutputListView.FullRowSelect = true;
			this.OutputListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.OutputListView.HideSelection = false;
			this.OutputListView.Location = new System.Drawing.Point(12, 39);
			this.OutputListView.Name = "OutputListView";
			this.OutputListView.ShowGroups = false;
			this.OutputListView.Size = new System.Drawing.Size(776, 383);
			this.OutputListView.TabIndex = 3;
			this.OutputListView.UseCompatibleStateImageBehavior = false;
			this.OutputListView.View = System.Windows.Forms.View.Details;
			this.OutputListView.VirtualMode = true;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Differences";
			this.columnHeader1.Width = 1000;
			// 
			// fileListView
			// 
			this.fileListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fileListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.fileListView.HideSelection = false;
			this.fileListView.Location = new System.Drawing.Point(12, 4);
			this.fileListView.Name = "fileListView";
			this.fileListView.ShowGroups = false;
			this.fileListView.Size = new System.Drawing.Size(587, 32);
			this.fileListView.TabIndex = 4;
			this.fileListView.UseCompatibleStateImageBehavior = false;
			this.fileListView.View = System.Windows.Forms.View.SmallIcon;
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.Location = new System.Drawing.Point(675, 7);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(45, 27);
			this.addButton.TabIndex = 5;
			this.addButton.Text = "Add...";
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.addButton_Click);
			// 
			// startButton
			// 
			this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.startButton.Location = new System.Drawing.Point(731, 6);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(45, 27);
			this.startButton.TabIndex = 5;
			this.startButton.Text = "Start";
			this.startButton.UseVisualStyleBackColor = true;
			this.startButton.Click += new System.EventHandler(this.startButton_Click);
			// 
			// clearButton
			// 
			this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearButton.Location = new System.Drawing.Point(620, 7);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(45, 27);
			this.clearButton.TabIndex = 5;
			this.clearButton.Text = "Clear";
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// CompareSF
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.startButton);
			this.Controls.Add(this.clearButton);
			this.Controls.Add(this.addButton);
			this.Controls.Add(this.fileListView);
			this.Controls.Add(this.OutputListView);
			this.Controls.Add(this.CopyButton);
			this.Controls.Add(this.statusStrip1);
			this.Name = "CompareSF";
			this.Text = "CompareSF";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.ComponentModel.BackgroundWorker backgroundCompare;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel StatusBar;
		private System.Windows.Forms.Button CopyButton;
		private MyListView OutputListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ListView fileListView;
		private System.Windows.Forms.Button addButton;
		private System.Windows.Forms.Button startButton;
		private System.Windows.Forms.Button clearButton;
	}
}

