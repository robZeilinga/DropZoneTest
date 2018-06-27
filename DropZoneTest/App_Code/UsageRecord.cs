using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for UsageRecord
/// </summary>
public class UsageRecord
{
    public DateTime timestamp { get; set; }
    public  string staff { get; set; }
    public string region  { get; set; }

    public UsageRecord(DateTime ts, string staff, string region )
    {
        //
        // TODO: Add constructor logic here
        //
        this.timestamp = ts;
        this.staff = staff;
        this.region = region;
    }
}