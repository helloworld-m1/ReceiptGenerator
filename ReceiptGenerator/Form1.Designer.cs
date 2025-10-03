namespace ReceiptGenerator
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox_ReceiptChain = new System.Windows.Forms.ComboBox();
            this.button_Generator = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_ReceiptChain
            // 
            this.comboBox_ReceiptChain.FormattingEnabled = true;
            this.comboBox_ReceiptChain.Location = new System.Drawing.Point(12, 12);
            this.comboBox_ReceiptChain.Name = "comboBox_ReceiptChain";
            this.comboBox_ReceiptChain.Size = new System.Drawing.Size(166, 23);
            this.comboBox_ReceiptChain.TabIndex = 0;
            // 
            // button_Generator
            // 
            this.button_Generator.Location = new System.Drawing.Point(184, 6);
            this.button_Generator.Name = "button_Generator";
            this.button_Generator.Size = new System.Drawing.Size(127, 32);
            this.button_Generator.TabIndex = 4;
            this.button_Generator.Text = "生成";
            this.button_Generator.UseVisualStyleBackColor = true;
            this.button_Generator.Click += new System.EventHandler(this.button_Generator_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(453, 234);
            this.Controls.Add(this.button_Generator);
            this.Controls.Add(this.comboBox_ReceiptChain);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_ReceiptChain;
        private System.Windows.Forms.Button button_Generator;
    }
}

