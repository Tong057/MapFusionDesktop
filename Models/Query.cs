using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapFusion.Models
{
    public class Query
    {
        public string City { get; set; }
        public string KeyWords { get; set; }

        public Query()
        {
            
        }

        public Query(string city, string keyWords)
        {
            City = city;
            KeyWords = keyWords;
        }

        public override string ToString()
        {
            return $"{City} {KeyWords}";
        }
    }
}
