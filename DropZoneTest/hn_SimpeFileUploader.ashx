<%@ WebHandler Language="C#" Class="hn_SimpeFileUploader" %>


using System;
using System.Web;
using System.IO;
using System.Drawing;
using Json;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text;

using System.Web.Script.Serialization;
using System.Diagnostics;
using static pdfUtility;

//using int_pdf.interestItem;



public class hn_SimpeFileUploader : IHttpHandler
{

    string debugFile = "";
    bool deb = false;

    public void ProcessRequest(HttpContext context)
    {

        HttpResponse oResponse = context.Response;

        //context.Response.ContentType = "text/plain";
        context.Response.ContentType = "application/pdf";
        iTextSharp.text.Document doc = new iTextSharp.text.Document();

        // new debug file (if exists) 
        int cnt = 0;
        debugFile = HttpContext.Current.Server.MapPath("~/Reports/") + "DEBUG.txt";
        while(File.Exists(debugFile))
        {
            debugFile = HttpContext.Current.Server.MapPath("~/Reports/") + cnt + "_DEBUG.txt";
        }

        string dirFullPath = HttpContext.Current.Server.MapPath("~/Reports/");
        string logFile = HttpContext.Current.Server.MapPath("~/Reports/") + "LOG.HTML";
        string logLine = "<tr><td>" + DateTime.Now.ToShortDateString() + "</td><td>" + DateTime.Now.ToShortTimeString() + "</td><td>" + context.Request.QueryString.ToString().Replace("&", "</td><td>") + "</td>";
        string[] files;
        int numFiles;
        files = System.IO.Directory.GetFiles(dirFullPath);
        numFiles = files.Length;
        numFiles = numFiles + 1;

        string str_image = "";
        string retVal = "";
        processedItems pi = new processedItems();

        decimal sum = 0;
        decimal endBalance = 0;
        string pathToSave = "";

        // url: "hn_SimpeFileUploader.ashx?start=20170301&end=20180228",
        DateTime startDate = DateTime.MinValue;
        DateTime endDate = DateTime.MaxValue;
        string region = "";
        string name = "";
        string num = "";
        string debug = "";

        if (context.Request.QueryString.Count == 6)
        {
            startDate = convertToDate(context.Request.QueryString.Get("start"));
            endDate = convertToDate(context.Request.QueryString.Get("end"));
            region = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("region"));
            name = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("name"));
            num = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("num"));
            debug = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("debug"));
        }

        Debug.WriteLineIf(deb, "Processing Context");
        Debug.WriteLineIf(deb, "---------------------------------------------------");
        Debug.WriteLineIf(deb, "StartDate : " + startDate.ToShortDateString());
        Debug.WriteLineIf(deb, "EndDate : " + endDate.ToShortDateString());
        Debug.WriteLineIf(deb, "Region : " + region);
        Debug.WriteLineIf(deb, "Name : " + name);
        Debug.WriteLineIf(deb, "Num : " + num);
        Debug.WriteLineIf(deb, "Debug : " + debug);
        Debug.WriteLineIf(deb, "---------------------------------------------------");
        Debug.WriteLineIf(deb, "Context Files");
        foreach (string s in context.Request.Files)
        {
            Debug.WriteLineIf(deb, s);
        }
        Debug.WriteLineIf(deb, "---------------------------------------------------");

        foreach (string s in context.Request.Files)
        {
            Debug.WriteLineIf(deb, "");
            Debug.WriteLineIf(deb, "Busy with File : " + s);

            HttpPostedFile file = context.Request.Files[s];
            string fileName = file.FileName;
            string fileContentType = file.ContentType;
            Debug.WriteLineIf(deb, "FileName : " + fileName);
            Debug.WriteLineIf(deb, "FileContentType : " + fileContentType);

            if (!string.IsNullOrEmpty(fileName))
            {
                string fileExtension = System.IO.Path.GetExtension(fileName);
                str_image = "STMNT_" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + numFiles.ToString() + fileExtension;
                Debug.WriteLineIf(deb,  "Generated File Name : " + str_image);

                pathToSave = HttpContext.Current.Server.MapPath("~/Reports/") + str_image;
                Debug.WriteLineIf(deb,  "Save Path  : " + pathToSave);
                file.SaveAs(pathToSave);

                List<StatementLine> items = new List<StatementLine>();

                try
                {
                    Debug.WriteLineIf(deb, "Extracting Statements From PDF ");
                    List<StatementHeader> stmnts = pdfUtility.ExtractStatementsFromPdf(pathToSave);

                    if (stmnts.Count > 0)
                    {
                        if(debug == "verbose")
                        {
                            logWrite(logFile, logLine);

                        }
                        // GET A LIST OF ALL FILES OLDER THAN 2 DAYS & DELETE 
                        string[] old_files = System.IO.Directory.GetFiles(HttpContext.Current.Server.MapPath("~/Reports/"));
                        foreach (string fle in old_files)
                        {
                            FileInfo fi = new FileInfo(fle);
                            if (fi.Name.ToUpper() != "LOG.HTML")
                            {
                                if (fi.CreationTime < DateTime.Now.AddDays(-1))
                                    fi.Delete();
                            }
                        }
                        // delete input pdf file 
                        System.IO.File.Delete(pathToSave);

                        List<string> AccountNumbers = new List<string>();

                        foreach (StatementHeader sh in stmnts)
                        {
                            foreach (StatementLine sl in sh.lines)
                            {
                                if (sl.transactionDate >= startDate)
                                {
                                    if (sl.transactionDate <= endDate)
                                    {
                                        if (sl.Ref != 0)
                                        {
                                            endBalance = sl.Balance;
                                            if (sl.Ref == 93 && sl.Narrative.StartsWith("INTEREST ON"))
                                            {
                                                items.Add(sl);
                                                //output += "Add Interest <==============================" + System.Environment.NewLine;
                                                sum += sl.Debit;
                                                // Finds first element greater than 20
                                                InterestAccount ia = pi.Accounts.Find(acc => acc.InterestAccountNumber == sl.InterestAccountNumber);
                                                if (ia == null)
                                                {
                                                    // create new interestAccount
                                                    pi.Accounts.Add(new InterestAccount(sl.InterestAccountNumber, sl.Debit));
                                                }
                                                else
                                                {
                                                    // add interest to running total 
                                                    pi.Accounts.Find(acc => acc.InterestAccountNumber == sl.InterestAccountNumber).AddAmount(sl.Debit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            pi.MainAccount = sh.AccNumber;
                        }
                        if(items.Count == 0)
                        {
                            throw new Exception("No Interest Line Items found!</br>Please check start & end dates!");
                        }
                        pi.Items = items;
                        pi.BalanceAtEnd = endBalance;
                        pi.InterestPaid = sum;

                        string accountList = "";
                        str_image = "REPORT_" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + numFiles.ToString() + fileExtension;

                        // str_image = "REPORT_xxxx5043_5 - Copy.PDF";
                        pathToSave = HttpContext.Current.Server.MapPath("~/Reports/") + str_image;

                        decimal InterestPaid = 0;


                        if (pi.Accounts.Count > 1)
                        {
                            // many accounts - so generate list of acccounts only 
                            accountList = ",\"accountTotals\":" + pi.getJson();

                            InterestPaid = pi.Accounts.Find(item => item.InterestAccountNumber == pi.MainAccount).Total;


                        }
                        else
                        {

                            InterestPaid = pi.InterestPaid;
                        }
                        try
                        {

                            pdfUtility.genPDF(pathToSave, InterestPaid, pi.BalanceAtEnd, startDate, endDate, stmnts, region, name, num);

                            str_image = "{\"filename\":\"" + str_image + "\"" + accountList + "}";
                            logLine += "<td>OK</td></tr>";


                        }
                        catch (Exception genPdfException)
                        {
                            str_image = "Error: " + genPdfException.InnerException.Message;
                            logLine += "<td>" + str_image + "</td></tr>";
                        }

                        // str_image = "{\"filename\":\"" + str_image + "\"" + accountList + "}";


                    }
                    else
                    {
                        str_image = "Error: Unable to Read / Process Statements";
                        logLine += "<td>" + str_image + "</td></tr>";
                    }
                }
                catch (Exception e)
                {
                    str_image = "Error:" + e.Message;
                    logLine += "<td>" + str_image + "</td></tr>";
                }

                context.Response.ClearContent();
                context.Response.ContentType = "text/html";
                context.Response.Write(str_image);
                context.Response.Flush();
                context.Response.Close();

                logWrite(logFile, logLine);

            }
        }

    }

    private void logWrite(string fileName, string logLine)
    {
        bool doHeader = false;
        if (!File.Exists(fileName))
        {
            doHeader = true;
        }
        StreamWriter sw = File.AppendText(fileName);
        if (doHeader)
        {
            sw.WriteLine("<html><body><div><table border=1 style=\"border-collapse:collapse; cellpadding:2px; \">");
        }
        sw.WriteLine(logLine);
        sw.Flush();
        sw.Close();

    }



    private DateTime convertToDate(string ss)
    {
        DateTime retVal = DateTime.MinValue;
        if (ss.Length == 8)
        {
            int year = int.Parse(ss.Substring(0, 4));
            int month = int.Parse(ss.Substring(4, 2));
            int day = int.Parse(ss.Substring(6, 2));
            retVal = new DateTime(year, month, day);
        }
        return retVal;
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}
