namespace NetworkPriorityManager
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private ComboBox comboBoxAdapter;
        private Label labelAdapter;
        private Label labelPriority;
        private TextBox textBoxPriority;
        private Button buttonSetPriority;
        private Button buttonRestoreDefault;
        private Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            labelAdapter = new Label();
            comboBoxAdapter = new ComboBox();
            labelPriority = new Label();
            textBoxPriority = new TextBox();
            buttonSetPriority = new Button();
            buttonRestoreDefault = new Button();
            statusLabel = new Label();
            SuspendLayout();
            // 
            // labelAdapter
            // 
            labelAdapter.AutoSize = true;
            labelAdapter.Location = new Point(15, 12);
            labelAdapter.Margin = new Padding(4, 0, 4, 0);
            labelAdapter.Name = "labelAdapter";
            labelAdapter.Size = new Size(118, 20);
            labelAdapter.TabIndex = 0;
            labelAdapter.Text = "选择网络适配器:";
            // 
            // comboBoxAdapter
            // 
            comboBoxAdapter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAdapter.FormattingEnabled = true;
            comboBoxAdapter.Location = new Point(15, 36);
            comboBoxAdapter.Margin = new Padding(4);
            comboBoxAdapter.Name = "comboBoxAdapter";
            comboBoxAdapter.Size = new Size(482, 28);
            comboBoxAdapter.TabIndex = 1;
            // 
            // labelPriority
            // 
            labelPriority.AutoSize = true;
            labelPriority.Location = new Point(15, 75);
            labelPriority.Margin = new Padding(4, 0, 4, 0);
            labelPriority.Name = "labelPriority";
            labelPriority.Size = new Size(159, 20);
            labelPriority.TabIndex = 2;
            labelPriority.Text = "设置优先级（整数）：";
            // 
            // textBoxPriority
            // 
            textBoxPriority.Location = new Point(15, 99);
            textBoxPriority.Margin = new Padding(4);
            textBoxPriority.Name = "textBoxPriority";
            textBoxPriority.Size = new Size(127, 27);
            textBoxPriority.TabIndex = 3;
            textBoxPriority.Text = "10";
            // 
            // buttonSetPriority
            // 
            buttonSetPriority.Location = new Point(15, 137);
            buttonSetPriority.Margin = new Padding(4);
            buttonSetPriority.Name = "buttonSetPriority";
            buttonSetPriority.Size = new Size(231, 31);
            buttonSetPriority.TabIndex = 4;
            buttonSetPriority.Text = "设置优先级";
            buttonSetPriority.UseVisualStyleBackColor = true;
            buttonSetPriority.Click += buttonSetPriority_Click;
            // 
            // buttonRestoreDefault
            // 
            buttonRestoreDefault.Location = new Point(267, 137);
            buttonRestoreDefault.Margin = new Padding(4);
            buttonRestoreDefault.Name = "buttonRestoreDefault";
            buttonRestoreDefault.Size = new Size(231, 31);
            buttonRestoreDefault.TabIndex = 5;
            buttonRestoreDefault.Text = "恢复默认";
            buttonRestoreDefault.UseVisualStyleBackColor = true;
            buttonRestoreDefault.Click += buttonRestoreDefault_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(15, 179);
            statusLabel.Margin = new Padding(4, 0, 4, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(81, 20);
            statusLabel.TabIndex = 6;
            statusLabel.Text = "等待操作...";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(514, 213);
            Controls.Add(statusLabel);
            Controls.Add(buttonRestoreDefault);
            Controls.Add(buttonSetPriority);
            Controls.Add(textBoxPriority);
            Controls.Add(labelPriority);
            Controls.Add(comboBoxAdapter);
            Controls.Add(labelAdapter);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "Network Priority Manager";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}