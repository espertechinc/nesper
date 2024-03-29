///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportQueueLeave
    {
        public SupportQueueLeave(
            int id,
            string location,
            long timeLeave)
        {
            Id = id;
            Location = location;
            TimeLeave = timeLeave;
        }

        public int Id { get; }

        public string Location { get; }

        public long TimeLeave { get; }
    }
} // end of namespace