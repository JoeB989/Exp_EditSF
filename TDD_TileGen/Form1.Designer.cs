namespace TDD_TileGen
{
	partial class Form1
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
			label1 = new Label();
			InputFileLabel = new Label();
			label2 = new Label();
			OutputFileLabel = new Label();
			SelectInputButton = new Button();
			SelectOutputButton = new Button();
			GenerateButton = new Button();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(93, 17);
			label1.Name = "label1";
			label1.Size = new Size(57, 15);
			label1.TabIndex = 0;
			label1.Text = "Input file:";
			// 
			// InputFileLabel
			// 
			InputFileLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			InputFileLabel.BorderStyle = BorderStyle.Fixed3D;
			InputFileLabel.Location = new Point(167, 17);
			InputFileLabel.MinimumSize = new Size(100, 0);
			InputFileLabel.Name = "InputFileLabel";
			InputFileLabel.Size = new Size(618, 17);
			InputFileLabel.TabIndex = 0;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(93, 55);
			label2.Name = "label2";
			label2.Size = new Size(67, 15);
			label2.TabIndex = 0;
			label2.Text = "Output file:";
			// 
			// OutputFileLabel
			// 
			OutputFileLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			OutputFileLabel.BorderStyle = BorderStyle.Fixed3D;
			OutputFileLabel.Location = new Point(167, 55);
			OutputFileLabel.MinimumSize = new Size(100, 0);
			OutputFileLabel.Name = "OutputFileLabel";
			OutputFileLabel.Size = new Size(618, 17);
			OutputFileLabel.TabIndex = 0;
			// 
			// SelectInputButton
			// 
			SelectInputButton.Location = new Point(12, 13);
			SelectInputButton.Name = "SelectInputButton";
			SelectInputButton.Size = new Size(75, 23);
			SelectInputButton.TabIndex = 0;
			SelectInputButton.Text = "Select...";
			SelectInputButton.UseVisualStyleBackColor = true;
			SelectInputButton.Click += SelectInputButton_Click;
			// 
			// SelectOutputButton
			// 
			SelectOutputButton.Location = new Point(12, 51);
			SelectOutputButton.Name = "SelectOutputButton";
			SelectOutputButton.Size = new Size(75, 23);
			SelectOutputButton.TabIndex = 1;
			SelectOutputButton.Text = "Select...";
			SelectOutputButton.UseVisualStyleBackColor = true;
			SelectOutputButton.Click += SelectOutputButton_Click;
			// 
			// GenerateButton
			// 
			GenerateButton.Anchor = AnchorStyles.Bottom;
			GenerateButton.Enabled = false;
			GenerateButton.Location = new Point(360, 93);
			GenerateButton.Name = "GenerateButton";
			GenerateButton.Size = new Size(75, 23);
			GenerateButton.TabIndex = 2;
			GenerateButton.Text = "Generate";
			GenerateButton.UseVisualStyleBackColor = true;
			GenerateButton.Click += GenerateButton_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(797, 128);
			Controls.Add(GenerateButton);
			Controls.Add(SelectOutputButton);
			Controls.Add(SelectInputButton);
			Controls.Add(OutputFileLabel);
			Controls.Add(InputFileLabel);
			Controls.Add(label2);
			Controls.Add(label1);
			Name = "Form1";
			Text = "TDD Tile_Upgrades Generator";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label1;
		private Label InputFileLabel;
		private Label label2;
		private Label OutputFileLabel;
		private Button SelectInputButton;
		private Button SelectOutputButton;
		private Button GenerateButton;
	}
}
