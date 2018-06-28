using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for StatementHeader
/// </summary>
public class StatementHeader
{
    public string AccNumber { get; set; }
    public DateTime StatementDate { get; set; }
    public int StatementNumber { get; set; }
    public int StatementPages { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string AccountType { get; set; }
    public decimal MonthEndBalance { get; set; }
    public List<StatementLine> lines { get; set; }
    public string AddressedTo { get; set; }
    public string CompanyName { get; set; }


    public static string Language { get; set; }
    
    private int thisPageNumber = 0;

    private static string getResX(string key)
    {
        return (string)HttpContext.GetGlobalResourceObject(Language, key);
    }

    public StatementHeader(string thePage)
    {
        Language = "";
        if( thePage.Contains("## These fees are inclusive of VAT"))
        {
            Language = "English";
        }
        if (thePage.Contains("## Hierdie koste sluit BTW in"))
        {
            Language = "Afrikaans";
        }

        if (Language == "")
                {
            return;
        }
        lines = new List<StatementLine>();
        string[] sls = thePage.Split(new char[] { '~' });
        List<string> lsls = sls.ToList<string>();
        bool itemsStarted = false;
        bool bizFound = false;
        string narrative = "";
        int index = 0;
        int previousMonth = 0;


        foreach (string ll in lsls)
        {
            string t = ll.Trim();

            //RENTE OP OORTREKKING TOT OP 01 24 LIMIET 1 370602145 @11,450% 106.432,84- 01 25 1.905.875,05-000000093
            //INTEREST ON OVERDRAFT UP TO 02 24 LIMIT 1 280015135 @11,700% 

            if (t.Contains(getResX("InterestOn")))
            {
                // finished ----
                // break;
            }


            if (t.StartsWith(getResX("FeesAreInclusive")))
            {
                // finished ----
                itemsStarted = false;
                break;
            }

            if (itemsStarted)
            {
                if (t.Length < 28)
                {
                    narrative += t + "~";
                    continue;
                }
                else
                {
                    index++;
                    if (previousMonth == 0) previousMonth = FromDate.Month;

                    if (t.StartsWith(getResX("BalanceBroughtForward")))

                    {
                        // BALANCE BROUGHT FORWARD 01 14 590.542,50-~
                        StatementLine bbf = new StatementLine();
                        bbf.Narrative = getResX("BalanceBroughtForward");
                        bbf.transactionDate = FromDate;
                        bbf.Month = FromDate.Month;
                        bbf.Day = FromDate.Day;
                        decimal amt = pdfUtility.getDecimal(t.Substring(t.LastIndexOf(" ")));
                        bbf.Balance = amt;
                        previousMonth = bbf.Month;
                        lines.Add(bbf);
                        narrative = "";
                    }
                    else
                    {
                        //RENTE OP OORTREKKING TOT OP 01 24 LIMIET 1 370602145 @11,450% 106.432,84- 01 25 1.905.875,05-000000093

                        if (narrative.Contains(getResX("OnOverdraft")) && !narrative.Contains(getResX("Limit"))) 
                        {

                        }
                        

                        
                        StatementLine sl = new StatementLine(narrative, t, index, thisPageNumber, FromDate, previousMonth, Language);
                        previousMonth = sl.Month;
                        lines.Add(sl);
                        narrative = "";
                    }
                    continue;
                }
            }

            if (t.Contains("BizDirect"))
            {
                bizFound = true;
                index = 0;
                continue;
            }

            if (bizFound)
            {
                index++;
                if (index == 3)
                {
                    AddressedTo = t.Trim();
                }
                if (index == 4)
                {
                    CompanyName = t.Trim();
                    index = 0;
                    bizFound = false;
                }
            }

            // Statement from 14 January 2017 to 13 February 2017~
            if (t.StartsWith(getResX("StatementFrom")))
            {
                GetStatementDates(t);
                continue;
            }
            //BUSINESS CURRENT ACCOUNT Account Number 08 275 504 3~
            if (t.Contains(getResX("AccountNumber")))
            {
                GetAccountNumber(t);
                continue;
            }
            //ALIWAL NORTH 0020 Statement No 13~
            if (t.Contains(getResX("StatementNo")))
            {
                StatementNumber = GetStatmentNumber(t);
                continue;
            }
            //Month-end Balance R 522.676,83-~
            if (t.StartsWith(getResX("MonthEndBalance")))
            {
                GetMonthEndBalance(t);
                continue;
            }
            //Page 1 of 34~
            if (t.StartsWith(getResX("Page") + " "))
            {
                GetStatementPages(t);
                continue;
            }
            if (Language == "English")
            {
                if (t == "Fee")
                {
                    itemsStarted = true;
                    continue;
                }
            }
            else
            {
                if (t.Contains("Diensgeld") && t.Contains("Ref"))
                {
                    itemsStarted = true;
                    continue;
                }
            }
        }
    }

    private void GetMonthEndBalance(string t)
    {
        //Month-end Balance R 522.676,83-~
        this.MonthEndBalance = pdfUtility.getDecimal(t.Substring(18));
    }

    private void GetStatementDates(string line)
    {
        // difficult - return To Date 
        // Statement from 14 January 2017 to 13 February 2017~
        int ff = 14;
        if (Language != "English") ff = 9;
        int pos2 = line.IndexOf(" " + getResX("to") + " ", ff);
        string fdate = line.Substring(ff, pos2 - ff).Trim();
        string tdate = line.Substring(pos2 + 4);
        if (Language == "English")
        {
            this.FromDate = DateTime.Parse(fdate);
            this.ToDate = DateTime.Parse(tdate);
        }
        if (Language == "Afrikaans")
        {
            this.FromDate = DateTime.Parse(Translate.translateDateToEnglish( fdate));
            this.ToDate = DateTime.Parse(Translate.translateDateToEnglish(tdate));
        }

    }

    private static int GetStatmentNumber(string line)
    {
        //Statement No 13~

        string target = getResX("StatementNo");
        int tLen = target.Length;
        int pos = line.IndexOf(target);
        if (pos != -1)
        {
            return int.Parse(line.Substring(pos + tLen).Trim());
        }
        return -1;
    }

    private void GetAccountNumber(string line)
    {
        //BUSINESS CURRENT ACCOUNT Account Number 08 275 504 3~
        string target = getResX("AccountNumber");
        int tLen = target.Length;
        int pos = line.IndexOf(target);
        if (pos != -1)
        {
            this.AccNumber = line.Substring(pos + tLen).Replace(" ", "").Trim();
            this.AccountType = line.Substring(0, pos);
        }
    }
    private void GetStatementPages(string t)
    {
        //Page 1 of 34~
        string[] parts = t.Split(new char[] { ' ' });
        this.StatementPages = int.Parse(parts[3].Trim());
        this.thisPageNumber = int.Parse(parts[1].Trim());
    }

}