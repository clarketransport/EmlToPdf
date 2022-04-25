using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfVtImgLib.eml
{
    public class EmlUtilities
    {

        public List<string> GetAllLines(byte[] buffer)
        {
            List<string> lines = new List<string>();
            byte[] pattern = new byte[2];
            pattern[0] = 13;
            pattern[1] = 10;
            List<int> int_list = IndexOfByteArray(buffer, pattern, 0);

            int fr_pos = 0;
            for (int i = 0; i < int_list.Count; i++)
            {
                int to_pos = int_list[i];
                int size = to_pos - fr_pos;
                if (size > 0)
                {
                    byte[] data = new byte[size];
                    Array.Copy(buffer, fr_pos, data, 0, size);
                    string str = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    lines.Add(str);
                }
                fr_pos = to_pos + 2;
            }
            return lines;
        }

        public List<int> IndexOfByteArray(byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }

        public string GetHtmlData(List<string> lines)
        {
            bool add_html = false;
            StringBuilder sb = new StringBuilder();
            int len = lines.Count;
            for (int i = 0; i < len; i++)
            {
                string str = lines[i];
                if (str.ToLower() == "<html>")
                {
                    sb.Append(str);
                    add_html = true;
                }
                else if (str.ToLower() == "</html>")
                {
                    sb.Append(str);
                    add_html = false;
                }
                else
                {
                    if (add_html)
                    {
                        sb.Append(str);
                    }
                }
            }
            return sb.ToString();
        }

        public string GetEncodedHtmlData(List<string> lines, string charset)
        {
            StringBuilder sb = new StringBuilder();
            int len = lines.Count;
            for (int i = 0; i < len; i++)
            {
                string str = lines[i];
                sb.Append(str);
            }
            string data = sb.ToString();
            byte[] bts = Convert.FromBase64String(data);
            string ret = data;
            if (charset.Contains("utf-8"))
                ret = System.Text.Encoding.UTF8.GetString(bts);
            else
                ret = System.Text.Encoding.Default.GetString(bts);
            return ret;
        }

        public string GetImageData(List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            int len = lines.Count;
            for (int i = 0; i < len; i++)
            {
                sb.Append(lines[i]);
            }
            return sb.ToString();
        }

        public Bitmap GetImage(string imgData)
        {
            Bitmap bit = null;
            try
            {
                byte[] bts = Convert.FromBase64String(imgData);
                MemoryStream ms = new MemoryStream(bts);
                bit = (Bitmap)Image.FromStream(ms);
            }
            catch
            { }
            return bit;
        }
    }
}
