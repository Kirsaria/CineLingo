using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineLingo.Models
{
    internal class DictionaryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string WordOrPhrase { get; set; }
        public string Fullsentence { get; set; }
        public string Translation { get; set; }
        public string SubtitleFile { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
