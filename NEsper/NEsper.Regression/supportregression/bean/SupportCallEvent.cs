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
	public class SupportCallEvent
	{
		virtual public long CallId
		{
            get { return _callId; }
		}
		
		virtual public String Source
		{
            get { return _source; }
		}
		
		virtual public String Dest
		{
            get { return _dest; }
		}
		
		virtual public long StartTime
		{
            get { return _startTime; }
		}

		virtual public long EndTime
		{
			get { return _endTime; }
		}

        virtual public long callId
        {
            get { return _callId; }
        }

        virtual public String source
        {
            get { return _source; }
        }

        virtual public String dest
        {
            get { return _dest; }
        }

        virtual public long startTime
        {
            get { return _startTime; }
        }

        virtual public long endTime
        {
            get { return _endTime; }
        }

        private long _callId;
        private String _source;
        private String _dest;
		private long _startTime;
		private long _endTime;
		
		public SupportCallEvent(long callId, String source, String destination, long startTime, long endTime)
		{
            this._callId = callId;
            this._source = source;
            this._dest = destination;
            this._startTime = startTime;
            this._endTime = endTime;
		}
	}
}
