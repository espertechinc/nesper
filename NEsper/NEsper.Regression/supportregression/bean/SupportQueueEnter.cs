///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.bean
{
    public class SupportQueueEnter
    {
        private readonly int id;
        private readonly string location;
        private readonly string sku;
        private readonly long timeEnter;
    
        public SupportQueueEnter(int id, string location, string sku, long timeEnter)
        {
            this.id = id;
            this.location = location;
            this.sku = sku;
            this.timeEnter = timeEnter;
        }

        public int Id
        {
            get { return id; }
        }

        public string Location
        {
            get { return location; }
        }

        public string Sku
        {
            get { return sku; }
        }

        public long TimeEnter
        {
            get { return timeEnter; }
        }
    }
}
