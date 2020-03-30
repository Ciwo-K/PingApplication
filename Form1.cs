using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pinger
{
    public partial class Form1 : Form
    {
        private Worker worker;
        private static Form1 formInstance = null;
        private Form1(Worker worker)
        {
            InitializeComponent();
            this.worker = worker;
        }

        public static Form1 InititalizeForm(Worker worker)
        {
            if (formInstance == null)
            {
                formInstance = new Form1(worker);
            }
            return formInstance;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void maskedTextBox2_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            worker.SetAddress(textBox1.Text);
            worker.SetPingsPerMinute((int)numericUpDown1.Value);
            worker.SetRanges(textBox2.Text.ToString(), textBox3.Text.ToString());
            Program.SaveState();
            this.Close();

        }

        private void EnterYeet(object sender, KeyEventArgs e)
        {
            if (IsInputKey(Keys.Enter))
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void EnterTab(object sender, KeyPressEventArgs e)
        {
            if (IsInputKey(Keys.Enter))
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void ApplyAndClose(object sender, KeyPressEventArgs e)
        {
            if (IsInputKey(Keys.Enter))
            {
                button1_Click(sender, e);
            }
        }

        private void EnterYeet(object sender, KeyPressEventArgs e)
        {
            if (IsInputKey(Keys.Enter))
            {
                SendKeys.Send("{TAB}");
            }
        }

        private new void Closing(object sender, FormClosingEventArgs e)
        {
            formInstance = null;
        }
    }
}
