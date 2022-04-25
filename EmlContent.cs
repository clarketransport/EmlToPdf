using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfVtImgLib.eml
{
    public class EmlContent
    {
        public string ContentType { get; set; } // type of content  Charset (utf-8, iso-8859-1)
        public string ContentDisposition { get; set; } // Image Size
        public string ContentTransferEncoding { get; set; } // encode method
        public List<string> Texts { get; set; } // text body
        public string Html { get; set; }        // html body
        public string Data { get; set; }        // image data
    }
}
