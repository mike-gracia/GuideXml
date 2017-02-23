using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace GuideXml
{
    class Program
    {
        static int minChannelNumber = 500;
        static int maxChannelNumber = 1000;
        static string sURL = "http://tuner.";

        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to scan for channels?(Y/N)");

            if (Console.ReadLine().ToUpper().FirstOrDefault() == 'Y')
                ScanForChannels();

            List<TvListing> listings = GetInput();


            EditXmlDoc(listings);

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        private static void ScanForChannels()
        {
            string startScanUrl = "/lineup.post?scan=start";


            WebStreamReader(startScanUrl).Close();
          

        }


        private static List<TvListing> GetInput()
        {
            string lineupUrl = "/lineup.json";

           

            while (IsTunerScanning())
            {
                System.Threading.Thread.Sleep(3000);
            }

            StreamReader webStreamReader = WebStreamReader(lineupUrl);

            var a = JsonConvert.DeserializeObject<List<TvListing>>(webStreamReader.ReadLine());

            foreach (TvListing channel in a)
            {
                Console.WriteLine("Found channel {0} : {1}", channel.GuideNumber, channel.GuideName);
            }

            webStreamReader.Close();
            return a;

        }

        private static bool IsTunerScanning()
        {
            string statusUrl = "/lineup_status.json";


            StreamReader webStreamReader = WebStreamReader(statusUrl);


            Console.WriteLine("Requesting channels from {0}", sURL);


            var status = JsonConvert.DeserializeObject<TvScanStatus>(webStreamReader.ReadLine());

            Console.WriteLine("Tuner status : {0}, Percent : {1}, Channels Found: {2}", status.ScanInProgress, status.Progress, status.Found);

            webStreamReader.Close();

            return Convert.ToBoolean(status.ScanInProgress);
        }

        private static StreamReader WebStreamReader(String target)
        {
            WebRequest wrGETURL;
            Stream objStream;
            StreamReader objReader;

            wrGETURL = WebRequest.Create(sURL + target);
            objStream = wrGETURL.GetResponse().GetResponseStream();
            objReader = new StreamReader(objStream);

            return objReader;

        }

        private static void EditXmlDoc(List<TvListing> _listings)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("input.xml");
            IEnumerator ie = doc.SelectNodes("/Config/Channel").GetEnumerator();
            doc.Save("input.xml");         //resave so xml is formatted the same as .new.xml

            XmlNode node;
            int changeCount = 0;
            while (ie.MoveNext())
            {
                node = (ie.Current as XmlNode);
                node.Attributes["IsVisible"].Value = "false";
                int nodeChannelInt = Convert.ToInt32(node.Attributes["Number"].Value);

                if (nodeChannelInt < maxChannelNumber  && nodeChannelInt > minChannelNumber)
                {
                    foreach (TvListing l in _listings)
                    {
                        if ((ie.Current as XmlNode).Attributes["Number"].Value == l.GuideNumber)
                        {
                            node.Attributes["IsVisible"].Value = "true";
                            //Debug.WriteLine(node.Attributes["Number"].Value + " == " + l.GuideNumber);
                            Console.WriteLine("{0} visible - {1}", l.GuideNumber, l.GuideName);
                            changeCount++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("{0} channels changed", changeCount);
            doc.Save("output.new.xml");
        }












        
     


    }
}
