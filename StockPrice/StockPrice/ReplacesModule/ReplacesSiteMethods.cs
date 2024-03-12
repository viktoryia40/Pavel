using Newtonsoft.Json;
using StockPrice.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockPrice.ReplacesModule
{
    public class ReplacesSiteMethods
    {
        public static void FixComReplaces(string search, List<string> replaces, List<string> tempNames)
        {


            try
            {

                string location = CustomHttpClass.CheckRedirectGet(@$"https://www.fix.com/Search.ashx?SearchTerm={search}&SearchMethod=standard");
                if (!string.IsNullOrEmpty(location))
                {

                    string html_result = CustomHttpClass.GetToString($@"{location}");


                    var title_regex = Regex.Matches(html_result, @"(?<=itemprop=""name"">).*(?=</h1>)");

                    //Add a title
                    if (title_regex.Count > 0)
                    {
                        if (title_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(title_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {

                            }


                        }
                        else
                            tempNames.Add(title_regex.First().Value.Trim());
                    }




                    var manufacture_regex = Regex.Matches(html_result, @"(?<=<span itemprop=""mpn"">).*?(?=</span></div>)");
                    if (manufacture_regex.Count > 0) replaces.Add(manufacture_regex.First().Value.Trim());
                    var replaces_regex_html_block = Regex.Matches(html_result, @"(?<=replaces these:</div>)[\w\W]*?</div>");
                    if (replaces_regex_html_block.Count > 0)
                    {

                        var replaces_regex = Regex.Matches(replaces_regex_html_block.First().Value, @"(?<=>)[\w\W]*?(?=</div>)");
                        if (replaces_regex.Count > 0)
                        {
                            var replaces_split = Regex.Replace(replaces_regex.First().Value.Replace(".", "").Replace("Show more", "").Replace("Show less", "").Replace("\r", "").Replace("\n", ""), @"<(.|\n)*?>", "").Split(',').ToList();
                            foreach (var replace in replaces_split)
                            {
                                replaces.Add(replace.Trim());
                            }
                        }

                    }



                }
            }
            catch
            {
                Console.WriteLine("Error FIX.COM - STAGE 1 - " + search);
            }

        }

        public static void ReliablePartsReplaces(string search, List<string> replaces, List<string> tempNames)
        {
            string location = null;

            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.reliableparts.com/search?q={search}",
                referrer: "https://www.reliableparts.com/",
                acceptencoding: "none");
            }
            catch
            {

                Console.WriteLine($"Error on ReliableParts registered. STAGE - 0    Request: {search}");
                return;
            }

            if (location != null)
            {
                /*string search_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.com/search?q={search}",
               referrer: "https://www.reliableparts.com/",
               acceptencoding: "none");*/
                try
                {
                    string total_result = CustomHttpClass.GetToString(@$"https://www.reliableparts.com{location}",
                 referrer: $"https://www.reliableparts.com/search?q={search}",
                 acceptencoding: "none");

                    var manufacture_regex = Regex.Matches(total_result, @"(?<=""partNumber"">Part #: ).*(?=</h2>)");
                    if (manufacture_regex.Count > 0) replaces.Add(manufacture_regex.First().Value.Trim());

                    var name_regex = Regex.Matches(total_result, @"(?<=id=""mainHeading"">).*(?=</h1>)");
                    if (name_regex.Count > 0) tempNames.Add(name_regex.First().Value.Trim());


                    var replaces_table_regex = Regex.Matches(total_result, @"(?<=<div class=""accordian-point"">)[\w\W]*?(?=</div>)");
                    if (replaces_table_regex.Count > 0)
                    {
                        string replaces_table = replaces_table_regex.First().Value;
                        var replaces_regex = Regex.Matches(replaces_table, @"(?<=<li>).*(?=</li>)");

                        foreach (var replace in replaces_regex)
                        {
                            if (replace.ToString().Contains("href") || replace.ToString().Contains("http")) continue;
                            replaces.Add(replace.ToString().Trim());
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"Error on ReliableParts registered. STAGE - 1    Request: {search}");
                    return;
                }
            }
        }

        public static void AppliancePartsHQReplaces(string search, List<string> replaces, List<string> tempNames)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.appliancepartshq.ca/search?hq={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    List<string> tempreplaces = new();

                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none");
                    var manufacture_regex = Regex.Matches(result, @"(?<=SKU: <span>).*?(?=</span></div>)");
                    if (manufacture_regex.Count > 0) tempreplaces.Add(manufacture_regex.First().Value.Trim());



                    var replaces_regex = Regex.Matches(result, @"(?<=Replaces Old Numbers:</span></strong> &nbsp;)[\w\W]*?(?=</p>)");
                    if (replaces_regex.Count > 0)
                    {
                        var replaces_table = replaces_regex.First().Value;
                        var replaces_split = replaces_table.Split(',');
                        foreach (var replace in replaces_split)
                        {
                            tempreplaces.Add(replace.Trim());
                        }
                    }

                    if (tempreplaces.Contains(search.ToLower()) || tempreplaces.Contains(search.ToUpper()))
                    {
                        var name_regex = Regex.Matches(result, @"(?<=<title>).*(?=</title>)");
                        if (name_regex.Count > 0)
                        {
                            string tempname = name_regex.First().Value.Trim();
                            var tempname_split = tempname.Split(':').ToList();
                            tempNames.Add(tempname_split.Last().Trim());
                        }
                        foreach (var replace in tempreplaces)
                        {
                            replaces.Add(replace);
                        }
                    }


                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 1 - {search}");
                }
            }
            else
            {
                string search_result = null;
                try
                {
                    search_result = CustomHttpClass.GetToString(@$"https://www.appliancepartshq.ca/search?hq={search}", acceptencoding: "none");


                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 2 - {search}");
                }
                if (search_result != null)
                {
                    var search_result_regex_block = Regex.Matches(search_result, @"(?<=<div class=""productListing"">)[\w\W]*?(?=</div>)");

                    foreach (var search_result_block in search_result_regex_block)
                    {
                        var href_regex = Regex.Matches(search_result_block.ToString(), @"(?<=<a href="").*?(?="" title=)");
                        var title_regex = Regex.Matches(search_result_block.ToString(), @"(?<=title="").*?(?="">)");

                        if (href_regex.Count > 0 && title_regex.Count > 0 && href_regex.Count == title_regex.Count)
                        {
                            List<string> title_list = title_regex.First().Value.Split(' ').ToList();
                            string href = href_regex.First().Value;

                            if (title_list.Contains(search.ToLower()) || title_list.Contains(search.ToUpper()))
                            {
                                location = href;

                                string result = null;
                                try
                                {
                                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none");

                                    var manufacture_regex = Regex.Matches(result, @"(?<=SKU: <span>).*?(?=</span></div>)");
                                    if (manufacture_regex.Count > 0) replaces.Add(manufacture_regex.First().Value.Trim());

                                    var replaces_regex = Regex.Matches(result, @"(?<=Replaces Old Numbers:</span></strong> &nbsp;)[\w\W]*?(?=</p>)");
                                    if (replaces_regex.Count > 0)
                                    {
                                        var replaces_table = replaces_regex.First().Value;
                                        var replaces_split = replaces_table.Split(',');
                                        foreach (var replace in replaces_split)
                                        {
                                            replaces.Add(replace.Trim());
                                        }
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 3 - {search}");
                                }

                            }
                        }
                    }
                }
            }

        }

        public static void AppliancePartsPros(string search, List<string> replaces, List<string> tempNames)
        {
            //(?<=replaces).*?(?=\.) - for replays (comma-delimited output, trim required)

            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.appliancepartspros.com/search.aspx?q={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.appliancepartspros.com{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0) tempNames.Add(name_regex.First().Value.Trim());

                    var replaces_regex = Regex.Matches(result, @"(?<=replaces).*?(?=\.)");
                    if (replaces_regex.Count > 0)
                    {
                        var replaces_split = replaces_regex.First().Value.Split(',');
                        foreach (var replace in replaces_split)
                            replaces.Add(replace.Trim());
                    }
                }
                catch
                {
                    Console.WriteLine($"AppliancePartsHQReplaces ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void EasyAppliancePartsCaNames(string search, List<string> tempNames)
        {
            //(?<=replaces).*?(?=\.) - for replays (comma-delimited output, trim required)

            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.easyapplianceparts.ca/Search.ashx?SearchTerm={search}&SearchMethod=standard", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"EasyAppliancePartsCaNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=""standard-blue-title"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                return;
                            }


                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }


                }
                catch
                {
                    Console.WriteLine($"EasyAppliancePartsCaNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void PartSelectComNames(string search, List<string> tempNames)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.com/Search.ashx?SearchTerm={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"PartSelectComNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                return;
                            }


                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }


                }
                catch
                {
                    Console.WriteLine($"PartSelectComNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void PartSelectCaNames(string search, List<string> tempNames)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.partselect.ca/Search.ashx?SearchTerm={search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"PartSelectCaNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"{location}", acceptencoding: "none", use_google_ua: false);

                    var name_regex = Regex.Matches(result, @"(?<=itemprop=""name"">).*(?=</h1>)");
                    if (name_regex.Count > 0)
                    {
                        if (name_regex.First().Value.Trim().Contains("USE WPL"))
                        {
                            var wpl_regex = Regex.Matches(name_regex.First().Value.Trim(), @"(?<=USE WPL).*");
                            if (wpl_regex.Count > 0)
                            {
                                return;
                            }


                        }
                        tempNames.Add(name_regex.First().Value.Trim());
                    }

                }
                catch
                {
                    Console.WriteLine($"PartSelectCaNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void ApWagnerComNames(string search, List<string> tempNames)
        {


            string location = null;
            try
            {
                location = CustomHttpClass.CheckRedirectGet(@$"https://www.apwagner.com/search/{search}", acceptencoding: "none");

            }
            catch
            {
                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 0 - {search}");
            }

            if (location != null)
            {

                string result = null;
                try
                {
                    result = CustomHttpClass.GetToString($@"https://www.apwagner.com{location}", acceptencoding: "none");

                    var title_result_regex = Regex.Matches(result, @"(?<=<title>)[\w\W]*?(?=</title>)");
                    if (title_result_regex.Count > 0 && title_result_regex.First().Value.ToUpper().Trim().Contains(@"USE WPL"))
                    {
                        var href_use_wpl_regex = Regex.Matches(result, @"(?<=href="").*(?="">USE WPL)");
                        if (href_use_wpl_regex.Count > 0)
                        {
                            try
                            {
                                result = CustomHttpClass.GetToString($@"https://www.apwagner.com{href_use_wpl_regex.First().Value.Trim()}", acceptencoding: "none");
                            }
                            catch
                            {
                                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 2 - {search}");
                            }

                            var name_regex = Regex.Matches(result, @"(?<=<h1 itemprop=""name"">)[\w\W]*?(?=</h1>)");
                            if (name_regex.Count > 0)
                            {
                                tempNames.Add(name_regex.First().Value.Trim());
                            }
                        }

                    }
                    else
                    {
                        var name_regex = Regex.Matches(result, @"(?<=<h1 itemprop=""name"">)[\w\W]*?(?=</h1>)");
                        if (name_regex.Count > 0)
                        {
                            tempNames.Add(name_regex.First().Value.Trim());
                        }
                    }


                }
                catch
                {
                    Console.WriteLine($"ApWagnerComNames ERROR - STAGE 1 - {search}");
                }

            }

        }

        public static void MidbecComReplaces(string search, List<string> replaces)
        {






            string result = null;
            try
            {

                result = CustomHttpClass.PostToString(url: @"https://midbec.com/core/direct/router",
                    jsonData: $@"[{{""action"":""ecommerce.ajax"",""method"":""updateProductTableList"",""data"":[{{""search"":""{search}"",""categories"":[],""currentPage"":1,""offset"":0,""nbPerPage"":""15"",""sortBy"":""relevance"",""pricerange"":""0"",""manufacturer"":[""""],""pageTagName"":""products""}}],""type"":""rpc"",""tid"":0}}]",
                    contentType: "application/json");




            }
            catch
            {
                Console.WriteLine($"ApWagnerComNames ERROR - STAGE 1 - {search}");
            }

            if (result != null)
                try
                {
                    dynamic _j = JsonConvert.DeserializeObject(result);
                    string replace = _j[0].result.products[0].sku.ToString();
                    replaces.Add(replace.Trim());
                }
                catch
                {

                }


        }
    }
}
