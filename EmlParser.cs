using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WcfVtImgLib.eml
{
    public class EmlParser
    {
        int line_pos = 0;
        int line_cnt = 0;
        List<string> lines;

        public List<EmlObject> Process(byte[] bytes)
        {
            EmlUtilities utilities = new EmlUtilities();
            lines = utilities.GetAllLines(bytes);
            List<EmlObject> emls = Process();
            return emls;
        }

        public List<EmlObject> Process()
        {
            List<EmlObject> emls = new List<EmlObject>();
            line_cnt = lines.Count;

            while (line_pos < line_cnt)
            {
                string line = lines[line_pos];
                int n = line.IndexOf(": ");
                if (n > 0)
                {
                    string header = line.Substring(0, n).ToLower();
                    if (header == "from")
                    {
                        GetObject(emls);
                    }
                }
                line_pos++;
            }
            return emls;
        }

        private void GetObject(List<EmlObject> emls)
        {
            EmlObject eml = new EmlObject();
            emls.Add(eml);

            while (line_pos < line_cnt)
            {
                string line = lines[line_pos];
                int n = line.IndexOf(": ");
                if (n > 0)
                {
                    string header = line.Substring(0, n).ToLower();
                    switch (header)
                    {
                        case "from":
                            eml.From = line;
                            break;
                        case "to":
                            eml.To = line;
                            break;
                        case "cc":
                            eml.Cc = line;
                            break;
                        case "date":
                            eml.Date = line;
                            break;
                        case "subject":
                            eml.Subject = line;
                            break;
                        case "mime-version":
                            eml.Version = line;
                            break;
                        case "content-type":
                            // to add new content
                            if (line.StartsWith(EmlCType.MULTI_PART))
                            {

                            }
                            else
                            {
                                GetContent(emls, eml);
                            }
                            break;
                        default:
                            break;
                    }
                }
                line_pos++;
            }
            return;
        }

        private void GetContent(List<EmlObject> emls, EmlObject eml)
        {
            List<string> list = new List<string>();
            EmlContent content = new EmlContent();
            eml.Content.Add(content);
            bool ct_data = false;
            bool org_data = false;

            while (line_pos < line_cnt)
            {
                string line = lines[line_pos];
                if (line.StartsWith("--_"))
                {
                    SetData(content, list);
                    break;
                }
                int n = line.IndexOf(": ");
                if (n > 0)
                {
                    string header = line.Substring(0, n).ToLower();
                    if (header == "from")
                    {
                        string preLine = lines[line_pos - 1];
                        if (preLine.Contains("Original Message"))
                        {
                            org_data = true;
                        }
                        else
                        {
                            org_data = false;
                            SetData(content, list);
                            GetObject(emls);
                        }
                    }
                    if (org_data)
                    {
                        list.Add(line);
                    }
                    else
                    {
                        switch (header)
                        {
                            case "content-type":
                                // to add new content
                                content.ContentType = line;
                                break;
                            case "content-disposition":
                                // to get size of content
                                content.ContentDisposition = line;
                                break;
                            case "content-transfer-encoding":
                                // to get encode method
                                content.ContentTransferEncoding = line;
                                ct_data = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    if (org_data)
                    {
                        list.Add(line);
                    }
                    else
                    {
                        if (ct_data)
                            list.Add(line);
                    }
                }
                line_pos++;
            }
            if (line_pos >= line_cnt) 
            {
                SetData(content, list);
            }
            return;
        }

        private void SetData(EmlContent content, List<string> list)
        {
            EmlUtilities utilities = new EmlUtilities();
            if (content.ContentType.StartsWith(EmlCType.TEXT_PLAIN))
            {
                content.Texts = list;
            }
            if (content.ContentType.StartsWith(EmlCType.TEXT_HTML))
            {
                if (content.ContentTransferEncoding.StartsWith(EmlCType.QUOTED))
                    content.Html = utilities.GetHtmlData(list);
                else
                    content.Html = utilities.GetEncodedHtmlData(list, content.ContentType);
            }
            if (content.ContentType.StartsWith(EmlCType.IMG))
            {
                content.Data = utilities.GetImageData(list);
            }
        }

        public string StripHTML(string source)
        {
            try
            {
                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&nbsp;", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");
                result = result.Replace("=M", "");
                result = result.Replace("=E0", "");
                result = result.Replace("=", "");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // That's it.
                return result;
            }
            catch
            {
                return source;
            }
        }


    }
}
