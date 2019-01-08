using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace PoC_AmazonRekognition
{
    class Program
    {
        private static bool foundSerial = false;
        private static string serialNumber = "";
        private static Stopwatch _stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            string fullPath = GetDirectory();
            Console.WriteLine($"Please select image 1-9");
            char userInput = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (char.IsDigit(userInput))
            {
                string path = fullPath + "\\images\\" + userInput + ".jpg";
                if (File.Exists(path))
                {
                    var options = new CredentialProfileOptions()
                    {
                        AccessKey = "<INSERT ACCESS KEY",
                        SecretKey = "<INSERT SECRET KEY"
                    };

                    var profile = new CredentialProfile("test", options) { Region = RegionEndpoint.USEast2 };
                    var netSdkFile = new NetSDKCredentialsFile();
                    netSdkFile.RegisterProfile(profile);


                    AmazonRekognitionClient rekoClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);
                    MemoryStream mStream;

                    using (System.Drawing.Image image = System.Drawing.Image.FromFile($"{fullPath}\\images\\9.jpg"))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat);
                            mStream = m;
                        }
                    }

                    DetectTextRequest detectTextRequest = new DetectTextRequest()
                    {
                        Image = new Image()
                        {
                            Bytes = mStream
                        }
                    };

                    try
                    {
                        _stopwatch.Start();
                        DetectTextResponse detectTextResponse = rekoClient.DetectText(detectTextRequest);
                        foreach (TextDetection text in detectTextResponse.TextDetections)
                        {
                            CheckFoundSerial(text.DetectedText);
                        }

                        Console.WriteLine($"Query time {_stopwatch.ElapsedMilliseconds}ms");
                        if (foundSerial)
                        {
                            Console.WriteLine($"Serial number: {serialNumber}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            Console.ReadKey();
        }

        private static void CheckFoundSerial(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Length == 6)
                {
                    if ((text[0] == 'w' || text[0] == 'W'))
                    {
                        string tmpString = text.Substring(1, 5);
                        if (tmpString.All(char.IsDigit))
                        {
                            foundSerial = true;
                            serialNumber = text;
                        }
                    }
                }
            }
        }

        private static string GetDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
