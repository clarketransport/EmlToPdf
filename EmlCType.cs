using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfVtImgLib.eml
{
    public class EmlCType
    {
        public const string MULTI_PART = "Content-Type: multipart/related";
        public const string TEXT_PLAIN = "Content-Type: text/plain";
        public const string TEXT_HTML = "Content-Type: text/html";
        public const string IMG = "Content-Type: image/";

        public const string QUOTED = "Content-Transfer-Encoding: quoted-printable";
        public const string BASE64 = "Content-Transfer-Encoding: base64";
    }
}
