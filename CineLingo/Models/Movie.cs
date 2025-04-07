using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineLingo.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description_movie { get; set; }
        public string VideoUrl { get; set; }
        public string PosterUrl { get; set; }
        public string Level_en { get; set; }
    }

    public class SubtitleInfo
    {
        public string Language { get; set; }
        public string FilePath { get; set; }
    }
}
