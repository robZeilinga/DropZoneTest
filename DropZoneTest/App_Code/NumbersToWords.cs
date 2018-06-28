﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for NumbersToWords
/// </summary>

static class NumbersToWords
{
    private static string[] _ones =
    {
            "zero",
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine"
        };

    private static string[] _teens =
    {
            "ten",
            "eleven",
            "twelve",
            "thirteen",
            "fourteen",
            "fifteen",
            "sixteen",
            "seventeen",
            "eighteen",
            "nineteen"
        };

    private static string[] _tens =
    {
            "",
            "ten",
            "twenty",
            "thirty",
            "forty",
            "fifty",
            "sixty",
            "seventy",
            "eighty",
            "ninety"
        };

    // US Nnumbering:
    private static string[] _thousands =
    {
            "",
            "thousand",
            "million",
            "billion",
            "trillion",
            "quadrillion"
        };

    /// <summary>
    /// Converts a numeric value to words suitable for the portion of
    /// a check that writes out the amount.
    /// </summary>
    /// <param name="value">Value to be converted</param>
    /// <returns></returns>
    public static string Convert(decimal value)
    {
        if (value < 0) value = value * -1;
        string digits, temp;
        bool showThousands = false;
        bool allZeros = true;

        // Use StringBuilder to build result
        StringBuilder builder = new StringBuilder();
        // Convert integer portion of value to string
        digits = ((long)value).ToString();
        // Traverse characters in reverse order
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int ndigit = (int)(digits[i] - '0');
            int column = (digits.Length - (i + 1));

            // Determine if ones, tens, or hundreds column
            switch (column % 3)
            {
                case 0:        // Ones position
                    showThousands = true;
                    if (i == 0)
                    {
                        // First digit in number (last in loop)
                        temp = String.Format("{0} ", _ones[ndigit]);
                    }
                    else if (digits[i - 1] == '1')
                    {
                        // This digit is part of "teen" value
                        temp = String.Format("{0} ", _teens[ndigit]);
                        // Skip tens position
                        i--;
                    }
                    else if (ndigit != 0)
                    {
                        // Any non-zero digit
                        temp = String.Format("{0} ", _ones[ndigit]);
                    }
                    else
                    {
                        // This digit is zero. If digit in tens and hundreds
                        // column are also zero, don't show "thousands"
                        temp = String.Empty;
                        // Test for non-zero digit in this grouping
                        if (digits[i - 1] != '0' || (i > 1 && digits[i - 2] != '0'))
                            showThousands = true;
                        else
                            showThousands = false;
                    }

                    // Show "thousands" if non-zero in grouping
                    if (showThousands)
                    {
                        if (column > 0)
                        {
                            temp = String.Format("{0}{1}{2}",
                                temp,
                                _thousands[column / 3],
                                allZeros ? " " : ", ");
                        }
                        // Indicate non-zero digit encountered
                        allZeros = false;
                    }
                    builder.Insert(0, temp);
                    break;

                case 1:        // Tens column
                    if (ndigit > 0)
                    {
                        temp = String.Format("{0}{1}",
                            _tens[ndigit],
                            (digits[i + 1] != '0') ? "-" : " ");
                        builder.Insert(0, temp);
                    }
                    break;

                case 2:        // Hundreds column
                    if (ndigit > 0)
                    {
                        temp = String.Format("{0} hundred ", _ones[ndigit]);
                        builder.Insert(0, temp);
                    }
                    break;
            }
        }

        // Append fractional portion/cents
        builder.AppendFormat("rand and {0:00} cents", (value - (long)value) * 100);

        // check for hundred 'and' 

        string tempStr = builder.ToString();
        int pos = tempStr.LastIndexOf("hundred");
        if (pos != -1)
        {
            string begin = tempStr.Substring(0, pos + 7);
            string tempStr2 = tempStr.Substring(pos + 7).Trim();
            if (!tempStr2.StartsWith("rand"))
            {
                tempStr = begin.Trim() + " and " + tempStr2.Trim();
            }

            int pos2 = begin.Trim().LastIndexOf("hundred", 8);
            //int pos2 = tempStr.LastIndexOf("hundred", pos + 1);
            if (pos2 != -1)
            {
                string begin2 = begin.Substring(0, pos2 + 7);
                tempStr2 = begin2.Substring(pos2 + 7).Trim();
                if (!tempStr2.StartsWith("thousand"))
                {
                    tempStr = begin2.Trim() + " and " + tempStr2.Trim();
                }
            }
        }

        // Capitalize first letter
        return String.Format("{0}{1}",
            Char.ToUpper(tempStr[0]),
            tempStr.Substring(1));

    }
}
