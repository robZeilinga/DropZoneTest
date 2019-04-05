using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Upload1 : System.Web.UI.Page
{
    /* change log  
     *  NB NB when deploying to 10.104.64.6  icertgenerator.standardbank.co.za - REMEMBER to set Default document to Upload.ASPX !!!! via IIS 
     *  =====================================================================================================================================
     *  1.0 added log,
     *  1.1 added year buttons and better exception reporting  15/10/2018
     *  1.2 fixed language bug in statements when reading vat analysis  22/10/2018
     *  1.3 2018-10-30 fixed findCompany name, using ':' , fixed end of items identifier if "Fee Structure" , added Alert if No items found for Date Filter.
     *  1.4 2019-04-03 fixed malformed handling of 3 line interest, 
     *          fixed change of interest line on statement  
     *          (need to check Afrikaans version of ## These fees include VAT at 14% up to 31 March 2018 and at 15% from 1 April 2018.) 
     *  1.5 2019-04-04 added check for moneymarket call accounts, 
     *          (need to get afrikaans version for test)
     *          Changed main page back to default.aspx - because of autostart page in IIS.
     * 
     * 
     * 
     * 
     * 
     * 
     */



    protected void Page_Load(object sender, EventArgs e)
    {
            hdnServerPrefix.Value = Request.Url.GetLeftPart(UriPartial.Authority)  + WebConfigurationManager.AppSettings["WebServerPrefix"];
        //hdnServerPrefix.Value = Server.MapPath(WebConfigurationManager.AppSettings["WebServerPrefix"]);

         //SW Southern Cape Business Centre
    }


    protected void Button1_Click(object sender, EventArgs e)
    {

    }

    protected void Button1_Click1(object sender, EventArgs e)
    {
       
    }

    protected void Button1_Click2(object sender, EventArgs e)
    {
        //WebConfigurationManager.AppSettings.Set("RegionName", txtRegion.Text);
    }
}