namespace PlatformAPIV2
{
    public class RegisterDto
    {
        public string Username { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public string Country { get; set; }
        
        public string Password { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }

    public class TradeRequestDto
    {
        public string Username { get; set; }
        
        public string StockSymbol { get; set; }
        
        public int Quantity { get; set; }
    }
}
