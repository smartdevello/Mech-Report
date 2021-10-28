using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mech_Report
{
    public class MechData
    {
        [Name("ID")]
        public int id { get; set; }

        [Name("Folder")]
        public string folder { get; set; }

        [Name("Name")]
        public string name { get; set; }

        [Name("Image ID")]
        public string image_id { get; set; }

        [Name("Deviation(%)")]
        public string _deviation { get; set; }

        [Name("Result")]
        public string result { get; set; }

        [Name("Time")]
        public string _time { get; set; }

        public int[] deviation;
        public string[] direction;
        public string[] calibration_type;
        public bool flaged = false;
    }
}
