using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class formForgotPass : Form
    {
        private string email_user = "";
        public formForgotPass()
        {
            InitializeComponent();
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.AppSettings.Get("database"));
        }

        private void label6_Click(object sender, EventArgs e)
        {
            var f2 = new formRegister();
            f2.Show();
            this.Hide();
        }

        private void buttonForgotPass_Click(object sender, EventArgs e)
        {
            if (textEmail.Text == "")
            {
                MessageBox.Show("Please enter your email first", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    string query = "SELECT email FROM [user] WHERE email=" + "'" + textEmail.Text.ToLower() + "'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            email_user = dr.GetString(0);
                        }

                        Random random = new Random();
                        string randomnumber = (random.Next(100000, 999999)).ToString();
                        string post = "This your verification Code : " + randomnumber;

                        MessageBox.Show(post, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        var f3 = new formOTP(email_user, randomnumber);
                        f3.Show();
                        this.Hide();

                    }
                    else
                    {
                        MessageBox.Show("No User Specified, Register First!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    dr.Close();
                    cn.Close();
                }
            }
        }
    }
}
