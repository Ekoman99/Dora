using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora
{
    public class BaseCsvData
    {        
        [Name("Time [hh:mm:ss]")]
        public DateTime Time { get; set; }

        [Name(" Latitude")]
        public double Latitude { get; set; }

        [Name(" Longitude")]
        public double Longitude { get; set; }

        [Name(" CellID")]
        public int CellID { get; set; }

        [Name(" PCI")]
        public int PCI { get; set; }

        [Name(" Packet Technology")]
        public string Tech { get; set; }

        [Name(" Band")]
        public string Band { get; set; }

        [Name(" RSRP [dBm]")]
        public int? RSRP { get; set; }

        [Name(" RSRQ [dB]")]
        public int? RSRQ { get; set; }

        [Name(" RSSI [dBm]")]
        public int? RSSI { get; set; }

        [Name(" SINR [dB]")]
        public int? SINR { get; set; }

        [Name(" CQI")]
        public int? CQI { get; set; }

        [Name(" Ping [ms]")]
        public int? Ping { get; set; }

        [Name(" Downlink [MB/s]")]
        public float? Downlink { get; set; }
        
    }

}
