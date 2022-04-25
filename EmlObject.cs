using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfVtImgLib.eml
{
    public class EmlObject
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public List<EmlContent> Content { get; set; }

        public EmlObject()
        {
            Content = new List<EmlContent>();
        }
    }
}
