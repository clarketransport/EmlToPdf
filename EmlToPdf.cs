using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace WcfVtImgLib.eml
{
    public class EmlToPdf
    {
        const string MatchEmailPattern =
          @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
          + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
          + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
          + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";

        const string MatchUrlPattern = @"(http(s)?://)?([\w-]+\.)+[\w-]+[.com]+(/[/?%&=]*)?";
        const string FR = "From:";
        const string SD = "Sent:";
        const string DT = "Date:";
        const string TO = "To:";
        const string CC = "Cc:";
        const string SJ = "Subject";

        float left_x = 25;//30;
        float right_x = 585;//760
        float top_y = 760;//580

        float pos_x = 25f;
        float pos_y = 760f;
        float line_gap = 13f;
        float page_wd = 25f;
        float page_ht = 760f;

        Font ft_N;
        //Font ft_B;

        int line_no = 0;
        bool gray = true;

        public byte[] CreatePdf(List<EmlObject> emls)
        {
            byte[] bytes = null;
            ft_N = FontFactory.GetFont(FontFactory.HELVETICA, 11f, Font.NORMAL, BaseColor.BLACK);
            //ft_B = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11f, Font.NORMAL | Font.BOLD, BaseColor.BLACK);
            PdfContentByte cb;
            float y = top_y;

            Document pdfDoc = new Document(PageSize.LETTER, 10f, 10f, 10f, 0f);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                pdfDoc.Open();
                cb = writer.DirectContent;
                page_wd = pdfDoc.PageSize.Width - 50;
                page_ht = pdfDoc.PageSize.Height - 50;

                pdfDoc.NewPage();
                SetEmls(pdfDoc, cb, emls);

                pdfDoc.Close();
                bytes = memoryStream.ToArray();
                memoryStream.Close();
            }
            return bytes;
        }

        private void SetEmls(Document pdfDoc, PdfContentByte cb, List<EmlObject> emls)
        {
            foreach (EmlObject eml in emls)
            {
                SetEml(pdfDoc, cb, eml);
            }
        }

        private void SetEml(Document pdfDoc, PdfContentByte cb, EmlObject eml)
        {
            // From
            PdfBasicFun.ShowLeftText(cb, FR, ft_N, left_x, pos_y);
            if (gray)
                PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
            gray = !gray;
            SetText(pdfDoc, cb, eml.From);
            pos_y = pos_y - line_gap;

            // Date:
            PdfBasicFun.ShowLeftText(cb, DT, ft_N, left_x, pos_y);
            PdfBasicFun.ShowLeftText(cb, eml.Date, ft_N, left_x, pos_y);
            if (gray)
                PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
            gray = !gray;
            pos_y = pos_y - line_gap;

            // To
            PdfBasicFun.ShowLeftText(cb, TO, ft_N, left_x, pos_y);
            string[] toList = eml.To.Split(';');
            if (gray)
                PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
            gray = !gray;
            SetText(pdfDoc, cb, toList[0]);
            pos_y = pos_y - line_gap;
            for (int i = 1; i < toList.Length; i++)
            {
                string strTo = toList[i];
                if (gray)
                    PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
                gray = !gray;
                SetText(pdfDoc, cb, strTo);
                pos_y = pos_y - line_gap;
            }

            // CC
            if (eml.Cc != null && eml.Cc.Length > 0)
            {
                string ccc = eml.Cc.Substring(0, 3);
                PdfBasicFun.ShowLeftText(cb, ccc, ft_N, left_x, pos_y);
                string[] ccList = eml.Cc.Split(';');
                if (gray)
                    PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
                gray = !gray;
                SetText(pdfDoc, cb, ccList[0]);
                pos_y = pos_y - line_gap;
                for (int i = 1; i < toList.Length; i++)
                {
                    string strCc = ccList[i];
                    if (gray)
                        PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap - 1);
                    gray = !gray;
                    SetText(pdfDoc, cb, strCc);
                    pos_y = pos_y - line_gap;
                }
            }

            // Subject
            //if (gray)
            //    PdfBasicFun.DrawLiteGrayFrame(cb, left_x - 2, right_x + 2, pos_y - 1, pos_y - line_gap);
            //gray = !gray;
            PdfBasicFun.ShowLeftText(cb, SJ, ft_N, left_x, pos_y);
            PdfBasicFun.ShowLeftText(cb, eml.Subject, ft_N, left_x, pos_y);
            PdfBasicFun.DrawGrayHorizontalLine(cb, left_x - 2, right_x + 2, top_y + line_gap);
            PdfBasicFun.DrawGrayHorizontalLine(cb, left_x - 2, right_x + 2, pos_y - 3);
            PdfBasicFun.DrawGrayVerticalLine(cb, left_x - 2, top_y + line_gap, pos_y - 3);
            PdfBasicFun.DrawGrayVerticalLine(cb, right_x + 2, top_y + line_gap, pos_y - 3);
            PdfBasicFun.DrawBlackLine(cb, left_x - 2, right_x + 2, pos_y - 8);
            pos_y = pos_y - line_gap;
            pos_y = pos_y - line_gap;

            SetContent(pdfDoc, cb, eml);
        }

        private void SetContent(Document pdfDoc, PdfContentByte cb, EmlObject eml)
        {
            foreach (EmlContent content in eml.Content)
            {
                if (content.ContentType.Contains(EmlCType.TEXT_PLAIN))
                {
                    SetTextContent(pdfDoc, cb, content.Texts);
                }
                else if (content.ContentType.Contains(EmlCType.TEXT_HTML))
                {
                    SetHtmlContent(pdfDoc, cb, content.Html);
                }
                else if (content.ContentType.Contains(EmlCType.IMG))
                {
                    SetImageContent(pdfDoc, cb, content.Data);
                }
            }
        }

        private void SetTextContent(Document pdfDoc, PdfContentByte cb, List<string> Texts)
        {
            int chunkSize = 110; // change 110 with the size of strings you want.
            foreach (string text in Texts)
            {
                if (pos_y <= 10f)
                {
                    pdfDoc.NewPage();
                    pos_y = top_y;
                }
                if (text.Length <= chunkSize)
                {
                    //PdfBasicFun.ShowLeftText(cb, text, ft_N, left_x, pos_y);
                    SetText(pdfDoc, cb, text);
                    pos_y = pos_y - line_gap;
                }
                else
                {
                    List<string> multiLines = new List<string>();
                    StringBuilder sb = new StringBuilder();
                    string[] newSplit = text.Split(' ');
                    foreach (string str in newSplit)
                    {
                        if (sb.Length != 0)
                            sb.Append(" ");

                        if (sb.Length + str.Length > chunkSize)
                        {
                            multiLines.Add(sb.ToString());
                            sb.Clear();
                        }
                        sb.Append(str);
                    }
                    multiLines.Add(sb.ToString());

                    foreach (string line in multiLines)
                    {
                        if (pos_y <= 10f)
                        {
                            pdfDoc.NewPage();
                            pos_y = top_y;
                        }
                        //PdfBasicFun.ShowLeftText(cb, line, ft_N, left_x, pos_y);
                        SetText(pdfDoc, cb, line);
                        pos_y = pos_y - line_gap;
                    }
                }
            }
        }

        private void SetHtmlContent(Document pdfDoc, PdfContentByte cb, string html)
        {
            //string[] lines = html.Split('\n');
            string[] lines = html.Split('\r');
            List<string> Texts = new List<string>(lines);
            SetTextContent(pdfDoc, cb, Texts);
        }

        private void SetImageContent(Document pdfDoc, PdfContentByte cb, string imgData)
        {
            byte[] bts = Convert.FromBase64String(imgData);
            MemoryStream ms = new MemoryStream(bts);
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms);
            if (IsTranslucent(bitmap))
            {

            }
            else
            {
                bitmap.MakeTransparent(System.Drawing.Color.White);
            }
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Png);
            img.ScalePercent(72f / img.DpiX * 100);
            if (img.Width > page_wd)
            {
                var imgHt = img.Height * page_wd / img.Width;
                if (pos_y - imgHt < 10f)
                {
                    pdfDoc.NewPage();
                    pos_y = top_y;
                }
                pos_y = pos_y - imgHt;
                img.SetAbsolutePosition(left_x, pos_y);
                img.ScaleToFit(page_wd, imgHt);
            }
            else
            {
                if (pos_y - img.Height < 10f)
                {
                    pdfDoc.NewPage();
                    pos_y = top_y;
                }
                pos_y = pos_y - img.Height;
                img.SetAbsolutePosition(left_x, pos_y);
            }
            cb.AddImage(img);
        }

        private void SetText(Document pdfDoc, PdfContentByte cb, string text)
        {
            if (text.Contains("@"))
            {
                SetBlakcBlueLine(pdfDoc, cb, text, MatchEmailPattern, 0);
            }
            else
            {
                SetBlakcBlueLine(pdfDoc, cb, text, MatchUrlPattern, 1);
            }
        }

        private void SetBlakcBlueLine(Document pdfDoc, PdfContentByte cb, string text, string matchlPattern, int type)
        {
            Font black = FontFactory.GetFont(FontFactory.HELVETICA, 11f, Font.NORMAL, BaseColor.BLACK);
            Font blue = FontFactory.GetFont(FontFactory.HELVETICA, 11f, Font.NORMAL | Font.UNDEFINED, BaseColor.BLUE);

            string textb = text;

            Regex rx = new Regex(matchlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(text);

            Paragraph pgrf = new Paragraph();
            if (matches.Count > 0)
            {
                // Report on each match.
                foreach (Match match in matches)
                {
                    string data = match.Value.ToString();
                    int st_pos = textb.IndexOf(data);
                    string datab = textb.Substring(0, st_pos);
                    if (type == 1 && datab == datab.TrimEnd())
                        datab = datab + " ";
                    Chunk blackText = new Chunk(datab, black);
                    pgrf.Add(blackText);
                    Chunk blueText = new Chunk(data, blue);
                    pgrf.Add(blueText);
                    if (textb.Length > st_pos + data.Length)
                    {
                        textb = textb.Substring(st_pos + data.Length);
                    }
                    else
                    {
                        textb = "";
                    }
                }
                if (textb.Length > 0)
                {
                    Chunk blackText = new Chunk(textb, black);
                    pgrf.Add(blackText);
                }
            }
            else
            {
                Chunk blackText = new Chunk(textb, black);
                pgrf.Add(blackText);
            }
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, pgrf, left_x, pos_y, 0);
        }

        public bool IsTranslucent(System.Drawing.Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    if (bitmap.GetPixel(x, y).A != 255)
                        return false;
            return true;
        }
    }
}
