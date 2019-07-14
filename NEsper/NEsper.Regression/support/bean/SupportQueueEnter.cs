namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportQueueEnter
    {
        public SupportQueueEnter(
            int id,
            string location,
            string sku,
            long timeEnter)
        {
            Id = id;
            Location = location;
            Sku = sku;
            TimeEnter = timeEnter;
        }

        public int Id { get; }

        public string Location { get; }

        public string Sku { get; }

        public long TimeEnter { get; }
    }
} // end of namespace