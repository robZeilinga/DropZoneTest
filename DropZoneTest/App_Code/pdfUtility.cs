using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Text;
using iTextSharp.text;
using System.IO;

/// <summary>
/// Summary description for pdfUtility
/// </summary>
/// 
public static class pdfUtility
{
    /*
    public static processedItems ExtractTextFromPdf(string path, DateTime endDate)
    {
        processedItems pi = new processedItems();

        List<interestItem> items = new List<interestItem>();

        using (PdfReader reader = new PdfReader(path))
        {
            StringBuilder text = new StringBuilder();

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                string thePage = PdfTextExtractor.GetTextFromPage(reader, i);
                thePage = Regex.Replace(thePage, @"\r\n?|\n", "~");
                text.Append("--------------------------------------------------------------------------------------");
                text.Append(thePage);

                if (thePage.Contains("INTEREST"))
                {
                    string Year = "";
                    string StmntNum = "";
                    if (thePage.Contains("Statement No"))
                    {
                        int pos = thePage.IndexOf("Statement No");
                        int pos2 = thePage.IndexOf("~", pos);
                        string[] num = thePage.Substring(pos, pos2 - pos).Trim().Split(new char[] { ' ' });
                        StmntNum = num[2];
                        //Console.WriteLine(" ====> " + num[2]);
                    }
                    if (thePage.Contains("Statement from "))
                    {
                        int pos = thePage.IndexOf("Statement from ");
                        int pos2 = thePage.IndexOf(" to ", pos);
                        string[] num = thePage.Substring(pos, pos2 - pos).Trim().Split(new char[] { ' ' });
                        Year = num[4];
                        //Console.WriteLine(" ====> " + num[4]);
                    }
                    bool found = true;
                    while (found)
                    {
                        int pos = thePage.IndexOf("INTEREST ON OVERDRAFT");
                        if (pos == -1)
                        {
                            found = false;
                            break;
                        }
                        int pos2 = thePage.IndexOf("-", pos);
                        int pos3 = thePage.IndexOf("~", pos2);

                        string line = thePage.Substring(pos, pos3 - pos + 1);

                        thePage = thePage.Substring(pos2);
                        line = Regex.Replace(line, @"\r\n?|\n", " ");
                        //                            line = line.Replace("\\n", " ");
                        items.Add(new interestItem(line, Year, StmntNum));

                        //                            retVal.Add(line);
                    }
                }
                // now check for end Balance 
                if (thePage.Contains("Statement from "))
                {
                    int pos = thePage.IndexOf("Statement from ");
                    int pos2 = thePage.IndexOf("~", pos);
//                  Statement from 14 January 2017 to 13 February 2017
                    string[] num = thePage.Substring(pos, pos2 - pos).Trim().Split(new char[] { ' ' });

                    int Year = int.Parse(num[4]);

                    //Console.WriteLine(" ====> " + num[4]);
                }

                //Statement from 14 January 2017 to 13 February 2017



            }
            pi.contents = text.ToString();
            pi.Items = items;
            return pi;
        }
    }
    */

