using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using JsonConfig;


namespace GuideXml
{
    class Program
    {
        static int minChannelNumber = Config.Default.minChannelNumber;
        static int maxChannelNumber = Config.Default.maxChannelNumber;
        static string tunerUrl = Config.Default.tunerUrl;

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


            Console.WriteLine("Requesting channels from {0}", tunerUrl);


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

            wrGETURL = WebRequest.Create(tunerUrl + target);
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
            int channelAnalizedCount = 0;
            while (ie.MoveNext())
            {
                node = (ie.Current as XmlNode);
                
                int nodeChannelInt = Convert.ToInt32(node.Attributes["Number"].Value);

                if (nodeChannelInt < maxChannelNumber  && nodeChannelInt > minChannelNumber)
                {
                    foreach (TvListing l in _listings)
                    {
                        if ((ie.Current as XmlNode).Attributes["Number"].Value == l.GuideNumber)
                        {
                            if (node.Attributes["IsVisible"].Value != "true")
                                Console.WriteLine("{0} ADDED - {1}", nodeChannelInt, l.GuideName);
                            node.Attributes["IsVisible"].Value = "true";
                            //Debug.WriteLine(node.Attributes["Number"].Value + " == " + l.GuideNumber);
                            //Console.WriteLine("{0} visible - {1}", l.GuideNumber, l.GuideName);
                            channelAnalizedCount++;
                            break;
                        }

                    }
                }
                else
                {
                    if (node.Attributes["IsVisible"].Value != "false")
                        Console.WriteLine("{0} REMOVED OUT OF BOUNDS", nodeChannelInt);
                    node.Attributes["IsVisible"].Value = "false";
                    
                }
            }
            Console.WriteLine("Between {0} - {1} | {2} channels analized", minChannelNumber, maxChannelNumber, channelAnalizedCount);
            doc.Save("output.new.xml");
        }












        
     


    }
}
