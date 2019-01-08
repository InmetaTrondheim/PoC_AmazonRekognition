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
        private static bool again = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Press 0 for image processing using camera or 1-9 for stored images");

            while (again)
            {
                foundSerial = false;
                serialNumber = string.Empty;
                PresentMenu();
            }
        }

        private static void PresentMenu()
        {
            char userInput = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (char.IsDigit(userInput))
            {
                if (userInput == '0')
                {
                    CaptureImage.FindCapturingDevice();
                }
                else
                {
                    string path = GetDirectory() + "\\images\\" + userInput + ".jpg";
                    DoImageRecognition(path);
                }
            }
        }

        public static void DoImageRecognition(string path)
        {
            if (File.Exists(path))
            {
                var options = new CredentialProfileOptions()
                {
                    AccessKey = "",
                    SecretKey = ""
                };

                var profile = new CredentialProfile("test", options) { Region = RegionEndpoint.USEast2 };
                var netSdkFile = new NetSDKCredentialsFile();
                netSdkFile.RegisterProfile(profile);


                AmazonRekognitionClient rekoClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);
                MemoryStream mStream;

                using (System.Drawing.Image image = System.Drawing.Image.FromFile($"{path}"))
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
                    else
                    {
                        Console.WriteLine("Could not find any serial number");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
               
            _stopwatch.Reset();
            Console.WriteLine();
            Console.WriteLine("Press enter to show menu");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Press 0 for image processing using camera or 1-9 for stored images");

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
