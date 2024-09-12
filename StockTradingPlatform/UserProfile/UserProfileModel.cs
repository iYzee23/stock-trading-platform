using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserProfile
{
    public class UserProfileModel
    {
        public string Username { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public decimal Balance { get; set; }
        
        public string Country { get; set; }
        
        public string HashedPassword { get; set; }
    }
}
