using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System.Diagnostics;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Engines;
using System.Drawing;
using System.Windows.Forms;

namespace EncAndSignWithCSharp
{
    internal class KriptoKu
    {
        
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        public static string ToSHA256(string s)
        {
            var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string HashMD5(string path)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var s = md5.ComputeHash(stream);
                    return Convert.ToBase64String(s);
                }
            }
        }


        public static string calculateX(string username, string pass, int i)
        {
            return (ToSHA256(ToSHA256(username + pass + i.ToString())));
        }

        public static X509Certificate2 CreateCertificate(string subjectName, string issuer, int ValidMonths, out AsymmetricCipherKeyPair KeyPair, int keyStrength = 2048)
        {
            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            var serialNumber = BigIntegers.CreateRandomInRange(Org.BouncyCastle.Math.BigInteger.One, Org.BouncyCastle.Math.BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            var subjectDN = new X509Name(subjectName);
            var issuerDN = new X509Name(issuer);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddMonths(ValidMonths);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.AddExtension(X509Extensions.KeyUsage.Id, true, new KeyUsage(KeyUsage.KeyEncipherment));

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            var issuerKeyPair = subjectKeyPair;
            KeyPair = subjectKeyPair;

            // Selfsign certificate
            certificateGenerator.SetSignatureAlgorithm("SHA256WithRSA");
            var certificate = certificateGenerator.Generate(issuerKeyPair.Private, random);
            certificate.CheckValidity();
            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            return x509;
        }

        
        public static byte[] Encrypt(string path, string pass)
        {
            byte[] abc = new byte[256];
            for(int i = 0; i < 256; i++)
            {
                abc[i] = Convert.ToByte(i);
            }
            byte[,] table = new byte[256,256];
            for(int i = 0; i < 256; i++)
            {
                for(int j = 0; j < 256; j++)
                {
                    table[i, j] = abc[(i+j)%256];
                }
            }

            byte[] filecontent = File.ReadAllBytes(path);
            byte[] passwordTmp = Encoding.UTF8.GetBytes(pass);
            byte[] keys = new byte[filecontent.Length];
            byte[] result = new byte[filecontent.Length];

            for (int i = 0; i < filecontent.Length; i++)
            {
                keys[i] = passwordTmp[i % passwordTmp.Length];
            }

            for (int i = 0; i < filecontent.Length; i++)
            {
                byte value = filecontent[i];
                byte key = keys[i];
                int valueIndex = -1, keyIndex = -1;
                for(int j = 0; j < 256; j++)
                {
                    if(abc[j] == value)
                    {
                        valueIndex = j;
                        break;
                    }
                }
                for(int j = 0; j < 256; j++)
                {
                    if(abc[j] == key)
                    {
                        keyIndex = j;
                        break;
                    }
                }
                result[i] = table[keyIndex,valueIndex];               
            }
            return result;
        }

        public static byte[] Decrypt(string path, string pass)
        {
            byte[] abc = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                abc[i] = Convert.ToByte(i);
            }
            byte[,] table = new byte[256, 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    table[i, j] = abc[(i + j) % 256];
                }
            }

            byte[] filecontent = File.ReadAllBytes(path);
            byte[] passwordTmp = Encoding.UTF8.GetBytes(pass);
            byte[] keys = new byte[filecontent.Length];
            byte[] result = new byte[filecontent.Length];

            for (int i = 0; i < filecontent.Length; i++)
            {
                keys[i] = passwordTmp[i % passwordTmp.Length];
            }

            for (int i = 0; i < filecontent.Length; i++)
            {
                byte value = filecontent[i];
                byte key = keys[i];
                int valueIndex = -1, keyIndex = -1;
                for (int j = 0; j < 256; j++)
                {
                    if (abc[j] == key)
                    {
                        keyIndex = j;
                        break;
                    }
                }
                for (int j = 0; j < 256; j++)
                {
                    if (table[keyIndex, j] == value)
                    {
                        valueIndex = j;
                        break;
                    }
                }
                result[i] = abc[valueIndex];
            }
            return result;
        }

        public static string RSASigntWithPEMPrivateKey(string PrivateKeyPEMFileName, string Text)
        {
            byte[] BytesToSign = Encoding.UTF8.GetBytes(Text);
            AsymmetricCipherKeyPair KeyPair = null;
            TextReader reader = File.OpenText(PrivateKeyPEMFileName);
            KeyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

            byte[] Signature = RSASigntWithPrivateKey(KeyPair, BytesToSign);
            string Result = Convert.ToBase64String(Signature);

            return Result;
        }

        public static byte[] RSASigntWithPrivateKey(AsymmetricCipherKeyPair KeyPair, byte[] BytesToSign)
        {
            // compute the SHA 256 hash from the bytes to sign received
            Sha256Digest sha256Digest = new Sha256Digest();
            byte[] TheHash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.BlockUpdate(BytesToSign, 0, BytesToSign.Length);
            sha256Digest.DoFinal(TheHash, 0);

            PssSigner Signer = new PssSigner(new RsaEngine(), new Sha256Digest(), sha256Digest.GetDigestSize());
            Signer.Init(true, KeyPair.Private);
            Signer.BlockUpdate(TheHash, 0, TheHash.Length);
            byte[] Signature = Signer.GenerateSignature();

            return Signature;
        }

        public static bool VerifySignature(string PublicKeyPEMFileName, string Text, string ExpectedSignature)
        {
            byte[] BytesToSign = Encoding.UTF8.GetBytes(Text);
            byte[] ExpectedSignatureBytes = Convert.FromBase64String(ExpectedSignature);

            TextReader reader = File.OpenText(PublicKeyPEMFileName);
            AsymmetricKeyParameter KeyPair = (AsymmetricKeyParameter)new PemReader(reader).ReadObject();

            Sha256Digest sha256Digest = new Sha256Digest();
            byte[] TheHash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.BlockUpdate(BytesToSign, 0, BytesToSign.Length);
            sha256Digest.DoFinal(TheHash, 0);

            PssSigner Signer = new PssSigner(new RsaEngine(), new Sha256Digest(), sha256Digest.GetDigestSize());
            Signer.Init(false, KeyPair);
            Signer.BlockUpdate(TheHash, 0, TheHash.Length);
            return Signer.VerifySignature(ExpectedSignatureBytes);
        }

        
    }
}
