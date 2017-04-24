﻿using System;
using System.Collections.Generic;

namespace OpBot
{
    internal static class GroupFinder
    {
        private static readonly string[] order = { "SV", "EV", "DF", "RAV", "KP", "EC", "TOS", "TFB", "DP" };
        private static readonly DateTime baseDate = new DateTime(2017, 4, 23);

        public static string OperationOn(DateTime dt)
        {
            int opIndex = GetOrderIndex(dt);
            return order[opIndex];
        }

        public static List<string> NextDays(int numDays)
        {
            List<string> nextDays = new List<string>();
            int opIndex = GetOrderIndex(DateTime.Now.Date);
            for (int k = 0; k < numDays; k++)
            {
                nextDays.Add(order[opIndex]);
                if (++opIndex >= order.Length)
                    opIndex = 0;
            }
            return nextDays;
        }

        private static int GetOrderIndex(DateTime dt)
        {
            TimeSpan toBase = dt.Date - baseDate;
            int opIndex = (int) toBase.TotalDays % order.Length;
            return opIndex;
        }
    }
}
