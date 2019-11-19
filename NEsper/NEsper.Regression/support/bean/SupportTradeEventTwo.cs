namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportTradeEventTwo
    {
        public SupportTradeEventTwo(
            long time,
            int securityID,
            double price,
            long volume)
        {
            Time = time;
            SecurityID = securityID;
            Price = price;
            Volume = volume;
        }

        public int SecurityID { get; }

        public long Time { get; }

        public double Price { get; }

        public long Volume { get; }
    }
} // end of namespace