///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanTimestamp
    {
        public SupportBeanTimestamp(
            string id,
            long timestamp)
        {
            Id = id;
            Timestamp = timestamp;
        }

        public SupportBeanTimestamp(
            string id,
            string groupId,
            long timestamp)
        {
            Id = id;
            GroupId = groupId;
            Timestamp = timestamp;
        }

        public string Id { get; }

        public long Timestamp { get; }

        public string GroupId { get; }
    }
} // end of namespace