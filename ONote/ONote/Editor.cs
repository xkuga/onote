using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ONote
{
    public partial class Editor : Form
    {
        private string AppName;       // 应用的名称
        private string CurrentPath;   // 当前文件的路径
        private string DeskTopPath;   // 系统桌面的路径
        private bool IsSaved;         // 文件是否保存标记
        private bool IsNewFile;       // 是否新文件标记
        private AES Aes;              // Aes加密对象

        public Editor()
        {
            InitializeComponent();

            // 参数初始化
            this.Text = this.AppName = "ONote";
            this.IsSaved = true;
            this.IsNewFile = true;
            this.CurrentPath = "";
            this.DeskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            InputBoxResult result = new InputBoxResult();
            result = InputBox.Show("Input Password", '*');
            if (result.ReturnCode == DialogResult.Cancel)
            {
                Application.Exit();
            }
            this.Aes = new AES(result.Text);
        }

        // 新建文件
        private void newMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.IsSaved)
            {
                DialogResult result = MessageBox.Show("Save the current file?", "Notice", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    this.saveMenuItem.PerformClick();
                }
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            this.rtbEditor.Text = "";
            this.Text = this.AppName;
            this.IsNewFile = true;
            this.IsSaved = true;
        }

        // 打开文件
        private void openMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.IsSaved)
            {
                DialogResult result = MessageBox.Show("Save the current file?", "Notice", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    this.saveMenuItem.PerformClick();
                }
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = this.DeskTopPath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream stream = openFileDialog.OpenFile())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        try
                        {
                            this.rtbEditor.Text = this.Aes.Decrypt(streamReader.ReadToEnd());

                            this.CurrentPath = openFileDialog.FileName;
                            this.statusBarLabel.Text = "";
                            this.Text = this.AppName;
                            this.IsNewFile = false;
                            this.IsSaved = true;
                        }
                        catch
                        {
                            MessageBox.Show("Open Failed: maybe wrong password :(");
                        }
                    }
                }
            }
        }

        // 保存文件，若是新文件则调用另存为，否则直接保存
        private void saveMenuItem_Click(object sender, EventArgs e)
        {
            if (this.IsNewFile)
            {
                this.saveAsMenuItem.PerformClick();
            }
            else
            {
                this.SaveFile();
            }
        }

        // 另存为
        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = this.DeskTopPath;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.CurrentPath = saveFileDialog.FileName;
                this.SaveFile();
            }
        }

        // 退出
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // 关于
        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ONE PIECE");
        }


        // 自动换行
        private void wordWrapMenuItem_Click(object sender, EventArgs e)
        {
            this.rtbEditor.WordWrap = !this.rtbEditor.WordWrap;
            this.statusBarLabel.Text = this.rtbEditor.WordWrap ? "自动换行(开启)" : "自动换行(关闭)";
        }

        // 修改密码
        private void changePasswordMenuItem_Click(object sender, EventArgs e)
        {
            InputBoxResult result = new InputBoxResult();
            result = InputBox.Show("Input Password", '*');
            if (result.ReturnCode == DialogResult.OK)
            {
                this.Aes = new AES(result.Text);
            }
        }

        // 保存文件
        private void SaveFile()
        {
            string content = this.rtbEditor.Text;
            string ciphertext = this.Aes.Encrypt(content);

            using (StreamWriter streamWriter = new StreamWriter(this.CurrentPath))
            {
                streamWriter.Write(ciphertext);
            }

            this.statusBarLabel.Text = "Saved";
            this.Text = this.AppName;
            this.IsNewFile = false;
            this.IsSaved = true;
        }

        // 编辑文本改变
        private void rtbEditor_TextChanged(object sender, EventArgs e)
        {
            this.statusBarLabel.Text = "";
            this.Text = this.AppName + " *";
            this.IsSaved = false;
        }

        // 关闭程序
        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.IsSaved)
            {
                DialogResult result = MessageBox.Show("Save the current file?", "Notice", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    this.saveMenuItem.PerformClick();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

    }
}
