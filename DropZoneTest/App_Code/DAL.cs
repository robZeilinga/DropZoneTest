using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;


/// <summary>
/// Summary description for DAL
/// </summary>
public class DAL
{


    public DAL() {}


    public static async System.Threading.Tasks.Task dbInsertAsync(string DBPath, string staff, string center )
    {
        try
        {
            

            // Open database (create new if file doesn't exist)
            JsonFlatFileDataStore.DataStore store = new JsonFlatFileDataStore.DataStore(DBPath);
            var collection = store.GetCollection("UsageRecord");

            // Create new employee
            UsageRecord rec = new UsageRecord(DateTime.Now, staff, center);

            await collection.InsertOneAsync(rec);

        }
        catch (Exception ex)
        {
            //console.log( "error" + ex.ToString());
            //don't know how to log here ! 
        }
    }

    public static string  getData(string DBPath)
    {
        JsonFlatFileDataStore.DataStore store = new JsonFlatFileDataStore.DataStore(DBPath);
        var collection = store.GetCollection<UsageRecord>();

        // Find item with name
        var usageRecords = collection.AsQueryable();

        var query1 = usageRecords.GroupBy(x => x.timestamp.Month, x => x.timestamp.Day);

        var query2 = from x in usageRecords
                     group x.timestamp.Month  by x.timestamp.Day;


        return new JavaScriptSerializer().Serialize(query2);



    }


}


