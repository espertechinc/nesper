///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
    /// <summary>
    /// State of the overlapping and non-overlapping context. Serializable for the purpose of SPI testing.
    /// </summary>
    [Serializable]
    public class ContextControllerInitTermState
    {
        private readonly long _startTime;
        private readonly IDictionary<String, Object> _patternData;

        public ContextControllerInitTermState(long startTime, IDictionary<String, Object> patternData)
        {
            _startTime = startTime;
            _patternData = patternData;
        }

        public long StartTime
        {
            get { return _startTime; }
        }

        public IDictionary<string, object> PatternData
        {
            get { return _patternData; }
        }
    }
}
