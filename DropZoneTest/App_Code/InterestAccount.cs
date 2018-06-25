using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for InterestAccount
/// </summary>
public class InterestAccount
{
    public string InterestAccountNumber { get; set; }
    public decimal Total { get; set; }
    public InterestAccount()
    {
        //
        // TODO: Add constructor logic here
        //
        Total = 0;
    }

    public InterestAccount(string _accNumber, decimal _amount)
    {
        InterestAccountNumber = _accNumber;
        Total = _amount;
    }

    public void AddAmount(decimal amount)
    {
        Total += amount;
    }
}