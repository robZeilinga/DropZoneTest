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

    private int thisPageNumber = 0;


    public StatementHeader(string thePage)
    {
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

            //INTEREST ON OVERDRAFT UP TO 02 24 LIMIT 1 280015135 @11,700% 

            if (t.Contains("INTEREST ON"))
            {
                // finished ----
                // break;
            }


            if (t.StartsWith("## These fees are inclusive of VAT"))
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

                    if (t.StartsWith("BALANCE BROUGHT FORWARD"))
                    {
                        // BALANCE BROUGHT FORWARD 01 14 590.542,50-~
                        StatementLine bbf = new StatementLine();
                        bbf.Narrative = "BALANCE BROUGHT FORWARD";
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
                        if (narrative.Contains("ON OVERDRAFT") && !narrative.Contains("LIMIT"))
                        {

                        }
                        

                        
                        StatementLine sl = new StatementLine(narrative, t, index, thisPageNumber, FromDate, previousMonth);
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
            if (t.StartsWith("Statement from "))
            {
                GetStatementDates(t);
                continue;
            }
            //BUSINESS CURRENT ACCOUNT Account Number 08 275 504 3~
            if (t.Contains("Account Number"))
            {
                GetAccountNumber(t);
                continue;
            }
            //ALIWAL NORTH 0020 Statement No 13~
            if (t.Contains("Statement No"))
            {
                StatementNumber = GetStatmentNumber(t);
                continue;
            }
            //Month-end Balance R 522.676,83-~
            if (t.StartsWith("Month-end Balance "))
            {
                GetMonthEndBalance(t);
                continue;
            }
            //Page 1 of 34~
            if (t.StartsWith("Page "))
            {
                GetStatementPages(t);
                continue;
            }
            if (t == "Fee")
            {
                itemsStarted = true;
                continue;
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
        int pos2 = line.IndexOf(" to ", 14);
        this.FromDate = DateTime.Parse(line.Substring(14, pos2 - 14).Trim());
        this.ToDate = DateTime.Parse(line.Substring(pos2 + 4));

    }

    private static int GetStatmentNumber(string line)
    {
        //Statement No 13~

        int pos = line.IndexOf("Statement No");
        if (pos != -1)
        {
            return int.Parse(line.Substring(pos + 12).Trim());
        }
        return -1;
    }

    private void GetAccountNumber(string line)
    {
        //BUSINESS CURRENT ACCOUNT Account Number 08 275 504 3~
        int pos = line.IndexOf("Account Number");
        if (pos != -1)
        {
            this.AccNumber = line.Substring(pos+14).Replace(" ", "").Trim();
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