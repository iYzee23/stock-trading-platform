using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStateful
{
    public class TransactionRecord
    {
        public string TransactionId { get; set; }
        
        public string StockSymbol { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }
        
        public bool IsBuy { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}
