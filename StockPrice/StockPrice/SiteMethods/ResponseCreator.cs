using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using StockPrice.DatabaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using StockPrice.ResponseClasses;
using static StockPrice.SiteMethods.Classes.AmazonCAClasses;

namespace StockPrice.SiteMethods
{
    internal class ResponseCreator
    {
        const string Type = "Price";

        /// <summary>
        /// Make a error log, add it in MainPriceResponseList, add it in DB
        /// </summary>
        /// <param name="con">MySQL connection</param>
        /// <param name="mpr">Main Price Response</param>
        /// <param name="mainPriceResponsesList">Main Price Response List</param>
        /// <param name="request">Request by working</param>
        /// <param name="stage">Number of stage</param>
        /// <param name="source">Source of response</param>
        /// <param name="classSource">Source of response class</param>
        /// <param name="base64WrongData">Additional data</param>
        /// <param name="base64ErrorData">Exception text or your text</param>
        /// <param name="url">Url of error data if required</param>
        /// <returns></returns>
        public static async Task MakeErrorLog(MySqlConnection con, 
            MainPriceResponse mpr, 
            List<MainPriceResponse> mainPriceResponsesList, 
            DatabaseTotalResults request, 
            string? base64ErrorData,
            int stage,
            string source,
            string classSource,
            string? base64WrongData,
            string? url)
        {
            await con.OpenAsync();
            var errorLog = new DatabaseUnregisteredResponses
            {
                RequestId = request.ID,
                RequestText = request.Request,
                Comment = $"STAGE {stage}",
                Url = url,
                Source = source,
                Base64errorData = base64ErrorData != null ? MySqlHelper.EscapeString(base64ErrorData) : null,
                Base64wrongData = base64WrongData != null ? MySqlHelper.EscapeString(base64WrongData) : null
            };


            await con.QueryFirstOrDefaultAsync<DatabaseSitesPriority>(
                $"INSERT INTO `unregistered_responses` (`requestId`, `requestText`, `source`, `base64wrongData`, `base64errorText`, `Comment`, `Url`) VALUES ('{errorLog.RequestId}', '{errorLog.RequestText}', '{errorLog.Source}', '{errorLog.Base64wrongData}', '{errorLog.Base64errorData}', '{errorLog.Comment}', '{errorLog.Url}');");
            await con.CloseAsync();
            Console.WriteLine($"Error on {classSource} registered. STAGE - {stage}    Request: {request.Request}");

            mpr.NoAnswerOrError = true;
            mpr.ErrorMessage = base64ErrorData;
            mainPriceResponsesList.Add(mpr);

            await con.OpenAsync();
            
            string dataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(mpr));
            await con.QueryFirstOrDefaultAsync<DatabaseSitesPriority>(
                $"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{Type}', '{dataEscaped}');");
            await con.CloseAsync();
        }



        /// <summary>
        /// Make a response log and add it in DB
        /// </summary>
        /// <param name="con"></param>
        /// <param name="mpr"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task MakeResponseLog(MySqlConnection con,
            MainPriceResponse mpr,
            DatabaseTotalResults request)
        {
            await con.OpenAsync();
            
            string dataEscaped = MySqlHelper.EscapeString(JsonConvert.SerializeObject(mpr));
            await con.QueryFirstOrDefaultAsync<DatabaseSitesPriority>($"INSERT INTO `response_temp_db` (`RequestID`, `Type`, `Data`) VALUES ({request.ID}, '{Type}', '{dataEscaped}');");
            await con.CloseAsync();
        }
    }
}
