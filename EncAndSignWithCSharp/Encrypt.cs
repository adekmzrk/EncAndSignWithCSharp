using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class Encrypt : Form
    {
        public string username;
        private string enc, sign, fileExt;
        public byte[] result;
        public Encrypt(string user)
        {
            InitializeComponent();
            username = user;
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.AppSettings.Get("database"));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();

            Dashboard f3 = new Dashboard(username);
            f3.Show();
        }

        private void Encrypt_Load(object sender, EventArgs e)
        {
            using (SqlConnection cn = GetConnection())
            {
                cn.Open();
                string query = "SELECT username FROM [user]";
                SqlCommand cmd = new SqlCommand(query, cn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    comboBox1.Items.Add(dr.GetString(0));
                }            
                dr.Close();
                cn.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select your Private Key (.PEM)";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBrowsePrivate.Text = openFileDialog1.FileName;
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Files (*" + fileExt + ") | *" + fileExt;
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, result);
            }

            string hash = KriptoKu.HashMD5(saveFileDialog1.FileName);

            using (SqlConnection cn = GetConnection())
            {
                cn.Open();
                string query = @"INSERT INTO [packet](sender, receiver, hash, sign) VALUES(@a, @b, @c, @d)";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.Add("@a", SqlDbType.VarChar).Value = username;
                cmd.Parameters.Add("@b", SqlDbType.VarChar).Value = comboBox1.Text;
                cmd.Parameters.Add("@c", SqlDbType.VarChar).Value = hash;
                cmd.Parameters.Add("@d", SqlDbType.VarChar).Value = sign;
                cmd.ExecuteNonQuery();

                cn.Close();

                MessageBox.Show("Successfully Download and Saved to Database", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

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

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select your File to Encrypt and Sign (.*)";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBrowseFile.Text = openFileDialog1.FileName;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            
            if(comboBox1.Text == "" || textBrowsePrivate.Text == "" || textBrowseFile.Text == "" || textPassword.Text == "")
            {
                MessageBox.Show("There's field empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                try
                {
                    // enkripsi plaintext
                    result = KriptoKu.Encrypt(textBrowseFile.Text, textPassword.Text);
                    enc = Convert.ToBase64String(result);
                    fileExt = Path.GetExtension(textBrowseFile.Text);

                    try
                    {
                        // signing 
                        sign = KriptoKu.RSASigntWithPEMPrivateKey(textBrowsePrivate.Text, enc);

                        MessageBox.Show("Encryption and Signing Success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        

                    } catch (Exception ex)
                    {
                        MessageBox.Show("Error while Signing File" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                } catch (Exception ex)
                {
                    MessageBox.Show("Error while Encrypt File " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                
            }
            buttonDownload.Enabled = true;
        }
    }
}
