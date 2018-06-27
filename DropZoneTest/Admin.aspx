<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Admin.aspx.cs" Inherits="Admin" %>

<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Usage Stats for Certificate of Interest Generator</h2>
        <div>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:UsageDBConn %>" SelectCommand="SELECT 
  CAST(TimeStamp AS DATE) AS Year, 
  COUNT(*) AS usage
FROM usage 
                GROUP BY CAST(TimeStamp AS DATE);
"></asp:SqlDataSource>
        </div>
        <div>

        </div>
        <asp:Repeater ID="Repeater1" runat="server" DataSourceID="SqlDataSource1"   OnItemDataBound="Repeater1_ItemDataBound"  >
            <HeaderTemplate>
                <table border="1">
                    <tr><th>Date</th><th>usage</th></tr>

            </HeaderTemplate>
            <ItemTemplate>
                <tr><td><asp:Label ID="r_YearLabel" runat="server" Text='<%# Eval("Year").ToString().Substring(0,10) %>' /></td>
                    <td><asp:Label ID="r_usageLabel" runat="server" Text='<%# Eval("usage") %>' /></td>
            </ItemTemplate>
            <FooterTemplate>
                    <tr><td>Total</td><td><asp:Label ID="lblTotalUsage" runat="server"  /></td></tr>
        </table>

            </FooterTemplate>
        </asp:Repeater>
        <div>
            <asp:Chart ID="Chart1" runat="server" DataSourceID="SqlDataSource1">
                <Series>
                    <asp:Series Name="Series1" ChartType="Line" YValueMembers="usage" XValueMember="Year"></asp:Series>
                </Series>
                <ChartAreas>
                    <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
                </ChartAreas>
            </asp:Chart>
        </div>
    </form>
</body>
</html>
