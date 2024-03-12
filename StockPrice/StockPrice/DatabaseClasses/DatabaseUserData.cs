
namespace StockPrice.DatabaseClasses
{
    /// <summary>
    /// CLass for userdata table
    /// </summary>
    class DatabaseUserData
    {
        /// <summary>
        /// ID of row in DB
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// User ID (Chat ID)
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Parse Canada prices YES or NO (true/false)
        /// </summary>
        public bool ParseCanada { get; set; }


        /// <summary>
        /// Parse USA prices YES or NO (true/false)
        /// </summary>
        public bool ParseUSA { get; set; }

        /// <summary>
        /// CANADA parse http://appliancepartshq.ca bool
        /// </summary>
        public bool AppliancepartshqCA { get; set; }

        /// <summary>
        /// CANADA parse http://partsexpert.ca bool
        /// </summary>
        public bool PartsexpertCa { get; set; }

        /// <summary>
        /// USA parse http://lowes.com bool
        /// </summary>
        public bool LowesCOM { get; set; }

        /// <summary>
        /// CANADA parse http://partselect.ca bool
        /// </summary>
        public bool PartselectCA { get; set; }

        /// <summary>
        /// MULTI parse http://reliableparts.ca bool
        /// </summary>
        public bool ReliablepartsCA { get; set; }

        /// <summary>
        /// CANADA parse http://easyapplianceparts.ca bool
        /// </summary>
        public bool EasyappliancepartsCA { get; set; }

        /// <summary>
        /// MULTI parse http://amresupply.com bool
        /// </summary>
        public bool AmresupplyCOM { get; set; }

        /// <summary>
        /// CANADA parse http://universalapplianceparts.ca bool
        /// </summary>
        public bool UniversalappliancepartsCA { get; set; }


        /// <summary>
        /// USA parse http://partswarehouse.com bool
        /// </summary>
        public bool PartswarehouseCOM { get; set; }

        /// <summary>
        /// CANADA parse http://greenlineappliancesparts.ca bool
        /// </summary>
        public bool GreenlineappliancespartsCA { get; set; }


        /// <summary>
        /// CANADA parse http://appliancepartsCanada.com bool
        /// </summary>
        public bool AppliancepartsCanadaCom { get; set; }

        /// <summary>
        /// CANADA parse http://apwagner.ca bool
        /// </summary>
        public bool ApwagnerCA { get; set; }

        /// <summary>
        /// USA parse http://bulbspro.com bool
        /// </summary>
        public bool BulbsproCOM { get; set; }

        /// <summary>
        /// CANADA parse http://majorapplianceparts.ca bool
        /// </summary>
        public bool MajorappliancepartsCA { get; set; }

        /// <summary>
        /// CANADA parse http://amazon.ca bool
        /// </summary>
        public bool AmazonCA { get; set; }

        /// <summary>
        /// USA parse http://amazon.com bool
        /// </summary>
        public bool AmazonCOM { get; set; }

        /// <summary>
        /// Canada parse http://marcone.com bool
        /// </summary>
        public bool MarconeCanada { get; set; }

        /// <summary>
        /// USA parse http://marcone.com bool
        /// </summary>
        public bool MarconeUsa { get; set; }

        /// <summary>
        /// CA parse http://ebay.ca bool
        /// </summary>
        public bool EbayCA { get; set; }

        /// <summary>
        /// USA parse http://ebay.com bool
        /// </summary>
        public bool EbayCOM { get; set; }

        /// <summary>
        /// USA parse http://encompass.com bool
        /// </summary>
        public bool EncompassCOM { get; set; }

        /// <summary>
        /// USA parse http://coastparts.com bool
        /// </summary>
        public bool CoastPartsCOM { get; set; }

        /// <summary>
        /// USA parse http://guaranteedparts.com bool
        /// </summary>
        public bool GuaranteedPartsCOM { get; set; }

        /// <summary>
        /// USA parse http://partsdr.com bool
        /// </summary>
        public bool PartsDrCOM { get; set; }

        /// <summary>
        /// USA parse http://appliancepartspros.com bool
        /// </summary>
        public bool AppliancePartsProsCOM { get; set; }

        /// <summary>
        /// USA parse http://partselect.com bool
        /// </summary>
        public bool PartSelectCOM { get; set; }

        /// <summary>
        /// USA parse http://applianceparts365.com bool
        /// </summary>
        public bool ApplianceParts365COM { get; set; }

        /// <summary>
        /// USA parse http://apwagner.com bool
        /// </summary>
        public bool ApwagnerCOM { get; set; }

        /// <summary>
        /// USA parse http://searspartsdirect.com bool
        /// </summary>
        public bool SearsPartsDirectCOM { get; set; }

        /// <summary>
        /// USA parse https://www.reliableparts.com/ bool
        /// </summary>
        public bool ReliablePartsCom { get; set; }

        /// <summary>
        /// USA parse https://www.dlpartsco.com/ bool
        /// </summary>
        public bool DlPartsCoCom { get; set; }

        /// <summary>
        /// USA parse https://cashwells.com/ bool
        /// </summary>
        public bool CashWellsCom { get; set; }

        /// <summary>
        /// USA parse https://www.repairclinic.com/ bool
        /// </summary>
        public bool RepairClinicCom { get; set; }

        /// <summary>
        /// USA parse https://www.partstown.com/ bool
        /// </summary>
        public bool PartsTownCom { get; set; }

        /// <summary>
        /// USA parse https://www.allvikingparts.com/ bool
        /// </summary>
        public bool AllVikingPartsCom { get; set; }

        /// <summary>
        /// Maximum days for delivery from Amazon.
        /// </summary>
        public int MaxDeliveryDays { get; set; }

        /// <summary>
        /// Have generated stock table or not bool.
        /// </summary>
        public bool IsHaveStockTable { get; set; }

        /// <summary>
        /// A URL of stock table
        /// </summary>
        public string StockTableUrl { get; set; }

        /// <summary>
        /// Api key for hm.ru
        /// </summary>
        public string ShortURLApiKey { get; set; }

        /// <summary>
        /// Can user user Mass Testing function or not.
        /// </summary>
        public bool CanUseMassTesting { get; set; }

    }
}
