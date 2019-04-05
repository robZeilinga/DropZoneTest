using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic;
using System.Diagnostics;

/// <summary>
/// Summary description for StatementLine
/// </summary>
public class StatementLine
{
    bool deb = false;

    public int Index { get; set; }
    public int PageNumber { get; set; }
    public string Narrative { get; set; }
    public bool isServiceFee { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime transactionDate { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public decimal Balance { get; set; }
    public Int64 Ref { get; set; }
    public string text { get; set; }
    public string InterestAccountNumber { get; set; }
    public  string Language { get; set; }

    private string getResX(string key)
    {
        string retVal = "";
        try
        {
            retVal = (string)HttpContext.GetGlobalResourceObject(Language, key);
        }
        catch(Exception e)
        {
            Debug.WriteLineIf(deb, "Exception getting Translation - Language [" + Language + "] key [" + key + "]");
            Debug.WriteLineIf(deb, "Exception  : " + e);
        }
        return retVal;
    }


    public StatementLine() { }
    public StatementLine(string nar, string rest, int index, int pageNum, DateTime statementDate, int previousMonth, string Language)
    {
        this.Language = Language;
        this.isServiceFee = false;
        this.Narrative = nar;
        this.text = rest;
        this.Index = index;
        this.PageNumber = pageNum;
        this.Ref = 0;


        string value = Microsoft.VisualBasic.Strings.Right(text, 9);
        this.Ref = Int64.Parse(value);

        // remove last 9 characters from text (ref) 
        this.text = text.Substring(0, text.Length - 9);
        
        //"029118:025397 17,50 489,06- 04 25 8.629,64-    SF
        //"@11,300% 17.646,58- 06 26 6.580.516,04-
        string neg = Microsoft.VisualBasic.Strings.Right(text, 1);
        //-
        //-
        this.text = text.Substring(0, text.Length - 1);
        int lPos = text.LastIndexOf(" ");

        this.Balance = pdfUtility.getDecimal(text.Substring(lPos).Trim());
        //6.580.516,04
        //8.629,64
        if (neg == "-") this.Balance = this.Balance * -1;
        // remove balance 
        //-6.580.516,04
        text = text.Substring(0, lPos).Trim();
        // day 
        lPos = text.LastIndexOf(" ");
        this.Day = int.Parse(text.Substring(lPos).Trim());
        //26
        // remove day 
        text = text.Substring(0, lPos).Trim();
        // Month
        // 06
        lPos = text.LastIndexOf(" ");
        this.Month = int.Parse(text.Substring(lPos).Trim());
        // remove month 
        text = text.Substring(0, lPos).Trim();
        // amount

        decimal amount = 0;
        //"029118:025397 17,50 489,06-   SF
        //"@11,300% 17.646,58- 
        lPos = text.LastIndexOf(" ");
        if (lPos != -1)
        {
            //17.646,58- 
            //489,06-     SF
            amount = pdfUtility.getDecimal(text.Substring(lPos).Trim());
            text = text.Substring(0, lPos).Trim();
        }
        else
        {
            amount = pdfUtility.getDecimal(text.Trim());
        }
        // remove amount

        if (amount < 0)
        {
            this.Debit = amount;
        }
        else
        {
            this.Credit = amount;
        }

        //"029118:025397 17,50   SF
        //"@11,300% 
        // get next word
        lPos = text.LastIndexOf(" ");
        if (lPos != -1)
        {
        }
        // check for interest payment 
        if(Ref == 93)
        {
            //INTEREST ON OVERDRAFT UP~
            //TO 02 24 LIMIT 1~
            //280065019 @10,500 %

            //INTEREST ON OVERDRAFT UP~
            //TO 02 24 280065019~
            //@10,500 %

            //RENTE OP OORTREKKING TOT
            //OP 01 24 LIMIET 1 
            //370602145 @11,450 % 106.432,84 - 01 25 1.905.875,05 - 000000093

            //RENTE OP OORTREKKING TOT 
            //OP 01 24 OOR LIMIET 1 
            //370602145 @13,950% 83,24- 01 25 1.906.935,02-000000093

            //RENTE OP OORTREKKING TOT 
            //OP 02 24 370604946 
            //@11,200% 54.772,60- 02 25 3.290.978,48-000000093

            if (text.Contains("@") && (text.Contains("%")))
            {
                // get full narrative 
                string tmp = Narrative += text;
                tmp = tmp.Replace("~", " ");
                while (tmp.Contains("  "))
                {
                    tmp = tmp.Replace("  ", " ");
                }
                string[] parts = tmp.Split(new char[] { ' ' });
                InterestAccountNumber = parts[parts.Length - 2];
            }

        }


        Narrative += text;

        if (!Narrative.StartsWith(getResX("BalanceBroughtForward")))
        {
            // transaction Date 
            int Year = statementDate.Year;
            if (this.Month == 1 && previousMonth == 12) Year++;
            transactionDate = new DateTime(Year, Month, Day);
        }
        else
        {
            transactionDate = DateTime.MinValue;
        }
    }

    public override string ToString()
    {
        bool deb = false;
        if(Narrative.StartsWith("INTEREST ON OVERDRAFT"))
        {
            //deb = true; 
        }
        // DEBUG DUMP =========================================
        Debug.WriteLineIf(deb, "---------------------------------------------------------------------");
        Debug.WriteLineIf(deb, "Balance : " + this.Balance);
        Debug.WriteLineIf(deb, "Credit : " + this.Credit);
        Debug.WriteLineIf(deb, "Day : " + this.Day);
        Debug.WriteLineIf(deb, "Debit  : " + this.Debit);
        Debug.WriteLineIf(deb, "Index : " + this.Index);
        Debug.WriteLineIf(deb, "InterestAccountNumber : " + this.InterestAccountNumber);
        Debug.WriteLineIf(deb, "isServiceFee : " + this.isServiceFee);
        Debug.WriteLineIf(deb, "Language : " + this.Language);
        Debug.WriteLineIf(deb, "Month : " +this.Month);
        Debug.WriteLineIf(deb, "Narrative : " + this.Narrative);
        Debug.WriteLineIf(deb, "Page Number : " + this.PageNumber);
        Debug.WriteLineIf(deb, "Ref : " + this.Ref);
        Debug.WriteLineIf(deb, "Service Amount : " + this.ServiceAmount);
        Debug.WriteLineIf(deb, "Text : " + this.text);
        Debug.WriteLineIf(deb, "Transaction date : "  +this.transactionDate);
        // DEBUG DUMP =========================================
        Debug.WriteLineIf(deb, "---------------------------------------------------------------------");

        int lnarLen = 0;
        string retVal = "";
        int nar_len = Narrative.Length;
        Debug.WriteLineIf(deb, "Narrative Length : " + nar_len);
        int lpos = Narrative.LastIndexOf("~");
        Debug.WriteLineIf(deb, "Last Pos of ~ : " + lpos);
        if (lpos == -1)
        {
            lnarLen = Narrative.Length;
        }
        else
        {
            lnarLen = nar_len - lpos - 1;
        }
        Debug.WriteLineIf(deb, "Adjusted narrative len : " + lnarLen);

        if (lnarLen < 30)
        {
            retVal = Narrative + "".PadRight(30 - lnarLen, ' ');
            //retVal = Narrative.Replace("~", System.Environment.NewLine) + "".PadRight(30 - lnarLen, ' ');
        }
        else
        {
            retVal = Narrative;
            //retVal = Narrative.Replace("~", System.Environment.NewLine);
        }
        Debug.WriteLineIf(deb, "After Replacement of ~ to newLine : " + retVal);


        if (isServiceFee)
        {
            retVal += "     ## ";
        }
        if (ServiceAmount > 0)
        {
            retVal += string.Format("{0:N2}", ServiceAmount).PadLeft(8, ' ');
        }
        if (!isServiceFee && ServiceAmount == 0)
        {
            retVal += "".PadRight(8, ' ');
        }
        if (Debit < 0)
        {
            retVal += string.Format("{0:N2}", Debit).PadLeft(15, ' ');
        }
        else
        {
            retVal += "".PadRight(15, ' '); ;  // 15 spaces
        }
        if (Credit > 0)
        {
            retVal += string.Format("{0:N2}", Credit).PadLeft(15, ' ');
        }
        else
        {
            retVal += "".PadRight(15, ' '); ;  // 15 spaces
        }
        retVal += " ";
        if (Narrative.StartsWith(getResX("BalanceBroughtForward")))
        {
            retVal += "".PadRight(6, ' '); ;  //  _01_12_
        }
        else
        {
            retVal += Month.ToString().PadLeft(2, '0') + " ";
            retVal += Day.ToString().PadLeft(2, '0') + " ";
        }

        retVal += string.Format("{0:N2}", Balance).PadLeft(15, ' ') + " ";

        if (!Narrative.StartsWith(getResX("BalanceBroughtForward")))
        {
            retVal += "  " + Ref.ToString().PadLeft(9, '0');
        }

        retVal += transactionDate.ToShortDateString();
        return retVal;
    }
}