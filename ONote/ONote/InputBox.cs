using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ONote
{
    public partial class InputBox : Form
    {
        public InputBox(string title)
        {
            InitializeComponent();
            this.groupBox.Text = title;
        }

        /// <summary>普通输入框</summary>
        /// <param name="title">标题</param>
        /// <returns>对话框结果和输入文本</returns>
        public static InputBoxResult Show(string title)
        {
            InputBox inputBox = new InputBox(title);

            InputBoxResult result = new InputBoxResult();

            result.ReturnCode = inputBox.ShowDialog();
            result.Text = inputBox.txtInput.Text;

            return result;
        }

        /// <summary>密码输入框</summary>
        /// <param name="title">标题</param>
        /// <param name="passwordChar">密码代替字符</param>
        /// <returns>对话框结果和输入文本</returns>
        public static InputBoxResult Show(string title, char passwordChar)
        {
            InputBox inputBox = new InputBox(title);

            inputBox.txtInput.PasswordChar = passwordChar;

            InputBoxResult result = new InputBoxResult();

            result.ReturnCode = inputBox.ShowDialog();
            result.Text = inputBox.txtInput.Text;

            return result;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.btnConfirm.PerformClick();
            }
        }
    }
}
