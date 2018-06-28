using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualBasic;

/// <summary>
/// Summary description for StatementLine
/// </summary>
public class StatementLine
{
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
        return (string)HttpContext.GetGlobalResourceObject(Language, key);
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


        //INTEREST ON OVERDRAFT UP TO 09 25 280016093 @11,050 %
        //nar
        //"INTEREST ON OVERDRAFT UP~TO 06 25 280016093~"
        //rest
        //"@11,300% 17.646,58- 06 26 6.580.516,04-000000093"
        //index
        //4
        //pageNum
        //10
        //statementDate
        //{ 6 / 12 / 2017 12:00:00 AM}
        //previousMon
        //6

        //
        // TODO: Add constructor logic here
        //
        //"029118:025397 17,50 489,06- 04 25 8.629,64- 019600587~    Service Fee

        // get ref  -- 019600587~     Service Fee
        // get ref  -- 000000093"
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
            /*
            string Service = text.Substring(lPos).Trim();
            if (Service == "##")
            {
                this.isServiceFee = true;
                text = text.Substring(0, lPos).Trim();
                Debit = amount;
            }
            else
            {
                // ================================================================
                //   this switch statement is to get the SERVICE fee applicable 
                // ================================================================
                switch (this.Ref)   
                {

                    //BALANCE BROUGHT FORWARD 4.420.990,79INTEREST ON OVERDRAFT UP TO 02 24 LIMIT 1 280015135 @11,700 %

                    case 20587:
                        //DEBIETOORPLASING 9792 EASYPAY EASYP 1172000498 17,50 476,27 - 01 15 10.162.198,32 - 019600587


                        this.ServiceAmount = pdfUtility.getDecimal(Service);
                        text = text.Substring(0, lPos).Trim();
                        break;
                    case 19600587:
                        //CREDIT TRANSFER 9346~
                        //OTE6600 LanherneGH                    3.679,50    04 19   217.044,51-     019600587~
                        //KREDIETOORPLASING 9052
                        //ABSA BANK 33 Kloovenburg              221,00      02 26   3.346.471,68-   063200587


                        if (Narrative.StartsWith(getResX("CreditTransfer")))
                        {
                            // do nothing 
                        }
                        //DEBIT TRANSFER 9940~
                        //CENTRAFIN DEBIT~
                        //029118:025397     17,50   489,06-     04 25   8.629,64-   019600587~

                        //DEBIETOORPLASING 9184
                        //VODACOM 0238096838 
                        //B0020935          17,50   773,63-     02 29   758.347,51- 019600587

                        if (Narrative.StartsWith(getResX("DebitTransfer")))
                        {
                            this.ServiceAmount = pdfUtility.getDecimal(Service);
                            text = text.Substring(0, lPos).Trim();
                        }
                        break;

                    case 83:
                        //IB PAYMENT TO~            //ELEKTRONIESE BANK BETALING NA
                        //GRAHAMSTOWN VEHICLE~
                        //263795552 7,33 360,00 - 02 14 456.025,58 - 000000083~
                        if (Narrative.StartsWith(getResX("IBPaymentTo")))
                        {
                            this.ServiceAmount = pdfUtility.getDecimal(Service);
                            text = text.Substring(0, lPos).Trim();
                        }
                        //IB FUTURE-DATED PAYMENT~
                        //TO ANDRE SWANEPOEL TRU~
                        //263940919 18,30 57.000,00 - 02 28 527.328,20 - 000000083~

                        //IB FUTURE-DATED PAYMENT~
                        //FROM STANNIC FAW 8.309,86 03 01 117.935,98 000000083
                        if (Narrative.StartsWith("IB FUTURE-DATED PAYMENT~"))
                        {
                            if (rest.StartsWith("FROM "))
                            {
                                // do nothing 
                            }
                            else
                            {
                                this.ServiceAmount = pdfUtility.getDecimal(Service);
                                text = text.Substring(0, lPos).Trim();
                            }
                        }
                        break;
                    case 94:

                        //KONTANTDEPOSITOFOOI 
                        //TAK REK 370602145 DEP 
                        //TAK 0507 ## 93,62- 01 15 11.413.360,94-000000094

                        break;


                }
            }
            */
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
        int lnarLen = 0;
        string retVal = "";
        int nar_len = Narrative.Length;
        int lpos = Narrative.LastIndexOf("~");
        if (lpos == -1)
        {
            lnarLen = Narrative.Length;
        }
        else
        {
            lnarLen = nar_len - lpos - 1;
        }
        retVal = Narrative.Replace("~", System.Environment.NewLine) + "".PadRight(30 - lnarLen, ' ');
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