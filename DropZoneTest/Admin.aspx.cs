using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin : System.Web.UI.Page
{
    int total = 0;

    protected void Page_Load(object sender, EventArgs e)
    {

    }




    protected void Repeater1_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        RepeaterItem ri = e.Item;
        if(ri.ItemType != ListItemType.Header && ri.ItemType != ListItemType.Footer)
        {
            Label ll = (Label)e.Item.FindControl("r_usageLabel");
            int i = 0;
            if (int.TryParse(ll.Text, out i))
            {
                total += i;
            }
        }
        if(ri.ItemType == ListItemType.Footer)
        {
            Label ll = (Label)ri.FindControl("lblTotalUsage");
            ll.Text = total.ToString();
        }
    }
}