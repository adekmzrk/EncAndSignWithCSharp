using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class Decrypt : Form
    {
        public string username, sender2;
        private string enc, sign, fileExt;
        public byte[] dec;
        public Decrypt(string user)
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

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Files (*" + fileExt + ") | *" + fileExt;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, dec);
            }
            MessageBox.Show("Successfully Download!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Decrypt_Load(object sender, EventArgs e)
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

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select your Public Key (.PEM)";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBrowsePublic.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select your File to Encrypted File (.*)";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBrowseFileEnc.Text = openFileDialog1.FileName;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "" || textBrowsePublic.Text == "" || textBrowseFileEnc.Text == "" || textPassword.Text == "")
            {
                MessageBox.Show("There's field empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                //hashing file
                string hash = KriptoKu.HashMD5(textBrowseFileEnc.Text);
                fileExt = Path.GetExtension(textBrowseFileEnc.Text);

                //cek database 
                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    string query = "SELECT sender, sign FROM [packet] WHERE hash=" + "'" + hash + "'";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            sender2 = dr.GetString(0);
                            sign = dr.GetString(1);
                        }
                        dr.Close();
                        cn.Close();

                        if(sender2 == comboBox1.Text)
                        {
                            // verify signature
                            try
                            {
                                byte[] filecontent = File.ReadAllBytes(textBrowseFileEnc.Text);
                                enc = Convert.ToBase64String(filecontent);

                                bool Result = KriptoKu.VerifySignature(textBrowsePublic.Text, enc, sign);

                                //string Output = Result ? "The signature matches. Verification was successfull." : "The signature does NOT Matches. Verification failed.";
                                if(Result == false)
                                {
                                    textBox1.Visible = true;
                                    textBox1.BackColor = Color.Red;
                                    textBox1.Text = "NOT VERIFIED!";
                                    textBox1.ForeColor = Color.White;
                                } else if (Result == true)
                                {
                                    textBox1.Visible = true;
                                    textBox1.BackColor = Color.Green;
                                    textBox1.Text = "VERIFIED!";
                                    textBox1.ForeColor = Color.White;
                                }

                                // decrypt proses
                                try
                                {
                                    dec = KriptoKu.Decrypt(textBrowseFileEnc.Text, textPassword.Text);
                                    if (Result == false)
                                    {
                                        MessageBox.Show("Decryption Success but Sign Not Verified", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else if (Result == true)
                                    {
                                        MessageBox.Show("Decryption Success and Sign Verified", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }

                                    

                                } catch (Exception ex)
                                {
                                    MessageBox.Show("Error while Decrypt " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            } catch(Exception ex)
                            {
                                MessageBox.Show("Error while Verify Signature " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        } else
                        {
                            MessageBox.Show("You choose the wrong Sender. Please select the right Sender!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } else
                    {
                        MessageBox.Show("There's no Encrypted FIle Data on our Database. Input the right File!", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }     
            }
            buttonDownload.Enabled = true;
        }
    }
}
