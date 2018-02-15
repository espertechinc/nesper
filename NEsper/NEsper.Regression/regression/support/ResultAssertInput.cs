///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;


namespace com.espertech.esper.regression.support
{
    public class ResultAssertInput
    {
        private static SortedDictionary<long, TimeAction> actions;
    
        static ResultAssertInput()
        {
            Init();
        }

        public static SortedDictionary<long, TimeAction> GetActions()
        {
            return actions;
        }
    
        private static void Init()
        {
            actions = new SortedDictionary<long, TimeAction>();
    
            // Instructions for a test set:
            // hardcoded for a 5.5-second time window and 1-second output rate !
            // First set the time, second send the Event(s)
            Add(200, MakeEvent("IBM", 100, 25), "Event E1 arrives");
            Add(800, MakeEvent("MSFT", 5000, 9), "Event E2 arrives");
            Add(1000);
            Add(1200);
            Add(1500, MakeEvent("IBM", 150, 24), "Event E3 arrives");
            Add(1500, MakeEvent("YAH", 10000, 1), "Event E4 arrives");
            Add(2000);
            Add(2100, MakeEvent("IBM", 155, 26), "Event E5 arrives");
            Add(2200);
            Add(2500);
            Add(3000);
            Add(3200);
            Add(3500, MakeEvent("YAH", 11000, 2), "Event E6 arrives");
            Add(4000);
            Add(4200);
            Add(4300, MakeEvent("IBM", 150, 22), "Event E7 arrives");
            Add(4900, MakeEvent("YAH", 11500, 3), "Event E8 arrives");
            Add(5000);
            Add(5200);
            Add(5700, "Event E1 leaves the time window");
            Add(5900, MakeEvent("YAH", 10500, 1), "Event E9 arrives");
            Add(6000);
            Add(6200);
            Add(6300, "Event E2 leaves the time window");
            Add(7000, "Event E3 and E4 leave the time window");
            Add(7200);
        }
    
        private static void Add(long time, SupportMarketDataBean theEvent, String eventDesc)
        {
            TimeAction timeAction = actions.Get(time);
            if (timeAction == null)
            {
                timeAction = new TimeAction();
                actions.Put(time, timeAction);
            }
            timeAction.Add(theEvent, eventDesc);
        }
    
        private static void Add(long time)
        {
            Add(time, null);
        }
    
        private static void Add(long time, String desc)
        {
            TimeAction timeAction = actions.Get(time);
            if (timeAction == null)
            {
                timeAction = new TimeAction();
                timeAction.ActionDesc = desc;
                actions.Put(time, timeAction);
            }
        }
    
        private static SupportMarketDataBean MakeEvent(String symbol, long volume, double price)
        {
            return new SupportMarketDataBean(symbol, price, volume, "");
        }
    
    }
}
