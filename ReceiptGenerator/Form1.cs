using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ReceiptGenerator
{
    /// <summary>
    /// 主窗体
    /// 实现功能：
    ///     生成真实可用的区块链钱包地址（ERC20/Polygon/Tron）
    ///     生成私钥、收款地址、链类型
    ///     保存为 CSV 表格文件
    /// </summary>
    public partial class Form1 : Form
    {
        // 运行时新增控件
        private DataGridView dataGridViewResults;
        private Button buttonSaveCsv;
        private Button buttonClear;
        private NumericUpDown numericUpDownCount;
        private Label labelCount;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 禁止更改窗体大小
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "ReceiptGenerator - 真实地址生成";

            // comboBox_ReceiptChain 添加选项
            comboBox_ReceiptChain.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_ReceiptChain.Items.Clear();
            comboBox_ReceiptChain.Items.Add("TRC20");
            comboBox_ReceiptChain.Items.Add("ERC20");
            comboBox_ReceiptChain.Items.Add("Polygon");
            comboBox_ReceiptChain.SelectedIndex = 0;

            // 对现有按钮做下布局与文本
            if (button_Generator != null)
            {
                button_Generator.Text = "生成";
                button_Generator.Location = new Point(352, 12);
                button_Generator.Width = 80;
                button_Generator.Height = 26;
            }

            // 让链下拉在左上
            comboBox_ReceiptChain.Location = new Point(16, 14);
            comboBox_ReceiptChain.Width = 120;

            InitializeRuntimeControls();
            ConfigureGrid();

            // 最小窗口尺寸
            this.MinimumSize = new Size(700, 520);
            if (this.ClientSize.Width < 680 || this.ClientSize.Height < 460)
            {
                this.ClientSize = new Size(680, 460);
            }
        }

        private void InitializeRuntimeControls()
        {
            // 数量
            labelCount = new Label
            {
                AutoSize = true,
                Text = "数量：",
                Location = new Point(150, 18)
            };
            this.Controls.Add(labelCount);

            numericUpDownCount = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 2000,
                Value = 10,
                Location = new Point(196, 14),
                Width = 80
            };
            this.Controls.Add(numericUpDownCount);

            // 保存CSV
            buttonSaveCsv = new Button
            {
                Text = "保存为CSV",
                Location = new Point(440, 12),
                Width = 100,
                Height = 26
            };
            buttonSaveCsv.Click += buttonSaveCsv_Click;
            this.Controls.Add(buttonSaveCsv);

            // 清空
            buttonClear = new Button
            {
                Text = "清空",
                Location = new Point(546, 12),
                Width = 80,
                Height = 26
            };
            buttonClear.Click += buttonClear_Click;
            this.Controls.Add(buttonClear);

            // 结果表格
            dataGridViewResults = new DataGridView
            {
                Location = new Point(16, 48),
                Size = new Size(650, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dataGridViewResults.CellDoubleClick += (s, e) =>
            {
                // 双击复制整行到剪贴板
                if (e.RowIndex >= 0)
                {
                    var row = dataGridViewResults.Rows[e.RowIndex];
                    string line = $"{row.Cells["Chain"].Value}\t{row.Cells["PrivateKey"].Value}\t{row.Cells["Address"].Value}";
                    try
                    {
                        Clipboard.SetText(line);
                        ToolTip tip = new ToolTip();
                        tip.Show("已复制该行到剪贴板", this, PointToClient(Cursor.Position), 1200);
                    }
                    catch { /* 忽略剪贴板异常 */ }
                }
            };
            this.Controls.Add(dataGridViewResults);
        }

        private void ConfigureGrid()
        {
            dataGridViewResults.Columns.Clear();

            var colChain = new DataGridViewTextBoxColumn
            {
                HeaderText = "链类型",
                Name = "Chain",
                FillWeight = 15
            };
            var colPrivKey = new DataGridViewTextBoxColumn
            {
                HeaderText = "私钥(HEX)",
                Name = "PrivateKey",
                FillWeight = 40
            };
            var colAddress = new DataGridViewTextBoxColumn
            {
                HeaderText = "收款地址",
                Name = "Address",
                FillWeight = 45
            };

            dataGridViewResults.Columns.AddRange(colChain, colPrivKey, colAddress);
        }

        private void button_Generator_Click(object sender, EventArgs e)
        {
            var chain = comboBox_ReceiptChain.SelectedItem?.ToString() ?? "ERC20";
            int count = (int)numericUpDownCount.Value;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var wallet = Crypto.WalletGenerator.Generate(chain);
                    dataGridViewResults.Rows.Add(chain, wallet.PrivateKeyHex, wallet.Address);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSaveCsv_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.Rows.Count == 0)
            {
                MessageBox.Show("没有可保存的数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "保存为 CSV";
                sfd.Filter = "CSV 文件 (*.csv)|*.csv";
                sfd.FileName = $"wallets_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        SaveGridToCsv(sfd.FileName);
                        MessageBox.Show("保存成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.Rows.Count == 0) return;

            var confirm = MessageBox.Show("确定清空列表吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                dataGridViewResults.Rows.Clear();
            }
        }

        private void SaveGridToCsv(string filePath)
        {
            // UTF-8 BOM，避免 Excel 乱码
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs, new UTF8Encoding(true)))
            {
                // 表头
                writer.WriteLine("链类型,私钥(HEX),收款地址");

                foreach (DataGridViewRow row in dataGridViewResults.Rows)
                {
                    if (row.IsNewRow) continue;

                    string chain = EscapeCsv(Convert.ToString(row.Cells["Chain"].Value));
                    string priv = EscapeCsv(Convert.ToString(row.Cells["PrivateKey"].Value));
                    string addr = EscapeCsv(Convert.ToString(row.Cells["Address"].Value));

                    writer.WriteLine($"{chain},{priv},{addr}");
                }
            }
        }

        private static string EscapeCsv(string input)
        {
            if (input == null) return string.Empty;
            bool needQuote = input.Contains(",") || input.Contains("\"") || input.Contains("\n") || input.Contains("\r");
            if (needQuote)
            {
                input = input.Replace("\"", "\"\"");
                return $"\"{input}\"";
            }
            return input;
        }
    }
}
