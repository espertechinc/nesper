///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.bean
{
    public class SupportQueueLeave
    {
        private readonly int id;
        private readonly string location;
        private readonly long timeLeave;
    
        public SupportQueueLeave(int id, string location, long timeLeave)
        {
            this.id = id;
            this.location = location;
            this.timeLeave = timeLeave;
        }

        public int Id
        {
            get { return id; }
        }

        public string Location
        {
            get { return location; }
        }

        public long TimeLeave
        {
            get { return timeLeave; }
        }
    }
}
