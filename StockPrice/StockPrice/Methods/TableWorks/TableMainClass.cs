using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Methods.TableWorks.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPrice.Settings;
using OfficeOpenXml;
using MySql.Data.MySqlClient;
using Dapper;

namespace StockPrice.Methods.TableWorks
{
    public class TableMainClass
    {
        public static void ReadTable (DatabaseTotalResults request, List<StockTable> responses_from_table)
        {
            string search_word = request.Request.Trim(); // get a search word
            long request_from = request.ChatID; // get a chat ID of request

            try
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);

                List<StockTable> stock_got = new(); // make a stock table list


                using FileStream fileStream = new(AppSettings.Current.DropBoxData.DropBoxAbsoluteTotalPath + request_from + "_stock.xlsx", FileMode.Open); // create a filestream
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // set a license
                ExcelPackage excel = new(fileStream); // make new ExcelPackage
                var workSheet = excel.Workbook.Worksheets.First(); // get first sheet on this table
                var got = workSheet.ConvertSheetToObjects<StockTable>(); // Convert a WorkSheet to our DTO object
                stock_got = got.ToList(); // convert to list
                fileStream.Close(); // Close filestream

                foreach (var row in stock_got) // Start searching equals 
                    if (row.Sku != null && row.Sku.Trim().ToUpper().Equals(search_word.ToUpper())) responses_from_table.Add(row); // if search word equals SKU col
                else // If search word not equeals SKU col
                    {
                        if (row.Replaces != null && row.Replaces.Trim().Contains(',')) // if replaces count > 1
                        {
                            var _spl_data = row.Replaces.ToUpper().Trim().Split(',').ToList(); // make split and convert to list
                            foreach(var got_replace in _spl_data) // check every replace
                            {
                                if (got_replace.Equals(search_word.ToUpper())) // if replace is equals search word
                                {
                                    responses_from_table.Add(row); // add row in responses list

                                    con.Open();
                                    string type = "Stock";
                                    string DataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(row));
                                    con.QueryFirstOrDefault<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{type}', '{DataEscaped}');");
                                    con.Close();
                                    break; // break from loop
                                }
                            }
                        }
                        else if (row.Replaces != null && row.Replaces.Trim().ToUpper().Equals(search_word.ToUpper())) responses_from_table.Add(row); // If replaces count is 1 and it is equals search word
                    }

               

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
