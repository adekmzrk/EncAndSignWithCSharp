using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class Dashboard : Form
    {
        public string username;
        public Dashboard(string user)
        {
            InitializeComponent();
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
            username = user;
            panelMenu.Height = 30;

            label1.Text = "Howdy, " + username;
        }
        
        private void label6_Click(object sender, EventArgs e)
        {
            formGenCert f2 = new formGenCert(username);
            f2.ShowDialog();
            this.Hide();
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            this.Close();

            formLogin f3 = new formLogin();
            f3.ShowDialog();
            
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            this.Hide();

            Encrypt f3 = new Encrypt(username);
            f3.Show();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            panel2_Click(sender, e);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            panel2_Click(sender, e);
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            this.Hide();

            Decrypt f4 = new Decrypt(username);
            f4.Show();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            panel3_Click(sender, e);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            panel3_Click(sender, e);
        }

        private void buttonChangePass_Click(object sender, EventArgs e)
        {
            formResetPass f4 = new formResetPass(username, 1);
            f4.ShowDialog();
            this.Hide();
        }

        private void buttonCheckUser_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming Soon!");
        }

        private void buttonMenu_Click(object sender, EventArgs e)
        {
            if(panelMenu.Height == 119)
            {
                panelMenu.Height = 30;
            } else
            {
                panelMenu.Height = 119;
            }
        }

    }
}
