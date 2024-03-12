using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPrice.Methods.DropboxClass
{
    public sealed class DropBoxTokenDTO
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public long ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string Uid { get; set; }
        public string AccountId { get; set; }
    }
}
