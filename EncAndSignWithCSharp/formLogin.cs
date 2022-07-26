using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace EncAndSignWithCSharp
{
    public partial class formLogin : Form
    {
        private int i = 0;
        private string Y = null;
        private string Z = null;

        public formLogin()
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
            this.Hide();

            var f2 = new formRegister();
            f2.Show();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textPassword.Text = null;
            textUsername.Text = null;
        }

        private void checkBoxPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPass.Checked)
            {
                textPassword.PasswordChar = '\0';
            }
            else
            {
                textPassword.PasswordChar = '*';
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if(textUsername.Text == "" || textPassword.Text == "")
            {
                MessageBox.Show("There's field empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    string query = "SELECT Y, Z, i FROM [user] WHERE username=" + "'" + textUsername.Text.ToLower() + "'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            Y = dr.GetString(0);
                            Z = dr.GetString(1);
                            i = dr.GetInt32(2);
                        }

                        string XX = KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i);
                        string YY = KriptoKu.ToSHA256(XX);
                        string ZZ = KriptoKu.ToSHA256(XX + KriptoKu.ToSHA256(KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i + 1)));

                        if(Y == YY && Z == ZZ)
                        {
                            i = i + 1;
                            string XXX = KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i);
                            string YYY = KriptoKu.ToSHA256(XXX);
                            string ZZZ = KriptoKu.ToSHA256(XXX + KriptoKu.ToSHA256(KriptoKu.calculateX(textUsername.Text.ToLower(), textPassword.Text.ToLower(), i + 1)));

                            dr.Close();
                            query = "Update [user] SET Y=(@yy), Z=(@zz), i=(@ii) WHERE username=" + "'" + textUsername.Text.ToLower() + "'";
                            SqlCommand cmd3 = new SqlCommand(query, cn);
                            cmd3.Parameters.Add("@yy", SqlDbType.VarChar).Value = YYY;
                            cmd3.Parameters.Add("@zz", SqlDbType.VarChar).Value = ZZZ;
                            cmd3.Parameters.Add("@ii", SqlDbType.Int).Value = i;
                            cmd3.ExecuteNonQuery();

                            MessageBox.Show("Success", "Login Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Hide();

                            Dashboard dash = new Dashboard(textUsername.Text.ToLower());                           
                            dash.Show();                                                          
                        } else
                        {
                            MessageBox.Show("Wrong Password. Try again!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                    } else
                    {
                        MessageBox.Show("No User Specified, Register First!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    dr.Close();
                    cn.Close();
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            this.Hide();

            var f2 = new formForgotPass();
            f2.Show();
        }
    }
}
