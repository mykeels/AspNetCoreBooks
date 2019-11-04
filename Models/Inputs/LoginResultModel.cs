using System;

namespace Books.Models
{
    public class LoginResultModel
    {
        public UserOutputModel User { get; set;}
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}