///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.supportregression.bean
{
	[Serializable]
    public class SupportBeanTimestamp
	{
		private readonly String id;
        private readonly long timestamp;
        private readonly String groupId;

        public String Id
        {
            get { return id; }
        }

        public long Timestamp
        {
            get { return timestamp; }
        }

	    public String GroupId
	    {
            get { return groupId; }
	    }

		public SupportBeanTimestamp(String id, long timestamp)
		{
            this.id = id;
            this.timestamp = timestamp;
		}

        public SupportBeanTimestamp(String id, String groupId, long timestamp)
        {
            this.id = id;
            this.groupId = groupId;
            this.timestamp = timestamp;
        }
	}
}
