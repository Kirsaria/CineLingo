using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineLingo.Models
{
    internal class User
    {
            public int Id { get; set; }
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<DictionaryItem> DictionaryItems { get; set; } = new List<DictionaryItem>();
    }
}
