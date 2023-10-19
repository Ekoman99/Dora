using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora
{
    internal class DataTransformation : MainWindow
    {
        private int[] order;
        private DateTime[] dateTimes;
        private double?[] latitudes;
        private double?[] longitudes;
        private int?[] cellIDs;
        private int?[] pcis;
        private string[] techs;
        private string[] bands;
        private int?[] rsrps;
        private int?[] rsrqs;
        private int?[] rssis;
        private int?[] sinrs;
        private int?[] cqis;
        private int?[] pings;
        private float?[] downlinks;

        //4g
        private int[] order4g;
        private DateTime[] dateTimes4g;
        private double?[] latitudes4g;
        private double?[] longitudes4g;
        private int?[] cellIDs4g;
        private int?[] pcis4g;
        private string[] techs4g;
        private string[] bands4g;
        private int?[] rsrps4g;
        private int?[] rsrqs4g;
        private int?[] rssis4g;
        private int?[] sinrs4g;
        private int?[] cqis4g;
        private int?[] pings4g;
        private float?[] downlinks4g;

        //5g
        private int[] order5g;
        private DateTime[] dateTimes5g;
        private double?[] latitudes5g;
        private double?[] longitudes5g;
        private int?[] cellIDs5g;
        private int?[] pcis5g;
        private string[] techs5g;
        private string[] bands5g;
        private int?[] rsrps5g;
        private int?[] rsrqs5g;
        private int?[] rssis5g;
        private int?[] sinrs5g;
        private int?[] cqis5g;
        private int?[] pings5g;
        private float?[] downlinks5g;

        public void ArrayDataSet(List<BaseCsvData> inputList)
        {
            //base data
            order = new int[inputList.Count];
            dateTimes = new DateTime[inputList.Count];
            latitudes = new double?[inputList.Count];
            longitudes = new double?[inputList.Count];
            cellIDs = new int?[inputList.Count];
            pcis = new int?[inputList.Count];
            techs = new string[inputList.Count];
            bands = new string[inputList.Count];
            rsrps = new int?[inputList.Count];
            rsrqs = new int?[inputList.Count];
            rssis = new int?[inputList.Count];
            sinrs = new int?[inputList.Count];
            cqis = new int?[inputList.Count];
            pings = new int?[inputList.Count];
            downlinks = new float?[inputList.Count];

            //4g data
            order = new int[inputList.Count];
            dateTimes = new DateTime[inputList.Count];
            latitudes = new double?[inputList.Count];
            longitudes = new double?[inputList.Count];
            cellIDs = new int?[inputList.Count];
            pcis = new int?[inputList.Count];
            techs = new string[inputList.Count];
            bands = new string[inputList.Count];
            rsrps = new int?[inputList.Count];
            rsrqs = new int?[inputList.Count];
            rssis = new int?[inputList.Count];
            sinrs = new int?[inputList.Count];
            cqis = new int?[inputList.Count];
            pings = new int?[inputList.Count];
            downlinks = new float?[inputList.Count];

            //5g data
            order = new int[inputList.Count];
            dateTimes = new DateTime[inputList.Count];
            latitudes = new double?[inputList.Count];
            longitudes = new double?[inputList.Count];
            cellIDs = new int?[inputList.Count];
            pcis = new int?[inputList.Count];
            techs = new string[inputList.Count];
            bands = new string[inputList.Count];
            rsrps = new int?[inputList.Count];
            rsrqs = new int?[inputList.Count];
            rssis = new int?[inputList.Count];
            sinrs = new int?[inputList.Count];
            cqis = new int?[inputList.Count];
            pings = new int?[inputList.Count];
            downlinks = new float?[inputList.Count];

            for (int i = 0; i < inputList.Count; i++)
            { 
                order[i] = (i + 1);
                dateTimes[i] = inputList[i].Time;
                latitudes[i] = inputList[i].Latitude;
                longitudes[i] = inputList[i].Longitude;
                cellIDs[i] = inputList[i].CellID;
                pcis[i] = inputList[i].PCI;
                techs[i] = inputList[i].Tech;
                bands[i] = inputList[i].Band;
                rsrps[i] = inputList[i].RSRP;
                rsrqs[i] = inputList[i].RSRQ;
                rssis[i] = inputList[i].RSSI;
                sinrs[i] = inputList[i].SINR;
                cqis[i] = inputList[i].CQI;
                pings[i] = inputList[i].Ping;
                downlinks[i] = inputList[i].Downlink;

                if (inputList[i].Tech == "EN-DC")
                {
                    order5g[i] = (i + 1);
                    dateTimes5g[i] = inputList[i].Time;
                    latitudes5g[i] = inputList[i].Latitude;
                    longitudes5g[i] = inputList[i].Longitude;
                    cellIDs5g[i] = inputList[i].CellID;
                    pcis5g[i] = inputList[i].PCI;
                    techs5g[i] = inputList[i].Tech;
                    bands5g[i] = inputList[i].Band;
                    rsrps5g[i] = inputList[i].RSRP;
                    rsrqs5g[i] = inputList[i].RSRQ;
                    rssis5g[i] = inputList[i].RSSI;
                    sinrs5g[i] = inputList[i].SINR;
                    cqis5g[i] = inputList[i].CQI;
                    pings5g[i] = inputList[i].Ping;
                    downlinks5g[i] = inputList[i].Downlink;
                }
                else
                {
                    order4g[i] = (i + 1);
                    dateTimes4g[i] = inputList[i].Time;
                    latitudes4g[i] = inputList[i].Latitude;
                    longitudes4g[i] = inputList[i].Longitude;
                    cellIDs4g[i] = inputList[i].CellID;
                    pcis4g[i] = inputList[i].PCI;
                    techs4g[i] = inputList[i].Tech;
                    bands4g[i] = inputList[i].Band;
                    rsrps4g[i] = inputList[i].RSRP;
                    rsrqs4g[i] = inputList[i].RSRQ;
                    rssis4g[i] = inputList[i].RSSI;
                    sinrs4g[i] = inputList[i].SINR;
                    cqis4g[i] = inputList[i].CQI;
                    pings4g[i] = inputList[i].Ping;
                    downlinks4g[i] = inputList[i].Downlink;
                }
            }
        }

        public int?[] ReturnRSRP4g()
        {
            return rsrps4g;
        }
    }
}
