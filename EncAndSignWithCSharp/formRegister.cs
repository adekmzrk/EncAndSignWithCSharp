using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Configuration;

namespace EncAndSignWithCSharp
{
    public partial class formRegister : Form
    {
        private string Y = null;
        private string Z = null;
        private string X = null;
        private int i = 0;
        private byte[] encrypted;

        public formRegister()
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
            var f2 = new formLogin();
            f2.Show();
            this.Hide();
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            if(textEmail.Text == "" || textUsername.Text == "" || textPassword.Text == "")
            {
                MessageBox.Show("There's field empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    encrypted = KriptoKu.EncryptStringToBytes_Aes(textPassword.Text, myAes.Key, myAes.IV);
                }

                X = KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i);
                Y = KriptoKu.ToSHA256(X);
                Z = KriptoKu.ToSHA256(X + KriptoKu.ToSHA256(KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i + 1)));
                string pass_enc = Convert.ToBase64String(encrypted);

                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    string query = "SELECT * FROM [user] WHERE email=" + "'" + textEmail.Text.ToLower() + "'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        MessageBox.Show("Email already exist", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);   
                    } else
                    {
                        dr.Close();
                        query = "SELECT * FROM [user] WHERE username=" + "'" + textUsername.Text.ToLower() + "'";
                        SqlCommand cmd2 = new SqlCommand(query, cn);
                        dr = cmd2.ExecuteReader();

                        if (dr.HasRows)
                        {
                            MessageBox.Show("Username already exist", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        } else
                        {
                            dr.Close();
                            query = @"INSERT INTO [user](email, username, password, Y, Z, i) VALUES(@email, @username, @password, @y, @z, @i)";
                            SqlCommand cmd3 = new SqlCommand(query, cn);
                            cmd3.Parameters.Add("@email", SqlDbType.VarChar).Value = textEmail.Text.ToLower();
                            cmd3.Parameters.Add("@username", SqlDbType.VarChar).Value = textUsername.Text.ToLower();
                            cmd3.Parameters.Add("@password", SqlDbType.VarChar).Value = pass_enc;
                            cmd3.Parameters.Add("@y", SqlDbType.VarChar).Value = Y;
                            cmd3.Parameters.Add("@z", SqlDbType.VarChar).Value = Z;
                            cmd3.Parameters.Add("@i", SqlDbType.Int).Value = i;
                            cmd3.ExecuteNonQuery();

                            MessageBox.Show("Success", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            textEmail.Text = null;
                            textPassword.Text = null;
                            textUsername.Text = null;
                        }
                    }
                    dr.Close();
                    cn.Close();
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textEmail.Text = null;
            textPassword.Text = null;
            textUsername.Text = null;
        }

        private void checkBoxPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPass.Checked)
            {
                textPassword.PasswordChar = '\0';
            } else
            {
                textPassword.PasswordChar = '*';
            }
        }
    }
}
