using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Kies.Common.DeviceServiceLib.DeviceDataService;
using Kies.Common.DeviceServiceLib.DeviceDataService.Data;
using DeviceHost.Pims;
using Kies.Interface;
namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            
            byte[] _key = Encoding.ASCII.GetBytes("epovviwlx,dirwq;sor0-fvksz,erwog");
            byte[] _iv = Encoding.ASCII.GetBytes("afie,crywlxoetka");
            byte[] _byteArray = new byte[0x110];

            FileStream stream = null;
            String tempFileName = "D:\\smeContent.xml";
            String path = "D:\\GitHub\\Sme_Decoder\\SMS001.sme";
            FileStream input = new FileStream("D:\\smeheader.xml", FileMode.OpenOrCreate);
            FileStream outFileStream = null;
            XmlSerializer serializer = new XmlSerializer(typeof(EntrySaveData));
            EntrySaveData data = null;


            UTF8Encoding encoding = new UTF8Encoding();
            
            try{
                stream = File.OpenRead(path);
                string str = String.Empty;

                while (stream.Read(_byteArray, 0, 0x110) == 0x110) {

                    byte[] buffer = Decrypt(_byteArray, _key, _iv);
                    input.Write(buffer, 0, buffer.Length);
                    str = str + encoding.GetString(buffer);
                    if (((buffer[0xff] == 0x20) && (buffer[0xfe] == 0x20)) && (str.IndexOf("</HeaderData>") != -1))
                    {
                        break;
                    }
                }
                outFileStream = new FileStream(tempFileName, FileMode.OpenOrCreate);

                int num = 0; byte[] bytes;
                    while ((num = stream.Read(_byteArray, 0, 0x110)) == 0x110){
                        bytes = Decrypt(_byteArray, _key, _iv);
                        outFileStream.Write(bytes, 0, bytes.Length);
                    }

                    if (num > 0){
                        bytes = Decrypt(_byteArray, _key, _iv);

                        for (int i = bytes.Length - 1; i >= 0; i--)
                        {
                            if (bytes[i] != 0)
                            {
                                outFileStream.Write(bytes, 0, i + 1);
                                break;
                            }
                        }
                    }
                    outFileStream.Flush();
                    outFileStream.Seek(0L, SeekOrigin.Begin);

                    XmlReader reader3 = new XmlTextReader(outFileStream);
                    data = (EntrySaveData)serializer.Deserialize(reader3);
                    if (data != null)
                    {
                        data.IsSupportSend = false;
                        if (data.SupportMMSVersion != "3.0")
                        {
                            data.MMSList.Clear();
                        }
                    }
                    reader3.Close();


                    using (TextWriter writer = new StreamWriter("D:\\MSG_LOAD_NO_CRYPTION.xml"))
                    {
                       serializer.Serialize(writer, data);
                        
                        writer.Close();
                    }
            }
            finally{

            }

        }


        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] buffer;
            using (Rijndael rijndael = Rijndael.Create())
            {
                using (ICryptoTransform transform = rijndael.CreateDecryptor(key, iv))
                {
                    buffer = Crypt(data, key, iv, transform);
                }
            }
            return buffer;
        }

        private static byte[] Crypt(byte[] data, byte[] key, byte[] iv, ICryptoTransform cryptor)
        {
            MemoryStream stream = new MemoryStream();
            using (Stream stream2 = new CryptoStream(stream, cryptor, CryptoStreamMode.Write))
            {
                stream2.Write(data, 0, data.Length);
            }
            return stream.ToArray();
        }

        
    }


}
