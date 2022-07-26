using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    public partial class formGenCert : Form
    {
        public string username;
        public formGenCert(string user)
        {
            InitializeComponent();
            (new EncAndSignWithCSharp.DropShadow()).ApplyShadows(this);
            username = user;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.AppSettings.Get("database"));
        }

        private void label6_Click(object sender, EventArgs e)
        {
            this.Hide();

            Dashboard f2 = new Dashboard(username);
            f2.Show();
            
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if(textFullName.Text == "" || textBrowse.Text == "")
            {
                MessageBox.Show("There's field empty", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                try
                {
                    AsymmetricCipherKeyPair CertificateKey;

                    //let us first generate the root certificate
                    X509Certificate2 X509RootCert = KriptoKu.CreateCertificate("CN=" + textFullName.Text, "C=Indonesia, ST=JawaTengah, L=Bogor, O=ADEKCorp", 12, out CertificateKey);

                    string PublicPEMFile = textBrowse.Text + "\\" + username + "-public.pem";
                    string PrivatePEMFile = textBrowse.Text + "\\" + username + "-private.pem";

                    //now let us also create the PEM file as well in case we need it
                    using (TextWriter textWriter = new StreamWriter(PublicPEMFile, false))
                    {
                        PemWriter pemWriter = new PemWriter(textWriter);
                        pemWriter.WriteObject(CertificateKey.Public);
                        pemWriter.Writer.Flush();
                    }

                    TextReader reader = File.OpenText(PublicPEMFile);

                    //now let us also create the PEM file as well in case we need it
                    using (TextWriter textWriter = new StreamWriter(PrivatePEMFile, false))
                    {
                        PemWriter pemWriter = new PemWriter(textWriter);
                        pemWriter.WriteObject(CertificateKey.Private);
                        pemWriter.Writer.Flush();
                    }                    

                    using (SqlConnection cn = GetConnection())
                    {
                        cn.Open();
                        string query = "SELECT * FROM [pubkey] WHERE username=" + "'" + username + "'";
                        SqlCommand cmd = new SqlCommand(query, cn);
                        SqlDataReader dr = cmd.ExecuteReader();

                        if (dr.HasRows)
                        {
                            MessageBox.Show("Certificate already exist!", "Generate Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            dr.Close();
                            query = @"INSERT INTO [pubkey](username, pubkey) VALUES(@username, @pubkey)";
                            SqlCommand cmd3 = new SqlCommand(query, cn);
                            cmd3.Parameters.Add("@username", SqlDbType.VarChar).Value = username;
                            cmd3.Parameters.Add("@pubkey", SqlDbType.VarChar).Value = reader.ReadToEnd();
                            cmd3.ExecuteNonQuery();

                            MessageBox.Show("The Certificates have been succcessfully generated and Public Key Saved to Database", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Hide();

                            Dashboard f2 = new Dashboard(username);
                            f2.Show();
                        }
                        dr.Close();
                        cn.Close();
                    }    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while generating certificates. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();
            FolderBrowser.Description = "Select the folder to store the certificates";
            DialogResult result = FolderBrowser.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textBrowse.Text = FolderBrowser.SelectedPath;
            }
        }
    }
}
