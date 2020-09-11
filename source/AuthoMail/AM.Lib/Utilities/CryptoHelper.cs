extern alias bc;

using AM.Lib.Properties;
using bc::Org.BouncyCastle.Bcpg;
using bc::Org.BouncyCastle.Bcpg.OpenPgp;
using bc::Org.BouncyCastle.Security;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AM.Lib
{
    public static class CryptoHelper
    {
        public static readonly string PublicKey;
        private static readonly string PrivateKey;

        static CryptoHelper()
        {
            //TODO: fix based on environment
            PublicKey = Resources.PGP_Public_Key_UAT;
            PrivateKey = Resources.PGP_Private_Key_UAT;
        }

        public static async Task DecryptFileAsync(string cipherFilePath, string passPhrase, string decryptedFilePath)
        {
            using (var inputStream = File.OpenRead(cipherFilePath))
            {
                using (Stream keyIn = await IOHelper.GetASCIIStreamAsync(PrivateKey))
                {
                    await DecryptPgpData(inputStream, keyIn, passPhrase, decryptedFilePath);                
                }
            }
        }

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        //public static string DecryptPgpData(string inputData)
        //{
        //    string output;
        //    using (Stream inputStream = IoHelper.GetStream(inputData))
        //    {
        //        using (Stream keyIn = File.OpenRead(PrivateKeyOnlyPath))
        //        {
        //            output = DecryptPgpData(inputStream, keyIn, Password).ToString();
        //        }
        //    }
        //    return output;
        //}

        public static async Task DecryptPgpData(Stream inputStream, Stream privateKeyStream, string passPhrase, string decryptedFilePath)
        {
            LiteralData output;

            PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            // find secret key
            PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

            PgpObject pgp = null;
            if (pgpFactory != null)
            {
                pgp = pgpFactory.NextPgpObject();
            }

            // the first object might be a PGP marker packet.
            PgpEncryptedDataList encryptedData = null;
            if (pgp is PgpEncryptedDataList)
            {
                encryptedData = (PgpEncryptedDataList)pgp;
            }
            else
            {
                encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
            }

            // decrypt
            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData pubKeyData = null;
            foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                if (privateKey != null)
                {
                    pubKeyData = pubKeyDataItem;
                    break;
                }
            }

            if (privateKey == null)
            {
                throw new ArgumentException("Secret key for message not found.");
            }

            PgpObjectFactory plainFact = null;
            using (Stream clear = pubKeyData.GetDataStream(privateKey))
            {
                plainFact = new PgpObjectFactory(clear);
            }

            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData compressedData = (PgpCompressedData)message;
                PgpObjectFactory pgpCompressedFactory = null;

                using (Stream compDataIn = compressedData.GetDataStream())
                {
                    pgpCompressedFactory = new PgpObjectFactory(compDataIn);
                }

                message = pgpCompressedFactory.NextPgpObject();
                if (message is PgpOnePassSignatureList)
                {
                    message = pgpCompressedFactory.NextPgpObject();
                }

                var literalData = (PgpLiteralData)message;
                using (var os = File.Open(decryptedFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    using (var @is = literalData.GetInputStream())
                    {
                        await @is.CopyToAsync(os);
                    }
                }
                //using (Stream unc = literalData.GetInputStream())
                //{
                //    output = IoHelper.GetString(unc);
                //}
                //output = new LiteralData((PgpLiteralData)message);
                
            }
            else if (message is PgpLiteralData)
            {
                //PgpLiteralData literalData = (PgpLiteralData)message;
                //using (Stream unc = literalData.GetInputStream())
                //{
                //    output = IoHelper.GetString(unc);
                //}

                //output = new LiteralData((PgpLiteralData)message);
                var literalData = (PgpLiteralData)message;
                using (var os = File.Open(decryptedFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    using (var @is = literalData.GetInputStream())
                    {
                        await @is.CopyToAsync(os);
                    }
                }
            }
            else if (message is PgpOnePassSignatureList)
            {
                throw new PgpException("Encrypted message contains a signed message - not literal data.");
            }
            else
            {
                throw new PgpException("Message is not a simple encrypted file - type unknown.");
            }

            //return output;
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        public static async Task EncryptPgpFile(string inputFile, string outputFile)
        {
            // use armor: yes, use integrity check? yes?
            using (var publicKeyStream = await IOHelper.GetASCIIStreamAsync(PublicKey))
            {
                EncryptPgpFile(inputFile, outputFile, publicKeyStream, true, true);
            }
        }

        public static void EncryptPgpFile(string inputFile, string outputFile, Stream publicKeyStream, bool armor, bool withIntegrityCheck)
        {

            PgpPublicKey pubKey = ReadPublicKey(publicKeyStream);

            using (MemoryStream outputBytes = new MemoryStream())
            {
                PgpCompressedDataGenerator dataCompressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                PgpUtilities.WriteFileToLiteralData(dataCompressor.Open(outputBytes), PgpLiteralData.Binary, new FileInfo(inputFile));

                dataCompressor.Close();
                PgpEncryptedDataGenerator dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());

                dataGenerator.AddMethod(pubKey);
                byte[] dataBytes = outputBytes.ToArray();

                using (Stream outputStream = File.Create(outputFile))
                {
                    if (armor)
                    {
                        using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(outputStream))
                        {
                            IOHelper.WriteStream(dataGenerator.Open(armoredStream, dataBytes.Length), ref dataBytes);
                        }
                    }
                    else
                    {
                        IOHelper.WriteStream(dataGenerator.Open(outputStream, dataBytes.Length), ref dataBytes);
                    }
                }
            }
        }
    }
}
