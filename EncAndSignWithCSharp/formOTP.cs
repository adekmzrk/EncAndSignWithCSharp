using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class formOTP : Form
    {
        public string otpp = "";
        public string email = "";
        public formOTP(string email_user, string otp)
        {
            InitializeComponent();
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
            otpp = otp;
            email = email_user;
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if(textOTP.Text == "")
            {
                MessageBox.Show("Please enter your verification code", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                if (textOTP.Text == otpp)
                {
                    MessageBox.Show("Verification success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    var f2 = new formResetPass(email, 0);
                    f2.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Verification failed. Try again!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            
        }

        private void label6_Click(object sender, EventArgs e)
        {
            var f3 = new formForgotPass();
            f3.Show();
            this.Hide();
        }

    }
}
