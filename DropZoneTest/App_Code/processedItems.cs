using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

/// <summary>
/// Summary description for processedItems
/// </summary>
public class processedItems
{
    public string  fileName { get; set; }
    public List<StatementLine> Items { get; set; }
    public string  contents { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal BalanceAtEnd { get; set; }
    public List<InterestAccount> Accounts{ get; set; }
    public string MainAccount { get; set; }



    public processedItems()
    {
        //
        // TODO: Add constructor logic here
        //
        Items = new List<StatementLine>();
        Accounts = new List<InterestAccount>();

    }

    public string getJson()
    {
        return JsonConvert.SerializeObject(Accounts);
    }
}