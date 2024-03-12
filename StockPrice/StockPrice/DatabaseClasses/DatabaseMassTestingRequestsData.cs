using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPrice.DatabaseClasses
{
    public class DatabaseMassTestingRequestsData
    {
        /// <summary>
        /// ID of mass testing request
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// ID of mass testing request initiator
        /// </summary>
        public long InitiatorID { get; set; }

        /// <summary>
        /// JSON-formatted SKU list
        /// </summary>
        public string SkuList { get; set; }

        /// <summary>
        /// Bool. Work with this list is done or not.
        /// </summary>
        public bool WorkDone { get; set; }

        /// <summary>
        /// Bool. Report for owner was sended or not.
        /// </summary>
        public bool ReportSended { get; set; }
    }
}
