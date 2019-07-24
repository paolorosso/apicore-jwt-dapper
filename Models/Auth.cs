using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{


        public class AccessCred
        {
            public string Grant_type { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Refresh_token { get; set; }
        }

        public class Token
        {
            public string Access_token { get; set; }
            public long Expires_in { get; set; }
            public string Refresh_token { get; set; }
        }

        public class RefreshToken
        {
            public int UserId { get; set; }
            public string Refresh_token { get; set; }
            public DateTime DateIssued { get; set; }
            public DateTime DateExpires { get; set; }

        }
}
