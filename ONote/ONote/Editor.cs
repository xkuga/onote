using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace ONote
{
    public partial class Editor : Form
    {
        private string AppName;            // 应用的名称
        private string CurrentPath;        // 当前文件的路径
        private string DeskTopPath;        // 系统桌面的路径
        private bool IsSaved;              // 标记文件是否保存
        private bool IsNewFile;            // 标记是否新文件
        private AES Aes;                   // Aes加密对象

        private int PrevFindLogicIndex;    // 上一次查找的逻辑下标
        private bool IsNewFind;            // 标记是否新的查找

        public Editor()
        {
            InitializeComponent();

            // 参数初始化
            this.Text = this.AppName = "ONote";

            this.IsSaved = true;
            this.IsNewFile = true;
            this.IsNewFind = true;

            this.PrevFindLogicIndex = -1;

            this.CurrentPath = "";
            this.DeskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        #region 文件菜单

        // 新建菜单项
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

        // 打开菜单项
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

        // 保存菜单项
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

        // 另存为菜单项
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

        // 退出菜单项
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        #endregion

        #region 编辑菜单

        // 查找菜单项
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.gbxFind.BringToFront();
            this.gbxFind.Show();
            this.txtFind.SelectAll();
            this.txtFind.Focus();
        }

        // 查找上一个
        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            this.Find("prev");
        }

        // 查找下一个
        private void btnFindNext_Click(object sender, EventArgs e)
        {
            this.Find("next");
        }

        // 查找
        private void Find(string mode)
        {
            // 获取查找内容在文中出现的所有下标
            int[] indexList = this.GetIndexList(this.txtFind.Text);

            if (indexList.Length != 0)
            {
                // 若是新的查找，则从头开始
                if (this.IsNewFind)
                {
                    this.PrevFindLogicIndex = -1;
                    this.IsNewFind = false;
                }

                int next, prev, logicIndex, start;

                // 根据不同的查找模试选择下标
                if (mode == "next")
                {
                    next = this.PrevFindLogicIndex + 1;
                    logicIndex = next < indexList.Length ? next : 0;
                    start = indexList[logicIndex];
                }
                else
                {
                    prev = this.PrevFindLogicIndex - 1;
                    logicIndex = prev >= 0 ? prev : indexList.Length - 1;
                    start = indexList[logicIndex];
                }

                // 记录上一次查找的逻辑下标
                this.PrevFindLogicIndex = logicIndex;

                // 高亮显示，切记此处要设置 HideSelection 属性为 false
                this.rtbEditor.Select(start, this.txtFind.Text.Length);

                // 焦点回到查找框，因为当自动换行关闭时焦点是不会自动回到查找框的
                this.txtFind.Focus();
            }
            else
            {
                this.statusBarLabel.Text = "Unable to find " + this.txtFind.Text;
            }
        }

        // 获取text在文中出现的所有下标，返回此下标列表，不区分大小写
        private int[] GetIndexList(string text)
        {
            int index;
            int start = 0;
            List<int> indexList = new List<int>();

            while ((index = this.rtbEditor.Text.ToLower().IndexOf(text.ToLower(), start)) != -1)
            {
                indexList.Add(index);
                start = index + text.Length;
            }

            return indexList.ToArray();
        }

        // 查找框键盘监听，Enter查找下一个，Shift+Enter查找上一个
        private void txtFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    this.btnFindPrev.PerformClick();
                }
                else
                {
                    this.btnFindNext.PerformClick();
                }
            }
        }

        // 查找框文本改变监听
        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            this.IsNewFind = true;
        }

        // 关闭查找窗口
        private void btnFindClose_Click(object sender, EventArgs e)
        {
            this.gbxFind.Hide();
            this.rtbEditor.Focus();
            this.txtFind.Text = "";
            this.statusBarLabel.Text = "";
        }

        #endregion

        #region 其它菜单

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

        #endregion

        #region 编辑窗口事件

        // 编辑文本改变
        private void rtbEditor_TextChanged(object sender, EventArgs e)
        {
            this.statusBarLabel.Text = "";
            this.Text = this.AppName + " *";
            this.IsSaved = false;
        }

        // 水平滚动条点击事件
        private void rtbEditor_HScroll(object sender, EventArgs e)
        {
            this.rtbEditor.Focus();
        }

        // 垂直滚动条点击事件
        private void rtbEditor_VScroll(object sender, EventArgs e)
        {
            this.rtbEditor.Focus();
        }

        #endregion

        #region 主窗口事件

        // 程序载入时进弹出密码输入框
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

        // 编码器大小改变
        private void Editor_Resize(object sender, EventArgs e)
        {
            this.gbxFind.SetBounds(this.Width - this.gbxFind.Width - 45, this.gbxFind.Location.Y, this.gbxFind.Width, this.gbxFind.Height);
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

        #endregion
    }
}
