using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Mech_Report
{
    public class MechManager
    {
        private string inputfile;
        private string exception_msg;
        public MechManager(string inputfile)
        {
            this.inputfile = inputfile;
        }
        public void setInputfile(string inputfile)
        {
            this.inputfile = inputfile;
        }
        public string getLastException()
        {

            return this.exception_msg;
        }

        public List<MechData> readData()
        {
            List<MechData> data = new List<MechData>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = header_args => header_args.Header.ToLower(),
            };
            try
            {
                using (var reader = new StreamReader(this.inputfile))
                using (var csv = new CsvReader(reader, config))
                {
                    data = csv.GetRecords<MechData>().ToList();
                    //foreach (var item in data)
                    //{
                    //    // "RTF:N_1100 RMF:N_1400 RBF:N_104300"
                    //    item.deviation = new int[] { };
                    //    item.calibration_type = new string[] { };
                    //    item.direction = new string[] { };

                    //    if (string.IsNullOrEmpty(item._deviation)) continue;
                    //    string[] subs = item._deviation.Split(' ');
                    //    for (int i = 0; i < subs.Length; i++)
                    //    {
                    //        // "RBF:N_104300"
                    //        var parts = subs[i].Split('_');
                    //        item.deviation.Append(
                    //            Convert.ToInt32(parts[1])
                    //            );
                    //        item.direction.Append(parts[0].Split(':')[1]);
                    //        item.calibration_type.Append(parts[0].Split(':')[0].Substring(1));

                    //    }
                    //}
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show("Hello, world!", "My App");
                exception_msg = e.GetType().FullName;
                return null;
            }
            return data;
        }

    }
}
