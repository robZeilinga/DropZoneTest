using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Translate
/// </summary>
public class Translate
{
    public  Translate()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static string translateDateToEnglish(string aDate)
    {
        string[] parts = aDate.Trim().Split(new char[] { ' ' });
        string month = "";
        switch (parts[1].Trim().Substring(0,3))
        {
            case "Jan":
            case "Feb":
            case "Apr":
            case "Jun":
            case "Jul":
            case "Aug":
            case "Sep":
            case "Nov":
                month = parts[1].Trim().Substring(0, 3);
                break;
            case "Maa":
                month = "Mar";
                break;
            case "Mei":
                month = "May";
                break;
            case "Okt":
                month = "Oct";
                break;
            case "Des":
                month = "Dec";
                break;
        }
        return parts[0].Trim() + " " + month + " " + parts[2];
    }
}