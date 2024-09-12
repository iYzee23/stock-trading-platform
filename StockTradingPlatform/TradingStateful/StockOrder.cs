using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStateful
{
    public class StockOrder
    {
        public string OrderId { get; set; }
        
        public string Username { get; set; }
        
        public string StockSymbol { get; set; }
        
        public int Quantity { get; set; }
        
        public string OrderType { get; set; }
    }
}