    public static List<StatementHeader> ExtractStatementsFromPdf(string path)
    {

        List<StatementHeader> statements = new List<StatementHeader>();

        using (PdfReader reader = new PdfReader(path))
        {
            StringBuilder text = new StringBuilder();
            DateTime start = DateTime.Now;

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                string thePage = "~" + PdfTextExtractor.GetTextFromPage(reader, i);   // add leading newLine 
                thePage = Regex.Replace(thePage, @"\r\n?|\n", "~");
                text.Append(thePage);
                try
                {
                    if(statements.Count == 28)
                    {
                        Console.WriteLine("debug now!");
                    }
                    StatementHeader sh = new StatementHeader(thePage);
                    if (sh.AccNumber != null) statements.Add(sh);
                }
                catch (Exception e)
                {
                    Console.WriteLine("error - " + e.Message);
                }
            }
            DateTime end = DateTime.Now;

            TimeSpan ts = end - start;
            //Console.WriteLine(" elasped Time : " + ts.TotalMilliseconds);

        }
        return statements;
    }

    public static decimal getDecimal(string instring)
    {
        bool neg = false;
        instring = instring.Trim();
        // start with R ? then trim it 
        if (instring.StartsWith("R")) instring = instring.Substring(1).Trim();
        // end with "-" minus? 
        if (instring.EndsWith("-"))
        {
            instring = instring.Substring(0, instring.Length - 1);
            neg = true;
        }
        // fix puctuation 
        instring = instring.Replace(".", "");
        instring = instring.Replace(",", ".");
        decimal retVal = decimal.Parse(instring);
        if (neg) retVal = retVal * -1;
        return retVal;
    }


    public static void genPDF_org(string pathToFile, decimal interestPaid, decimal balanceAtEnd, DateTime startDate, DateTime endDate, List<StatementHeader> stmnts, string region, string name, string num)
    {
        System.IO.FileStream fs = new FileStream(pathToFile, FileMode.OpenOrCreate);

        Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);

        pdfDoc.Open();

        #region logic


        BaseFont bf = FontFactory.GetFont(FontFactory.HELVETICA).GetCalculatedBaseFont(false);
        BaseFont bf_bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD).GetCalculatedBaseFont(false);

        PdfContentByte cb = writer.DirectContent;

        iTextSharp.text.Font logoFont = FontFactory.GetFont("Segoe UI", 22, Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.DarkBlue));
        pdfDoc.Add(new Paragraph("  Standard Bank", logoFont));
        iTextSharp.text.Font headingFont = FontFactory.GetFont("Segoe UI", 16, Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.DarkBlue));
        iTextSharp.text.Font fillInFont = FontFactory.GetFont("Segoe UI", 16, Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.Black));
        // step 4: we grab the ContentByte and do some stuff with it

        int blueFontSize = 7;
        int blackFontSize = 8;


        // we tell the ContentByte we're ready to draw text
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(400, 780);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 15);
        cb.ShowText("Certificate of Interest");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();

        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(370, 755);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Date (YYYY-MM-DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(445, 750);
        cb.LineTo(550, 750);
        cb.Stroke();
        // fill in Date 
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(455, 755);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        cb.ShowText(DateTime.Now.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();


        //Region  SW Southern Cape Business Centre     
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(370, 730);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Region");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(395, 725);
        cb.LineTo(550, 725);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(400, 730);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText("SW Southern Cape Business Centre");
        cb.ShowText(region);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        // Account in the Name of  -------------------------------------------------
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(38, 690);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Account in the Name of");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(116, 685);
        cb.LineTo(550, 685);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(120, 690);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText("Bob auto mac Business Centre");
        cb.ShowText(stmnts.First().CompanyName);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //We confirm that the     Credit balance of the above customer’s Enterprise Autobank PlusPlan  --------------------------------

        cb.BeginText();
        cb.SetTextMatrix(38, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("We confirm that the");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(105, 660);
        cb.LineTo(160, 660);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(165, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("balance of the above customer’s");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(270, 660);
        cb.LineTo(550, 660);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(120, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);

        cb.ShowText(balanceAtEnd > 0 ? "Credit" : "Debit");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //BUSINESS CURRENT ACCOUNT
        cb.BeginText();
        cb.SetTextMatrix(280, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText("BUSINESS CURRENT ACCOUNT");
        cb.ShowText(stmnts.First().AccountType);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //Account Number ----------------------------------------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 640);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Account Number");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(95, 635);
        cb.LineTo(550, 635);
        cb.Stroke();

        // 082755043 
        cb.BeginText();
        cb.SetTextMatrix(100, 640);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText(" 082755043 ");
        cb.ShowText(stmnts.First().AccNumber);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //in our books as at date(YYYY - MM - DD)  2016 - 02 - 29     amounted to R  109.64  ---------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("in our books as at date (YYYY - MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(171, 610);
        cb.LineTo(245, 610);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(250, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("amounted to R");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(300, 610);
        cb.LineTo(550, 610);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(175, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText(" YYYY - MM - DD");
        cb.ShowText(endDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //BUSINESS CURRENT ACCOUNT
        cb.BeginText();
        cb.SetTextMatrix(310, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText("129,234.95");
        cb.ShowText(String.Format("{0:N2}", balanceAtEnd));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //(amount in words)  One Hundred and Nine Rand and Sixty Four Cents  -----------------
        cb.BeginText();
        cb.SetTextMatrix(38, 590);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("(amount in words)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(100, 585);
        cb.LineTo(550, 585);
        cb.Stroke();

        // 082755043 
        cb.BeginText();
        cb.SetTextMatrix(105, 590);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText(" One Hundred and Twenty Nine Thousand, Four Hundred and Thirty Six Rand and Twenty Eight Cents");
        cb.ShowText(NumbersToWords.Convert(balanceAtEnd));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //Interest paid or accrued from date (YYYY - MM - DD)  2015 - 03 - 01     to date(YYYY-MM - DD)  2016 - 02 - 29   ----------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Interest paid or accrued from date (YYYY - MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(205, 560);
        cb.LineTo(300, 560);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(300, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("to date(YYYY-MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(380, 560);
        cb.LineTo(550, 560);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(210, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText("YYYY - MM - DD");
        cb.ShowText(startDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(385, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText("YYYY - MM - DD");
        cb.ShowText(endDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //amounted to R  0.00     (amount in words)   -----------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("amounted to R");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(88, 535);
        cb.LineTo(160, 535);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(162, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("(amount in words)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(223, 535);
        cb.LineTo(550, 535);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(92, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //        cb.ShowText("1,234.67");
        cb.ShowText(String.Format("{0:N2}", interestPaid));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(227, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText("One Thousand, Two Hundred and Thirty Four Rand and Sixty Seven Cents");
        cb.ShowText(NumbersToWords.Convert(interestPaid));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //=====================================================================================
        //=====================================================================================
        //=====================================================================================

        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.MoveTo(38, 520);
        cb.LineTo(550, 520);
        cb.Stroke();
        cb.MoveTo(38, 519);
        cb.LineTo(550, 519);
        cb.Stroke();
        cb.MoveTo(38, 518);
        cb.LineTo(550, 518);
        cb.Stroke();

        //Consultant details  -------------------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 505);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, blueFontSize + 3);
        cb.ShowText("Consultant details");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();
        //Name  Musa Baloyi         Personnel number  A206879   ----------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Name");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(62, 460);
        cb.LineTo(355, 460);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(70, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText("Robert Zeilinga");
        cb.ShowText(name);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(360, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Personnel number");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(425, 460);
        cb.LineTo(550, 460);
        cb.Stroke();


        cb.BeginText();
        cb.SetTextMatrix(435, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.Black));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText("A142337");
        cb.ShowText(num);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.Black));
        cb.EndText();

        //<b>Standard Bank Operations</b>  6 Simmonds street Johannesburg 2001 
        // PO Box 61690 Marshalltown 2107 South Africa <href>www.standardbank.co.za</href>
        // Customer Contact Centre: 0860 123 000
        cb.BeginText();
        cb.SetTextMatrix(38, 440);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, blueFontSize);
        cb.ShowText("Standard Bank Operations");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(133, 440);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("6 Simmonds street Johannesburg 2001");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 430);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("PO Box 61690 Marshalltown 2107 South Africa ");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 420);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Customer Contact Centre: 0860 123 000");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // --------------------------------------------------------------------------

        cb.BeginText();
        cb.SetTextMatrix(38, 320);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 7);
        cb.ShowText("Please consider the clause that follows carefully as it limits the bank’s liability and constitutes an assumption of risk by you.");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 312);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("If a balance certificate has been given for the current and previous business day, we cannot guarantee the correctness of the balances on the accounts shown on the");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 304);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("certificate. The amounts shown on the balance certificate may change because all transactions on an account may not be available at the time the balance certificate is");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 296);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("issued.  We are not responsible for any claims, losses or damages of any kind, including without limitation, any direct, indirect, special or consequential damages, which");

        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 288);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("may arise as a results of actions taken in with the information on the balance of certificates.");

        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();


        // footer line =======================================================================================

        int x = 90;

        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.MoveTo(38, x);
        cb.LineTo(550, x);
        cb.Stroke();
        cb.MoveTo(38, x - 1);
        cb.LineTo(550, x - 1);
        cb.Stroke();
        cb.MoveTo(38, x - 2);
        cb.LineTo(550, x - 2);
        cb.Stroke();

        // form Number 

        x = x - 15;
        int sub = 9;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 10);
        cb.ShowText("00151836");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(90, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 9);
        cb.ShowText("2013 - 06");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // the standard bank group 
        x = x - sub - 2;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("The Standard Bank of South Africa Limited(Reg.No. 1962 / 000738 / 06).Authorised financial services and registered credit provider(NCRCP15). ");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors
        x = x - sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 7);
        cb.ShowText("Directors:");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors
        //x = 85;
        cb.BeginText();
        cb.SetTextMatrix(75, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText(" T M F Phaswana(Chairman), S K Tshabalala * (Chief Executive), D D B Band, R M W Dunne#, T S Gcabashe, K P Kalyan, B J  Kruger*, S J Macozoma");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors  2
        x = x - sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("Adv K D Moroka, A C Nissen, S P Ridley*, M J D Ruck, Lord Smith of Kelvin, Kt#, P D Sullivan##, P G Wharton-Hood*, E M Woods");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();
        // Directors  2
        x = x - sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("Secretary: Z Stephen         *Executive Director    #British     ##Australian     03/06/2013");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();



        //00151836 2013 - 06  The Standard Bank of South Africa Limited(Reg.No. 1962 / 000738 / 06).Authorised financial services and registered credit provider(NCRCP15). Directors: T M F Phaswana(Chairman), S K Tshabalala * (Chief Executive), D D B Band, R M W Dunne#, T S Gcabashe, K P Kalyan, B J Kruger*, S J Macozoma Adv K D Moroka, A C Nissen, S P Ridley*, M J D Ruck, Lord Smith of Kelvin, Kt#, P D Sullivan##, P G Wharton-Hood*, E M Woods Secretary: Z Stephen         *Executive Director    #British     ##Australian     03/06/2013 


        //pdfDoc.Add(new Paragraph(""));
        //iTextSharp.text.Font secondFont = FontFactory.GetFont("Segoe UI", 12, new iTextSharp.text.BaseColor(System.Drawing.Color.Black));
        //pdfDoc.Add(new Paragraph("Interest Paid : " + String.Format("{0:N2}", interestPaid), secondFont));
        //pdfDoc.Add(new Paragraph("Final Balance : " + String.Format("{0:N2}", balanceAtEnd), secondFont));
        //        PdfContentByte cb = writer.DirectContent;
        //        cb.MoveTo(pdfDoc.PageSize.Width / 2, pdfDoc.PageSize.Height / 2);
        //        cb.LineTo(pdfDoc.PageSize.Width / 2, pdfDoc.PageSize.Height);
        //        cb.Stroke();

        #endregion

        pdfDoc.Close();
        // Close the writer instance

        writer.Close();
        // Always close open filehandles explicity
        fs.Close();
    }

    public static void genPDF(string pathToFile, decimal interestPaid, decimal balanceAtEnd, DateTime startDate, DateTime endDate,  List<StatementHeader> stmnts, string region, string name, string num )
    {
        System.IO.FileStream fs = new FileStream(pathToFile, FileMode.OpenOrCreate);

        Document pdfDoc = new Document(PageSize.A4, 25, 25, 30, 30);
        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);

        pdfDoc.Open();

        #region logic


        BaseFont bf = FontFactory.GetFont(FontFactory.HELVETICA).GetCalculatedBaseFont(false);
        BaseFont bf_bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD).GetCalculatedBaseFont(false);

        PdfContentByte cb = writer.DirectContent;

        iTextSharp.text.Font logoFont = FontFactory.GetFont("Segoe UI", 22, Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.DarkBlue));
        //pdfDoc.Add(new Paragraph("  Standard Bank", logoFont));
        iTextSharp.text.Font headingFont = FontFactory.GetFont("Segoe UI", 16, Font.BOLD, new iTextSharp.text.BaseColor(System.Drawing.Color.DarkBlue));
        iTextSharp.text.Font fillInFont = FontFactory.GetFont("Segoe UI", 16, Font.NORMAL, new iTextSharp.text.BaseColor(System.Drawing.Color.DarkBlue));
        // step 4: we grab the ContentByte and do some stuff with it

        int blueFontSize = 7;
        int blackFontSize = 8;


        string imgPath = HttpContext.Current.Server.MapPath("~/Images/");

        Image jpg = Image.GetInstance(imgPath + "SBSA_Logo2.jpg");
        jpg.SetAbsolutePosition(38, 770);
        jpg.ScalePercent(65f);

        pdfDoc.Add(jpg);


        // we tell the ContentByte we're ready to draw text
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(450, 790);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 12);
        cb.ShowText("Certificate of Interest");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();

        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(370, 755);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Date (YYYY-MM-DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(445, 750);
        cb.LineTo(550, 750);
        cb.Stroke();
        // fill in Date 
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(455, 755);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
        cb.ShowText(DateTime.Now.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();


        //Region  SW Southern Cape Business Centre     
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(370, 730);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Region");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(395, 725);
        cb.LineTo(550, 725);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(400, 730);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText("SW Southern Cape Business Centre");
        cb.ShowText(region);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Account in the Name of  -------------------------------------------------
        cb.BeginText();
        // we draw some text on a certain position
        cb.SetTextMatrix(38, 690);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Account in the Name of");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        // we tell the contentByte, we've finished drawing text
        cb.EndText();
        cb.MoveTo(116, 685);
        cb.LineTo(550, 685);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(120, 690);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText("Bob auto mac Business Centre");
        cb.ShowText(stmnts.First().CompanyName);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //We confirm that the     Credit balance of the above customer’s Enterprise Autobank PlusPlan  --------------------------------

        cb.BeginText();
        cb.SetTextMatrix(38, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("We confirm that the");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(105, 660);
        cb.LineTo(160, 660);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(165, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("balance of the above customer’s");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(270, 660);
        cb.LineTo(550, 660);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(120, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);

        cb.ShowText( balanceAtEnd>0 ? "Credit" : "Debit");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //BUSINESS CURRENT ACCOUNT
        cb.BeginText();
        cb.SetTextMatrix(280, 665);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);

//        cb.ShowText("BUSINESS CURRENT ACCOUNT");
        cb.ShowText(stmnts.First().AccountType);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //Account Number ----------------------------------------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 640);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Account Number");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(95, 635);
        cb.LineTo(550, 635);
        cb.Stroke();

        // 082755043 
        cb.BeginText();
        cb.SetTextMatrix(100, 640);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText(" 082755043 ");
        cb.ShowText(stmnts.First().AccNumber);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //in our books as at date(YYYY - MM - DD)  2016 - 02 - 29     amounted to R  109.64  ---------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("in our books as at date (YYYY - MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(171, 610);
        cb.LineTo(245, 610);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(250, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("amounted to R");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(300, 610);
        cb.LineTo(550, 610);
        cb.Stroke();

        // we draw some text on a certain position
        cb.BeginText();
        cb.SetTextMatrix(175, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText(" YYYY - MM - DD");
        cb.ShowText(endDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //BUSINESS CURRENT ACCOUNT
        cb.BeginText();
        cb.SetTextMatrix(310, 615);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText("129,234.95");
        cb.ShowText(String.Format("{0:N2}", balanceAtEnd));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //(amount in words)  One Hundred and Nine Rand and Sixty Four Cents  -----------------
        cb.BeginText();
        cb.SetTextMatrix(38, 590);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("(amount in words)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(100, 585);
        cb.LineTo(550, 585);
        cb.Stroke();

        // 082755043 
        cb.BeginText();
        cb.SetTextMatrix(105, 590);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);

        //        cb.ShowText(" One Hundred and Twenty Nine Thousand, Four Hundred and Thirty Six Rand and Twenty Eight Cents");
        cb.ShowText(NumbersToWords.Convert(balanceAtEnd));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //Interest paid or accrued from date (YYYY - MM - DD)  2015 - 03 - 01     to date(YYYY-MM - DD)  2016 - 02 - 29   ----------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Interest paid or accrued from date (YYYY - MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(205, 560);
        cb.LineTo(300, 560);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(300, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("to date(YYYY-MM - DD)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(380, 560);
        cb.LineTo(550, 560);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(210, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText("YYYY - MM - DD");
        cb.ShowText(startDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(385, 565);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText("YYYY - MM - DD");
        cb.ShowText(endDate.ToString("yyyy-MM-dd"));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //amounted to R  0.00     (amount in words)   -----------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("amounted to R");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(88, 535);
        cb.LineTo(160, 535);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(162, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("(amount in words)");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(223, 535);
        cb.LineTo(550, 535);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(92, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
//        cb.ShowText("1,234.67");
        cb.ShowText(String.Format("{0:N2}",interestPaid));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(227, 540);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);

//        cb.ShowText("One Thousand, Two Hundred and Thirty Four Rand and Sixty Seven Cents");
        cb.ShowText(NumbersToWords.Convert(interestPaid));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //=====================================================================================
        //=====================================================================================
        //=====================================================================================

        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.MoveTo(38, 520);
        cb.LineTo(550, 520);
        cb.Stroke();
        cb.MoveTo(38, 519);
        cb.LineTo(550, 519);
        cb.Stroke();
        cb.MoveTo(38, 518);
        cb.LineTo(550, 518);
        cb.Stroke();

        //Consultant details  -------------------------------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 505);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, blueFontSize + 3);
        cb.ShowText("Consultant details");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();
        //Name  Musa Baloyi         Personnel number  A206879   ----------------------------
        cb.BeginText();
        cb.SetTextMatrix(38, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Name");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(62, 460);
        cb.LineTo(355, 460);
        cb.Stroke();

        cb.BeginText();
        cb.SetTextMatrix(70, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText("Robert Zeilinga");
        cb.ShowText(name);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(360, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Personnel number");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.MoveTo(425, 460);
        cb.LineTo(550, 460);
        cb.Stroke();


        cb.BeginText();
        cb.SetTextMatrix(435, 465);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blackFontSize);
        //cb.ShowText("A142337");
        cb.ShowText(num);
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        //<b>Standard Bank Operations</b>  6 Simmonds street Johannesburg 2001 
        // PO Box 61690 Marshalltown 2107 South Africa <href>www.standardbank.co.za</href>
        // Customer Contact Centre: 0860 123 000
        cb.BeginText();
        cb.SetTextMatrix(38, 440);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, blueFontSize);
        cb.ShowText("Standard Bank Operations");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(133, 440);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("6 Simmonds street Johannesburg 2001");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 430);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("PO Box 61690 Marshalltown 2107 South Africa ");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 420);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.ShowText("Customer Contact Centre: 0860 123 000");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // --------------------------------------------------------------------------

        cb.BeginText();
        cb.SetTextMatrix(38, 320);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 7);
        cb.ShowText("Please consider the clause that follows carefully as it limits the bank’s liability and constitutes an assumption of risk by you.");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 312);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("If a balance certificate has been given for the current and previous business day, we cannot guarantee the correctness of the balances on the accounts shown on the");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 304);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("certificate. The amounts shown on the balance certificate may change because all transactions on an account may not be available at the time the balance certificate is");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 296);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("issued.  We are not responsible for any claims, losses or damages of any kind, including without limitation, any direct, indirect, special or consequential damages, which");

        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(38, 288);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("may arise as a results of actions taken in with the information on the balance of certificates.");

        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();


        // footer line =======================================================================================

        int x = 90;

        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, blueFontSize);
        cb.MoveTo(38, x);
        cb.LineTo(550, x);
        cb.Stroke();
        cb.MoveTo(38, x-1);
        cb.LineTo(550, x-1);
        cb.Stroke();
        cb.MoveTo(38, x-2);
        cb.LineTo(550, x-2);
        cb.Stroke();

        // form Number 

        x = x-15;
        int sub = 9;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 10);
        cb.ShowText("00151836");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        cb.BeginText();
        cb.SetTextMatrix(90, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 9);
        cb.ShowText("2013 - 06");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // the standard bank group 
        x = x - sub-2;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("The Standard Bank of South Africa Limited(Reg.No. 1962 / 000738 / 06).Authorised financial services and registered credit provider(NCRCP15). ");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors
        x = x-sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf_bold, 7);
        cb.ShowText("Directors:");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors
        //x = 85;
        cb.BeginText();
        cb.SetTextMatrix(75, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText(" T M F Phaswana(Chairman), S K Tshabalala * (Chief Executive), D D B Band, R M W Dunne#, T S Gcabashe, K P Kalyan, B J  Kruger*, S J Macozoma");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();

        // Directors  2
        x = x - sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("Adv K D Moroka, A C Nissen, S P Ridley*, M J D Ruck, Lord Smith of Kelvin, Kt#, P D Sullivan##, P G Wharton-Hood*, E M Woods");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();
        // Directors  2
        x = x - sub;
        cb.BeginText();
        cb.SetTextMatrix(38, x);
        cb.SetColorFill(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.SetFontAndSize(bf, 7);
        cb.ShowText("Secretary: Z Stephen         *Executive Director    #British     ##Australian     03/06/2013");
        cb.SetColorStroke(new BaseColor(System.Drawing.Color.DarkBlue));
        cb.EndText();



        //00151836 2013 - 06  The Standard Bank of South Africa Limited(Reg.No. 1962 / 000738 / 06).Authorised financial services and registered credit provider(NCRCP15). Directors: T M F Phaswana(Chairman), S K Tshabalala * (Chief Executive), D D B Band, R M W Dunne#, T S Gcabashe, K P Kalyan, B J Kruger*, S J Macozoma Adv K D Moroka, A C Nissen, S P Ridley*, M J D Ruck, Lord Smith of Kelvin, Kt#, P D Sullivan##, P G Wharton-Hood*, E M Woods Secretary: Z Stephen         *Executive Director    #British     ##Australian     03/06/2013 


        //pdfDoc.Add(new Paragraph(""));
        //iTextSharp.text.Font secondFont = FontFactory.GetFont("Segoe UI", 12, new iTextSharp.text.BaseColor(System.Drawing.Color.Black));
        //pdfDoc.Add(new Paragraph("Interest Paid : " + String.Format("{0:N2}", interestPaid), secondFont));
        //pdfDoc.Add(new Paragraph("Final Balance : " + String.Format("{0:N2}", balanceAtEnd), secondFont));
        //        PdfContentByte cb = writer.DirectContent;
        //        cb.MoveTo(pdfDoc.PageSize.Width / 2, pdfDoc.PageSize.Height / 2);
        //        cb.LineTo(pdfDoc.PageSize.Width / 2, pdfDoc.PageSize.Height);
        //        cb.Stroke();

        #endregion

        pdfDoc.Close();
        // Close the writer instance

        writer.Close();
        // Always close open filehandles explicity
        fs.Close();
    }
}