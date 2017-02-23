using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuideXml
{
    public class TvScanStatus
    {
        public int ScanInProgress { get; set; }
        public int ScanPossible { get; set; }
        public string Source { get; set; }
        public string[] SourceList { get; set; }
        public int Progress { get; set; }
        public int Found { get; set; }
    }
}
