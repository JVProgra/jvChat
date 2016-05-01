using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvChatServer.Core.Users
{
    public enum AccountLevel
    {
        Basic = 0,
        Moderator,
        Administrator,
        Owner
    }

    class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public AccountLevel Level { get; set; }
    }
}
