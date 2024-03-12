using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using ChoETL;
using Aspose.Cells;
using Aspose.Cells.Utility;
using StockPrice.ResponseClasses;

namespace StockPrice.Methods
{
    internal class CSVMaker
    {
        private static DataTable JsonStringToTable(string jsonContent)
        {
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(jsonContent);
            return dt;
        }

       

        public static string JsonToCSVJson(string jsonContent)
        {
            StringBuilder sb = new StringBuilder();
            using (var p = ChoJSONReader.LoadText(jsonContent))
            {
                using (var w = new ChoCSVWriter(sb))
                    w.Write(p);
            }
            return sb.ToString();
        }

        public static string AsposeJsonToCsv (string _jdata)
        {
            var workbook = new Workbook();
            
            // Get Cells
            Cells cells = workbook.Worksheets[0].Cells;

            // Set JsonLayoutOptions
            JsonLayoutOptions importOptions = new JsonLayoutOptions();
            importOptions.ConvertNumericOrDate = true;
            importOptions.ArrayAsTable = true;
            importOptions.IgnoreArrayTitle = true;
            importOptions.IgnoreObjectTitle = true;
            JsonUtility.ImportData(_jdata, cells, 0, 0, importOptions);

            Guid _guid = Guid.NewGuid();
            TxtSaveOptions options = new(SaveFormat.Csv)
            {
                Separator = Convert.ToChar(";")
            };

            workbook.Save($@"{_guid}.csv", options);

            return $@"{_guid}.csv";

        }

    }
}
