using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class formResetPass : Form
    {
        public string email = string.Empty;
        public string username = string.Empty;
        private string Y = null;
        private string Z = null;
        private string X = null;
        private int x;
        public formResetPass(string e, int i)
        {
            InitializeComponent();
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
            x = i;
            if(i == 1)
            {
                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    string query = "SELECT email FROM [user] WHERE username=" + "'" + e + "'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            email = dr.GetString(0);
                        }
                        username = e;
                    }
                    
                    dr.Close();
                    cn.Close();
                }
            } else
            {
                email = e;
            }
            labelEmail.Text = email;
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.AppSettings.Get("database"));
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if(textPass.Text == "" || textConfPass.Text == "")
            {
                MessageBox.Show("There's field empty", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                if(textConfPass.Text != textPass.Text)
                {
                    MessageBox.Show("Confirmation Password wrong!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else
                {
                    byte[] encrypted = null;
                    using (Aes myAes = Aes.Create())
                    {
                        // Encrypt the string to an array of bytes.
                        encrypted = KriptoKu.EncryptStringToBytes_Aes(textPass.Text, myAes.Key, myAes.IV);
                    } 

                    using (SqlConnection cn = GetConnection())
                    {
                        cn.Open();
                        string query = "SELECT username FROM [user] WHERE email=" + "'" + email + "'";                    
                        SqlCommand cmd = new SqlCommand(query, cn);
                        SqlDataReader dr = cmd.ExecuteReader();

                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                               username = dr.GetString(0);                               
                            }

                            X = KriptoKu.calculateX(username, textPass.Text.ToLower(), 0);
                            Y = KriptoKu.ToSHA256(X);
                            Z = KriptoKu.ToSHA256(X + KriptoKu.ToSHA256(KriptoKu.calculateX(username, textPass.Text.ToLower(), 1)));

                            dr.Close();
                            query = "Update [user] SET password=(@pass), Y=(@yy), Z=(@zz), i=(@ii) WHERE username=" + "'" + username + "'";
                            SqlCommand cmd3 = new SqlCommand(query, cn);
                            cmd3.Parameters.Add("@pass", SqlDbType.VarChar).Value = Convert.ToBase64String(encrypted);
                            cmd3.Parameters.Add("@yy", SqlDbType.VarChar).Value = Y;
                            cmd3.Parameters.Add("@zz", SqlDbType.VarChar).Value = Z;
                            cmd3.Parameters.Add("@ii", SqlDbType.Int).Value = 0;
                            dr = cmd3.ExecuteReader();

                            MessageBox.Show("Password Updated", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.Hide();
                            if (x == 1)
                            {
                                Dashboard f3 = new Dashboard(username);
                                f3.Show();
                            }
                            else
                            {
                                formLogin f4 = new formLogin();
                                f4.Show();
                            }

                        } else
                        {
                            MessageBox.Show("Password Updated Failed. Try again!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }                      
                        dr.Close();
                        cn.Close();
                    }
                }
            }
        }

        private void checkBoxPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPass.Checked)
            {
                textPass.PasswordChar = '\0';
                textConfPass.PasswordChar = '\0';
            }
            else
            {
                textPass.PasswordChar = '*';
                textConfPass.PasswordChar = '*';
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            if(x == 1)
            {
                Dashboard f3 = new Dashboard(username);
                f3.Show();
            } else
            {
                formLogin f4 = new formLogin();
                f4.Show();
            }
            
        }
    }
}
