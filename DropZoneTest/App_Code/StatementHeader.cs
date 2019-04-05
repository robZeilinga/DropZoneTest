using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static pdfUtility;
using System.Diagnostics;
/// <summary>
/// Summary description for StatementHeader
/// </summary>
public class StatementHeader
{
    bool deb = false;

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


    public string Language { get; set; }

    private int thisPageNumber = 0;

    private string getResX(string key)
    {
        return (string)HttpContext.GetGlobalResourceObject(Language, key);
    }

    public StatementHeader(string thePage)
    {
        Debug.WriteLineIf(deb, "Looking for Langauge");
        Language = "";
        if (thePage.Contains("Statement No ") || thePage.Contains("## These fees are inclusive of VAT"))
        {
            Debug.WriteLineIf(deb, "Found - [Statement No ] or [## These fees are inclusive of VAT] so Language == English");
            Language = "English";
        }

        if (thePage.Contains("Statnommer ") || thePage.Contains("## Hierdie koste sluit BTW in"))
        {
            Debug.WriteLineIf(deb, "Found - [Statnommer ] or [## Hierdie koste sluit BTW in] so Language == Afrikaans");
            Language = "Afrikaans";
        }

        if (Language == "")
        {
            Debug.WriteLineIf(deb, "Exception Thrown, Could not determine langage! - Dumping thePage ");
            Debug.WriteLineIf(deb, thePage );
            Debug.WriteLineIf(deb, "---------------------------------------------------");

            throw new Exception("Unable to Determine Language for statement! / Statement format unknown ");
            // return;
        }

        lines = new List<StatementLine>();
        string[] sls = thePage.Split(new char[] { '~' });
        List<string> lsls = sls.ToList<string>();
        Debug.WriteLineIf(deb, "adding all lines in thePage to list<string>");
        Debug.WriteLineIf(deb, "itemsStarted = false");
        bool itemsStarted = false;
        Debug.WriteLineIf(deb, "dateFound = false");
        bool dateFound = false;
        string narrative = "";
        int index = 0;
        int previousMonth = 0;

        Debug.WriteLineIf(deb, "looping through lines in list ");
        foreach (string ll in lsls)
        {
            Debug.WriteLineIf(deb, "Line : " + ll);
            try
            {
                string t = ll.Trim();

                //Overdraft Details* :Overdraft Limit R 2.000.000 at 14,700%
                //RENTE OP OORTREKKING TOT OP 01 24 LIMIET 1 370602145 @11,450% 106.432,84- 01 25 1.905.875,05-000000093
                //INTEREST ON OVERDRAFT UP TO 02 24 LIMIT 1 280015135 @11,700% 

                if (t.Contains(getResX("InterestOn")))   // INTEREST ON  or  RENTE OP
                {
                    // finished ----
                    // break;
                }


                if (t.StartsWith(getResX("FeesAreInclusive")) || t.StartsWith("**") || t.StartsWith(getResX("FeeStructure")))   // ## Hierdie koste sluit BTW in  // Kostestruktuur
                {
                    Debug.WriteLineIf(deb, "Line Starts With " + getResX("FeesAreInclusive") + " OR ** OR " + getResX("FeeStructure") + " So We Are Finished" );
                    // finished ----
                    itemsStarted = false;
                    Debug.WriteLineIf(deb, "itemsStarted = false & Break out of line loop");
                    break;
                }


                if (itemsStarted)
                {

                    if (t.Length <= 28)
                    {
                        narrative += t + "~";
                        Debug.WriteLineIf(deb, "itemsStarted = true && line length < 28 , so adding to narrative : " + narrative + " and continuing ");
                        continue;
                    }
                    else
                    {
                        index++;
                        if (previousMonth == 0) previousMonth = FromDate.Month;
                        Debug.WriteLineIf(deb, "set Previous Month to FromDate.Month if it is 0 ");

                        if (t.StartsWith(getResX("BalanceBroughtForward")))
                        {
                            Debug.WriteLineIf(deb, "Line starts with : " + getResX("BalanceBroughtForward") + " So Build bbf");
                            // BALANCE BROUGHT FORWARD 01 14 590.542,50-~
                            StatementLine bbf = new StatementLine();
                            bbf.Language = Language;
                            bbf.Narrative = getResX("BalanceBroughtForward");
                            bbf.transactionDate = FromDate;
                            bbf.Month = FromDate.Month;
                            bbf.Day = FromDate.Day;
                            decimal amt = pdfUtility.getDecimal(t.Substring(t.LastIndexOf(" ")));
                            bbf.Balance = amt;
                            previousMonth = bbf.Month;
                            Debug.WriteLineIf(deb, "bbf : " + bbf);
                            lines.Add(bbf);
                            narrative = "";
                        }
                        else
                        {
                            //RENTE OP OORTREKKING TOT OP 01 24 LIMIET 1 370602145 @11,450% 106.432,84- 01 25 1.905.875,05-000000093

                            if (narrative.Contains(getResX("OnOverdraft")) && !narrative.Contains(getResX("Limit")))
                            {

                            }

                            if (!t.Contains("*"))
                            {

                                StatementLine sl = new StatementLine(narrative, t, index, thisPageNumber, FromDate, previousMonth, Language);
                                Debug.WriteLineIf(deb, "Line does not contain [*]  -> statement Line : " + sl.ToString()  );
                                previousMonth = sl.Month;
                                lines.Add(sl);
                                Debug.WriteLineIf(deb, "Set Previous Month to statement line month ");
                                narrative = "";
                            }
                        }
                        continue;
                    }
                }


                // search for : ( usually in BizDirect : or Prestige : ) if there are 2 lines with colons, the second instance will reset the counter down t business name 


                if (t.Contains(":"))
                {
                    Debug.WriteLineIf(deb, "Date Found");

                    dateFound = true;
                    index = 0;
                    continue;
                }

                if (dateFound)
                {
                    index++;
                    if (index == 3)
                    {
                        AddressedTo = t.Trim();
                        Debug.WriteLineIf(deb, "Addressed To (index == 3) : " + AddressedTo  );

                    }
                    if (index == 4)
                    {
                        CompanyName = t.Trim();
                        Debug.WriteLineIf(deb, "Company Name (index == 4) : " + CompanyName);
                        index = 0;
                        dateFound = false;
                    }
                }

                // Statement from 14 January 2017 to 13 February 2017~
                if (t.StartsWith(getResX("StatementFrom")))
                {
                    Debug.WriteLineIf(deb, "Getting Statement Dates Line starts with [" + getResX("StatementFrom") + "] " );

                    GetStatementDates(t);
                    Debug.WriteLineIf(deb, "From Date : " + this.FromDate );
                    Debug.WriteLineIf(deb, "To Date : " + this.ToDate);
                    continue;
                }
                //BUSINESS CURRENT ACCOUNT Account Number 08 275 504 3~
                if (t.Contains(getResX("AccountNumber")))
                {
                    GetAccountNumber(t);
                    Debug.WriteLineIf(deb, "Acc Number : " + this.AccNumber);
                    Debug.WriteLineIf(deb, "Acc Type : " + this.AccountType);
                    continue;
                }
                //ALIWAL NORTH 0020 Statement No 13~
                if (t.Contains(getResX("StatementNo")))
                {
                    StatementNumber = GetStatmentNumber(t);
                    Debug.WriteLineIf(deb, "Statement Number  : " + StatementNumber);
                    continue;
                }
                //Month-end Balance R 522.676,83-~
                if (t.StartsWith(getResX("MonthEndBalance")))
                {
                    GetMonthEndBalance(t);
                    Debug.WriteLineIf(deb, "Line starts with [" + getResX("MonthEndBalance") + "]  == " + this.MonthEndBalance);
                    continue;
                }
                //Page 1 of 34~
                if (t.StartsWith(getResX("Page") + " "))
                {
                    GetStatementPages(t);
                    Debug.WriteLineIf(deb, "page " + this.thisPageNumber + " total pages " + this.StatementPages );
                    continue;
                }
                if (Language == "English")
                {
                    if (t == "Fee")
                    {
                        Debug.WriteLineIf(deb, "Line is [Fee] so start items - English");
                        itemsStarted = true;
                        continue;
                    }
                }
                else
                {
                    if (t.Contains("Diensgeld") && t.Contains("Entry Ref"))
                    {
                        Debug.WriteLineIf(deb, "Line contains [Diensgeld] and contains [Entry Ref] so start items - Afrikaans");
                        itemsStarted = true;
                        continue;
                    }
                }
            }

            catch (Exception e4)
            {
                Debug.WriteLineIf(deb, "Exception thrown, on Line : " + ll + " Exception : " + e4.ToString());
                throw new Exception("Statement Header Exception: " + ll, e4);
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
            this.FromDate = DateTime.Parse(Translate.translateDateToEnglish(fdate));
            this.ToDate = DateTime.Parse(Translate.translateDateToEnglish(tdate));
        }

    }

    private int GetStatmentNumber(string line)
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