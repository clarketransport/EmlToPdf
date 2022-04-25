using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WcfVtImgLib
{
    public class PdfBasicFun
    {
        static float lineWd = 0.5f;

        public static void DrawGrayHorizontalLine(PdfContentByte cb, float x1, float x2, float y1)
        {
            cb.SetColorStroke(new BaseColor(145, 145, 145));
            cb.SetLineWidth(lineWd);
            cb.MoveTo(x1, y1);
            cb.LineTo(x2, y1);
            cb.ClosePathFillStroke();
        }

        public static void DrawGrayVerticalLine(PdfContentByte cb, float x1, float y1, float y2)
        {
            cb.SetColorStroke(new BaseColor(145, 145, 145));
            cb.SetLineWidth(lineWd);
            cb.MoveTo(x1, y1);
            cb.LineTo(x1, y2);
            cb.ClosePathFillStroke();
        }

        public static void DrawLiteGrayFrame(PdfContentByte cb, float x1, float x2, float y1, float y2)
        {

            //cb.SetColorStroke(new BaseColor(145, 145, 145));
            cb.SetColorStroke(new BaseColor(235, 235, 235));
            cb.SetColorFill(new BaseColor(235, 235, 235));
            cb.MoveTo(x1, y1);
            cb.LineTo(x2, y1);
            cb.LineTo(x2, y2);
            cb.LineTo(x1, y2);
            cb.ClosePathFillStroke();
            //cb.Rectangle(x1, y1, x2 - x1, y2 - y1);
            //cb.Fill();
        }

        public static void DrawWhiteFrame(PdfContentByte cb, float x1, float x2, float y1, float y2)
        {
            cb.SetColorStroke(new BaseColor(145, 145, 145));
            cb.SetColorFill(new BaseColor(255, 255, 255));
            cb.MoveTo(x1, y1);
            cb.LineTo(x2, y1);
            cb.LineTo(x2, y2);
            cb.LineTo(x1, y2);
            cb.ClosePathFillStroke();
            //cb.Rectangle(x1, y1, x2 - x1, y2 - y1);
            //cb.Fill();
        }

        public static void DrawBlackLine(PdfContentByte cb, float x1, float x2, float y1)
        {
            cb.SetColorStroke(new BaseColor(95, 95, 95));
            cb.SetLineWidth(2.0f);
            cb.MoveTo(x1, y1);
            cb.LineTo(x2, y1);
            cb.ClosePathFillStroke();
        }

        public static void ShowLeftText(PdfContentByte cb, string mText, Font ft, float x, float y)
        {
            Chunk chunk = new Chunk(mText, ft);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(chunk), x, y, 0);
        }

        public static void ShowLeftTextM(PdfContentByte cb, string mText, Font ft, float x, float y)
        {
            string text = mText.Replace(Environment.NewLine, String.Empty).Replace("  ", String.Empty);
            Chunk chunk = new Chunk(text, ft);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(chunk), x, y, 0);
        }

        public static void ShowCenterText(PdfContentByte cb, string mText, Font ft, float x, float y)
        {
            Chunk chunk = new Chunk(mText, ft);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(chunk), x, y, 0);
        }

        public static void ShowRightText(PdfContentByte cb, string mText, Font ft, float x, float y)
        {
            Chunk chunk = new Chunk(mText, ft);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(chunk), x, y, 0);
        }

        public static void ShowLinkText(PdfContentByte cb, string mLink, float x, float y)
        {
            Font ft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8f, Font.UNDERLINE, BaseColor.BLUE);
            Anchor anchor = new Anchor(mLink, ft);
            anchor.Reference = "http://" + mLink;
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase(anchor), x, y, 0);
        }

        public static void ShowRightLinkText(PdfContentByte cb, string mLink, float x, float y)
        {
            Font ft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8f, Font.UNDERLINE, BaseColor.BLUE);
            Anchor anchor = new Anchor(mLink, ft);
            anchor.Reference = "http://" + mLink;
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, new Phrase(anchor), x, y, 0);
        }
    }
}
