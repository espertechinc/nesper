///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.pattern;

namespace com.espertech.esper.supportunit.pattern
{
    public class SupportMatchedEventConvertor : MatchedEventConvertor
    {
        public EventBean[] Convert(MatchedEventMap events)
        {
            return new EventBean[0];
        }

        public MatchedEventMapMeta MatchedEventMapMeta
        {
            get { return new MatchedEventMapMeta(new String[0], false); }
        }
    }
}
