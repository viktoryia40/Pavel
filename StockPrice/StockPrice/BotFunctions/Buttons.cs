using Dapper;
using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StockPrice.BotFunctions
{
    class Buttons
    {
        
        /// <summary>
        /// Make InlineKeyboardMarkup when person is only registered
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup ChoseYourCountryButton()
        {
            List<List<InlineKeyboardButton>> buttons = new();

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Canada")
                {
                    Text = "🇨🇦 CANADA",
                    CallbackData = "ChooseCountry_Canada"
                }
            });

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Usa")
                {
                    Text = "🇺🇸 USA",
                    CallbackData = "ChooseCountry_USA"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        public static InlineKeyboardMarkup AmazonDaysCountChangeButton()
        {
            List<List<InlineKeyboardButton>> buttons = new();

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "🖊 Change the number of delivery days.",
                    CallbackData = "ChangeNumberOfDeliveryDays"
                }
            });

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        public static InlineKeyboardMarkup CloseButton()
        {
            List<List<InlineKeyboardButton>> buttons = new();

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        public static InlineKeyboardMarkup AlldoneButton()
        {
            List<List<InlineKeyboardButton>> buttons = new();

            buttons.Add(new()
            {
                new InlineKeyboardButton($"null")
                {
                    Text = "All done",
                    CallbackData = "AllDoneTableCallback"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        /// <summary>
        /// Make InlineKetboardMarkup for Canada sites
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static InlineKeyboardMarkup CanadaSettingsOutput(Update update)
        {
            List<List<InlineKeyboardButton>> buttons = new();

            string true_em = "✅";
            string false_em = "❌";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            long userid = 0;
            if (update.Message != null) userid = update.Message.Chat.Id;
            else userid = update.CallbackQuery.From.Id;


            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{userid}';");

            con.Close();


            if (userdata == null)
            {
                return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>() {
                new()
                {
                    new InlineKeyboardButton("ABCD")
                    {
                         Text = $"You are not registered! Write a /start",
                         CallbackData = "RegisterCallback"
                    }
                }
                });
            }

            string now_em = null;

            //Button for Amazon.ca
            if (userdata.AmazonCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"🅰 Amazon.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_AmazonCA_CA"
                        }
                    });


            //Button for appliancepartshqCA
            if (userdata.AppliancepartshqCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Appliancepartshq.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_AppliancepartshqCA_CA"
                        }
                    });

            //Button for partsexpertCa
            if (userdata.PartsexpertCa)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Partsexpert.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_PartsexpertCA_CA"
                        }
                    });

            //Button for PartselectCA
            if (userdata.PartselectCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Partselect.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_PartselectCA_CA"
                        }
                    });

            //Button for ReliablepartsCA
            if (userdata.ReliablepartsCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Reliableparts.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_ReliablepartsCA_CA"
                        }
                    });

            //Button for EasyappliancepartsCA
            if (userdata.EasyappliancepartsCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Easyapplianceparts.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_EasyappliancepartsCA_CA"
                        }
                    });

            //Button for AmresupplyCOM
            if (userdata.AmresupplyCOM)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Amresupply.com {now_em}",
                            CallbackData = "ChangeResourceCallback_AmresupplyCOM_CA"
                        }
                    });

            //Button for UniversalappliancepartsCA
            if (userdata.UniversalappliancepartsCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Universalapplianceparts.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_UniversalappliancepartsCA_CA"
                        }
                    });

            //Button for GreenlineappliancespartsCA
            if (userdata.GreenlineappliancespartsCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Greenlineappliancesparts.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_GreenlineappliancespartsCA_CA"
                        }
                    });

            //Button for AppliancepartsCanadaCom
            if (userdata.AppliancepartsCanadaCom)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"AppliancepartsCanada.com {now_em}",
                            CallbackData = "ChangeResourceCallback_AppliancepartsCanadaCom_CA"
                        }
                    });

            //Button for ApwagnerCA
            if (userdata.ApwagnerCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Apwagner.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_ApwagnerCA_CA"
                        }
                    });

            //Button for MajorappliancepartsCA
            if (userdata.MajorappliancepartsCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Majorapplianceparts.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_MajorappliancepartsCA_CA"
                        }
                    });



            //Button for MarconeCanada
            if (userdata.MarconeCanada)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Marcone.com {now_em}",
                            CallbackData = "ChangeResourceCallback_MarconeCanada_CA"
                        }
                    });

            //Button for EbayCA
            if (userdata.EbayCA)
                now_em = true_em;
            else
                now_em = false_em;

            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"🅱 eBay.ca {now_em}",
                            CallbackData = "ChangeResourceCallback_EbayCA_CA"
                        }
                    });


            buttons = buttons.OrderBy(x => x[0].CallbackData).ToList();


            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close the settings menu",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);



        }

        /// <summary>
        /// Make InlineKeyboardMarkup for USA sites
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static InlineKeyboardMarkup UsaSettingsOutput(Update update)
        {
            List<List<InlineKeyboardButton>> buttons = new();

            const string trueEm = "✅";
            const string falseEm = "❌";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            long userid = 0;
            userid = update.Message != null ? update.Message.Chat.Id : update.CallbackQuery.From.Id;


            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{userid}';");

            con.Close();

            if (userdata == null)
            {
                return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>() {
                new()
                {
                    new InlineKeyboardButton("ABCD")
                    {
                         Text = $"You are not registered! Write a /start",
                         CallbackData = "RegisterCallback"
                    }
                }
                });
            }

            string nowEm = null;

            //Button for Lowes.com
            /*nowEm = userdata.LowesCOM ? trueEm : falseEm;
            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Lowes.com {nowEm}",
                            CallbackData = "ChangeResourceCallback_LowesCOM_USA"
                        }
                    });*/


            //Button for Partswarehouse.com
            nowEm = userdata.PartswarehouseCOM ? trueEm : falseEm;
            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Partswarehouse.com {nowEm}",
                            CallbackData = "ChangeResourceCallback_PartswarehouseCOM_USA"
                        }
                    });

            //Button for Bulbspro.com
            nowEm = userdata.BulbsproCOM ? trueEm : falseEm;
            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Bulbspro.com {nowEm}",
                            CallbackData = "ChangeResourceCallback_BulbsproCOM_USA"
                        }
                    });


            //Button for Marcone.com
            nowEm = userdata.MarconeUsa ? trueEm : falseEm;
            buttons.Add(new() {
                        new InlineKeyboardButton($"ABCD")
                        {
                            Text = $"Marcone.com {nowEm}",
                            CallbackData = "ChangeResourceCallback_MarconeUsa_USA"
                        }
                    });


            //Button for Searspartsdirect.com
            nowEm = userdata.SearsPartsDirectCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Searspartsdirect.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_SearsPartsDirectCOM_USA"
                }
            });

            //Button for Coastparts.com
            nowEm = userdata.CoastPartsCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Coastparts.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_CoastPartsCOM_USA"
                }
            });

            //Button for Guaranteedparts.com
            nowEm = userdata.GuaranteedPartsCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Guaranteedparts.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_GuaranteedPartsCOM_USA"
                }
            });

            //Button for Partsdr.com
            nowEm = userdata.PartsDrCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Partsdr.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_PartsDrCOM_USA"
                }
            });

            //Button for Appliancepartspros.com
            nowEm = userdata.AppliancePartsProsCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Appliancepartspros.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_AppliancePartsProsCOM_USA"
                }
            });

            //Button for Partselect.com
            nowEm = userdata.PartSelectCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Partselect.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_PartSelectCOM_USA"
                }
            });

            //Button for Applianceparts365.com
            nowEm = userdata.ApplianceParts365COM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Applianceparts365.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_ApplianceParts365COM_USA"
                }
            });

            //Button for Apwagner.com
            nowEm = userdata.ApwagnerCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Apwagner.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_ApwagnerCOM_USA"
                }
            });

            //Button for reliableparts.com
            nowEm = userdata.ReliablePartsCom ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Reliableparts.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_ReliablePartsCOM_USA"
                }
            });

            //Button for ebay.com
            nowEm = userdata.EbayCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"🅱 Ebay.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_EbayCOM_USA"
                }
            });

            //Button for amazon.com
            nowEm = userdata.AmazonCOM ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"🅰 Amazon.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_AmazonCOM_USA"
                }
            });


            //Button for dlpartsco.com
            nowEm = userdata.DlPartsCoCom ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Dlpartsco.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_DlPartsCoCom_USA"
                }
            });




            //Button for cashweels.com
            nowEm = userdata.CashWellsCom ? trueEm : falseEm;
            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Cashweels.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_CashWellsCom_USA"
                }
            });

            //Button for Encompass
            nowEm = userdata.EncompassCOM ? trueEm : falseEm;

            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Encompass.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_EncompassCOM_USA"
                }
            });

            //Button for Repairclinic.com
            nowEm = userdata.RepairClinicCom ? trueEm : falseEm;

            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Repairclinic.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_RepairClinicCom_USA"
                }
            });

            //Button for https://www.partstown.com/
            nowEm = userdata.PartsTownCom ? trueEm : falseEm;

            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Partstown.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_PartsTownCom_USA"
                }
            });

            //Button for https://www.allvikingparts.com/
            nowEm = userdata.AllVikingPartsCom ? trueEm : falseEm;

            buttons.Add(new() {
                new InlineKeyboardButton($"ABCD")
                {
                    Text = $"Allvikingparts.com {nowEm}",
                    CallbackData = "ChangeResourceCallback_AllVikingPartsCom_USA"
                }
            });




            buttons = buttons.OrderBy(x => x[0].CallbackData).ToList();

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close the settings menu",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }


        /// <summary>
        /// Make InlineKeyboardMarkup for USA sites
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static InlineKeyboardMarkup MyProfileSettingsOutput(Update update)
        {
            List<List<InlineKeyboardButton>> buttons = new();

            string true_em = "✅";
            string false_em = "❌";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            long userid = 0;
            if (update.Message != null) userid = update.Message.Chat.Id;
            else userid = update.CallbackQuery.From.Id;


            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{userid}';");

            con.Close();

            if (userdata == null)
            {
                return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>() {
                new()
                {
                    new InlineKeyboardButton("ABCD")
                    {
                         Text = $"You are not registered! Write a /start",
                         CallbackData = "RegisterCallback"
                    }
                }
                });
            }


            string now_em = null;

            //Button for USA
            if (userdata.ParseUSA)
                now_em = true_em;
            else
                now_em = false_em;
            buttons.Add(new() {
                        new InlineKeyboardButton($"null")
                        {
                            Text = $"Parse USA {now_em}",
                            CallbackData = "ChangeParseCallback_USA"
                        }
                    });
            //Button for Canada
            if (userdata.ParseCanada)
                now_em = true_em;
            else
                now_em = false_em;
            buttons.Add(new() {
                        new InlineKeyboardButton($"null")
                        {
                            Text = $"Parse Canada {now_em}",
                            CallbackData = "ChangeParseCallback_CA"
                        }
                    });


            if (!userdata.IsHaveStockTable)
                buttons.Add(new() {
                        new InlineKeyboardButton($"null")
                        {
                            Text = $"Create a stock table.",
                            CallbackData = "CreateTableCallback"
                        }
                    });

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close the settings menu",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for Stock File Data
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static InlineKeyboardMarkup MyStockDataMarkup(Update update)
        {
            List<List<InlineKeyboardButton>> buttons = new();

            string true_em = "✅";
            string false_em = "❌";

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);
            con.Open();
            long userid = 0;
            if (update.Message != null) userid = update.Message.Chat.Id;
            else userid = update.CallbackQuery.From.Id;


            DatabaseUserData userdata = con.QueryFirstOrDefault<DatabaseUserData>($"SELECT * FROM userdata WHERE `userid`='{userid}';");

            con.Close();

            if (userdata == null)
            {
                return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>() {
                new()
                {
                    new InlineKeyboardButton("ABCD")
                    {
                         Text = $"You are not registered! Write a /start",
                         CallbackData = "RegisterCallback"
                    }
                }
                });
            }


            string now_em = null;
            
            if (!userdata.IsHaveStockTable)
                buttons.Add(new() {
                        new InlineKeyboardButton($"null")
                        {
                            Text = $"Create a stock table.",
                            CallbackData = "CreateTableCallback"
                        }
                    });
            else
            {
                buttons.Add(new() {
                    new InlineKeyboardButton($"null")
                    {
                        Text = $"Create a new stock table.",
                        CallbackData = "CreateNewTableCallback"
                    }
                });
            }

            buttons.Add(new()
            {
                new InlineKeyboardButton($"Close")
                {
                    Text = "❌ Close the settings menu",
                    CallbackData = "CloseSettingsMenu"
                }
            });

            return new InlineKeyboardMarkup(buttons);

        }

        /// <summary>
        /// Make ForceReplyMarkup for Spreadsheet
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SpreadsheetFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "https://docs.google.com/spreadsheets/d/1Om3ou1S3un1KyiG35DTAwTjZLMi8u3hV7Us5xViCToM/edit#gid=0";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Spreadsheet Sheet Name
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SpreadsheetSheetNameFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "Лист1";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Email data
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup IntegerFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "Целочисленное значение: 5,8,15 и т.д.";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Integer data
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup EmailFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "example@site.com";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Site URL data
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SiteFRM()
        {
            var a = new ForceReplyMarkup
            {
                InputFieldPlaceholder = "https://example.com"
            };
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for SKU data
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SkuFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "ABCD123EFG";
            return a;

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for Show More action
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup ShowMoreIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "⇣ Show other results",
                                "ShowMoreQuery"
                            )
                        }

                    }
                );

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for Show More action
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup ShowLessIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "⇡ Hide other results",
                                "ShowLessQuery"
                            )
                        }

                    }
                );

        }

    }
}
