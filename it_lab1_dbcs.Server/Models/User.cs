using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public User() { }
        public User(string username, string passwordHash, string salt)
        {
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
