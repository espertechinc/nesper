///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.expression
{
    public class ExpressionWindowTimestampEventPairEnumerator
    {
        public static IEnumerator<EventBean> Create(IEnumerator<ExpressionWindowTimestampEventPair> events)
        {
            while (events.MoveNext()) {
                yield return events.Current?.TheEvent;
            }
        }
    }
} // end of namespace