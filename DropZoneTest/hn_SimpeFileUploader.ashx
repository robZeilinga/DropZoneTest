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

//using int_pdf.interestItem;



public class hn_SimpeFileUploader : IHttpHandler
{

    public  void ProcessRequest(HttpContext context)
    {

        HttpResponse oResponse = context.Response;

        //context.Response.ContentType = "text/plain";
        context.Response.ContentType = "application/pdf";
        iTextSharp.text.Document doc = new iTextSharp.text.Document();

        string dirFullPath = HttpContext.Current.Server.MapPath("~/MediaUploader/");
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

        foreach (string s in context.Request.Files)
        {
            HttpPostedFile file = context.Request.Files[s];
            string fileName = file.FileName;
            string fileExtension = file.ContentType;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileExtension = System.IO.Path.GetExtension(fileName);
                str_image = "STMNT_" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + numFiles.ToString() + fileExtension;
                pathToSave = HttpContext.Current.Server.MapPath("~/MediaUploader/") + str_image;
                file.SaveAs(pathToSave);

                List<StatementLine> items = new List<StatementLine>();

                // url: "hn_SimpeFileUploader.ashx?start=20170301&end=20180228",
                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.MaxValue;
                string region = "";
                string name = "";
                string num = "";

                if (context.Request.QueryString.Count == 5)
                {
                    startDate = convertToDate(context.Request.QueryString.Get("start"));
                    endDate = convertToDate(context.Request.QueryString.Get("end"));
                    region = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("region"));
                    name = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("name"));
                    num = HttpContext.Current.Server.UrlDecode(context.Request.QueryString.Get("num"));
                }

                List<StatementHeader> stmnts = pdfUtility.ExtractStatementsFromPdf(pathToSave);

                if (stmnts.Count > 0)
                {

                    // GET A LIST OF ALL FILES OLDER THAN 2 DAYS & DELETE 
                    string[] old_files = System.IO.Directory.GetFiles(HttpContext.Current.Server.MapPath("~/MediaUploader/"));
                    foreach (string fle in old_files)
                    {
                        FileInfo fi = new FileInfo(fle);
                        if (fi.CreationTime < DateTime.Now.AddDays(-1))
                            fi.Delete();
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
                    pi.Items = items;
                    pi.BalanceAtEnd = endBalance;
                    pi.InterestPaid = sum;

                    string accountList = "";
                    str_image = "REPORT_" + System.IO.Path.GetFileNameWithoutExtension(fileName) + "_" + numFiles.ToString() + fileExtension;

                    // str_image = "REPORT_xxxx5043_5 - Copy.PDF";
                    pathToSave = HttpContext.Current.Server.MapPath("~/MediaUploader/") + str_image;

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
                    pdfUtility.genPDF(pathToSave, InterestPaid, pi.BalanceAtEnd, startDate, endDate, stmnts, region, name, num);


                    str_image = "{\"filename\":\"" + str_image + "\"" + accountList + "}";


                }
                else
                {
                    str_image = "No interest payments found";
                }

                context.Response.ClearContent();
                context.Response.ContentType = "text/html";
                context.Response.Write(str_image);
                context.Response.Flush();
                context.Response.Close();


            }
        }

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
