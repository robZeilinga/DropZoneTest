using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ITextItem
/// </summary>
public class interestItem
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public decimal Amount { get; set; }
    public string Narrative { get; set; }

    public int StatementNumber { get; set; }


    public interestItem(string line, string Year, string StmntNum)
    {

        //RENTE OP OORTREKKING TOT OP    01 24    LIMIET 1      370602145 @11,450% 106.432,84-  01 25 1.905.875,05- 000000093
        //INTEREST ON OVERDRAFT UP TO    01 24    OVER LIMIT 1  082755043 @15,050% 98,94-       01 25 280.196,28-   000000093
        bool isNeg = false;
        string amt = "";
        // we are only interested int he stuff after the % 
        int perc = line.IndexOf("%");
        this.Narrative = line.Substring(0, perc+1).Trim()  ;
        line = line.Substring(perc + 1).Trim();
        string[] vs = line.Split(new char[] { ' ' });
        decimal ii = 0;
        if (vs[0].EndsWith("-"))
        {
            isNeg = true;
            amt = vs[0].Substring(0, vs[0].Length - 1).Replace(".", "").Replace(',', '.');
        }
        else
        {
            amt = vs[0].Trim().Replace(".", "").Replace(',', '.');
        }
        int factor = 1;
        if (isNeg) factor = -1;
        this.Amount = decimal.TryParse(amt.Trim(), out ii) ? ii : 0;
        this.Amount = this.Amount * factor;
        this.Month = int.Parse(vs[1].Trim());
        this.Day = int.Parse(vs[2].Trim());
        this.Year = int.Parse(Year);
        this.StatementNumber = int.Parse(StmntNum);
    }

    public string toCSV()
    {
        return this.Year.ToString() + "," + this.Month.ToString().PadLeft(2, '0') + "," + this.Day.ToString().PadLeft(2, '0') + "," + String.Format("{0,12:N2}", this.Amount).Trim() + "," + this.StatementNumber.ToString() + ",'" + this.Narrative + "'";
    }
    public override string ToString()
    {
        return this.Year.ToString() + " " + this.Month.ToString().PadLeft(2, '0') + " " + this.Day.ToString().PadLeft(2, '0') + " " + String.Format("{0,12:N2}", this.Amount).PadLeft(20, ' ') + " stmt # " + this.StatementNumber.ToString();
    }
}
