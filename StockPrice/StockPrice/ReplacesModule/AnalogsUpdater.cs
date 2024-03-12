using StockPrice.ReplacesModule;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using StockPrice.Settings;
using Dapper;
using StockPrice.DatabaseClasses;
using OfficeOpenXml;
using StockPrice.Methods.TableWorks.Classes;
using StockPrice.Methods.TableWorks;
using MySqlX.XDevAPI.Relational;
using System;

namespace StockPrice.ReplacesModule
{
    public class AnalogsUpdater
    {
        public static bool WasEditModifier { get; set; } = false;

        /// <summary>
        /// The main parallel method for checking stock tables.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static void MainChecker(CancellationToken cancellationToken)
        {
            Console.WriteLine("Table Replaces Checker started");
            while (!cancellationToken.IsCancellationRequested) 
            {
                var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
                using var con = new MySqlConnection(cs);

                con.Open();
                var userdata = con.Query<DatabaseUserData>("SELECT * FROM `userdata` WHERE `isHaveStockTable`='1';"); //Get a data
                con.Close();

                foreach (var user in userdata)
                {
                    long user_id = user.UserId;

                    // Start checking last edited time
                    double min_minutes_last_edited = 3;
                    DateTime dt_now = DateTime.UtcNow;

                    FileInfo stock_table_fi = new FileInfo(AppSettings.Current.DropBoxData.DropBoxAbsoluteTotalPath + user_id + "_stock.xlsx");
                    DateTime dt_last_edited = stock_table_fi.LastWriteTimeUtc;
                    if ((dt_now - dt_last_edited).TotalMinutes < min_minutes_last_edited) continue;
                    else
                    {
                        // Start working with local table
                        List<StockTable> stock_got = new(); // make a stock table list

                        WasEditModifier = false;
                        //using FileStream fileStream = new(AppSettings.Current.DropBoxData.DropBoxAbsoluteTotalPath + user_id + "_stock.xlsx", FileMode.Open); // create a filestream
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // set a license
                        ExcelPackage excel = new(stock_table_fi); // make new ExcelPackage
                        if (excel.Workbook.Worksheets.Count > 0)
                        {
                            var workSheet = excel.Workbook.Worksheets.First(); // get first sheet on this table
                            var got = workSheet.ConvertSheetToObjects<StockTable>(); // Convert a WorkSheet to our DTO object
                            stock_got = got.ToList(); // convert to list


                            foreach (var row in stock_got) // Start searching equals 
                            {
                                if (row.Replaces == null && row.Sku != null)
                                {
                                    int index = stock_got.IndexOf(row);
                                    MakeReplaces(row.Sku, index, excel, workSheet);
                                }
                            }
                            if (WasEditModifier) excel.Save();
                        }
                        //fileStream.Close(); // Close filestream
                    }

                    



                }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// [Private] A method for generating Replaces and Title.
        /// </summary>
        /// <param name="search_data">SKU</param>
        /// <param name="index">Index of row</param>
        /// <param name="package">Package interface</param>
        /// <param name="worksheet">Worksheet interface</param>
        private static void MakeReplaces (string search_data, int index, ExcelPackage package, ExcelWorksheet worksheet)
        {
            Console.WriteLine("Editing row: " + (index + 1));
            string total_replaces = string.Empty;
            string total_title = string.Empty;

            List<Task> tasks = new();
            List<string> replaces = new();
            List<string> tempNames = new();

            search_data = search_data.Trim();
            
            //Starting a thread for Fix.Com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.FixComReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.ReliablePartsReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.AppliancePartsHQReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.AppliancePartsPros(search_data, replaces, tempNames)));
            //Starting a thread for easyapplianceparts.ca
            tasks.Add(Task.Run(() => ReplacesSiteMethods.EasyAppliancePartsCaNames(search_data, tempNames)));
            //Starting a thread for partselect.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.PartSelectComNames(search_data, tempNames)));
            //Starting a thread for partselect.ca
            tasks.Add(Task.Run(() => ReplacesSiteMethods.PartSelectCaNames(search_data, tempNames)));
            //Starting a thread for apwagner.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.ApWagnerComNames(search_data, tempNames)));
            //Starting a thread for midbec.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.MidbecComReplaces(search_data, replaces)));

            

            Task.WaitAll(tasks.ToArray());

            replaces = replaces.Distinct().ToList();
            replaces.Remove(search_data.ToLower());
            replaces.Remove(search_data.ToUpper());
            replaces.Add(search_data.ToUpper());
            replaces = replaces.Distinct().ToList();
            replaces = replaces.OrderBy(x => x).ToList();

            if (replaces.Count > 0)
            {
                if (replaces.Count > 20)
                    replaces = replaces.GetRange(0, 20);
                total_replaces = string.Join(',', replaces);
            }
            else total_replaces = "NoReplaces";

            tempNames.RemoveAll(x => x.Trim().Contains(@"Searching, please wait...") || x.ToUpper().Contains("USE WPL") || x.Contains('<') || x.Contains('>'));

            if (tempNames.Count > 0)
            {
                int minLength = tempNames.Min(y => y.Length); // this gets you the shortest length of all elements in names
                string shortest = tempNames.FirstOrDefault(x => x.Length == minLength);
                total_title = shortest;
            }
            else total_title = string.Empty;
            worksheet.Cells[$"E{index+1}"].LoadFromText(total_title); // Add a title
            worksheet.Cells[$"G{index + 1}"].Value = total_replaces; // Add a replaces
            WasEditModifier = true;
        }

        /// <summary>
        /// [Public] A method for generating Replaces and Title.
        /// </summary>
        /// <param name="search_data">SKU</param>
        /// <param name="total_replaces">A string with replaces. Delimiter is ','.</param>
        /// <param name="total_title">A string with got from sites shortest title.</param>
        public static void MakeReplaces(string search_data, out string total_replaces, out string total_title)
        {
           
            List<Task> tasks = new();
            List<string> replaces = new();
            List<string> tempNames = new();

            search_data = search_data.Trim();

            //Starting a thread for Fix.Com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.FixComReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.ReliablePartsReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.AppliancePartsHQReplaces(search_data, replaces, tempNames)));
            //Starting a thread for reliableparts.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.AppliancePartsPros(search_data, replaces, tempNames)));
            //Starting a thread for easyapplianceparts.ca
            tasks.Add(Task.Run(() => ReplacesSiteMethods.EasyAppliancePartsCaNames(search_data, tempNames)));
            //Starting a thread for partselect.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.PartSelectComNames(search_data, tempNames)));
            //Starting a thread for partselect.ca
            tasks.Add(Task.Run(() => ReplacesSiteMethods.PartSelectCaNames(search_data, tempNames)));
            //Starting a thread for apwagner.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.ApWagnerComNames(search_data, tempNames)));
            //Starting a thread for midbec.com
            tasks.Add(Task.Run(() => ReplacesSiteMethods.MidbecComReplaces(search_data, replaces)));



            Task.WaitAll(tasks.ToArray());

            replaces = replaces.Distinct().ToList();
            replaces.Remove(search_data.ToLower());
            replaces.Remove(search_data.ToUpper());
            replaces.Add(search_data.ToUpper());
            replaces = replaces.Distinct().ToList();
            replaces = replaces.OrderBy(x => x).ToList();

            if (replaces.Count > 0)
            {
                if (replaces.Count > 20)
                    replaces = replaces.GetRange(0, 20);
                total_replaces = string.Join(',', replaces);
            }
            else total_replaces = "NoReplaces";

            tempNames.RemoveAll(x => x.Trim().Contains(@"Searching, please wait...") || x.ToUpper().Contains("USE WPL") || x.Contains('<') || x.Contains('>'));

            if (tempNames.Count > 0)
            {
                int minLength = tempNames.Min(y => y.Length); // this gets you the shortest length of all elements in names
                string shortest = tempNames.FirstOrDefault(x => x.Length == minLength);
                total_title = shortest;
            }
            else total_title = string.Empty;
        }
    }
}
