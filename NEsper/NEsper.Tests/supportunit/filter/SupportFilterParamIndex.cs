///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.filter;

namespace com.espertech.esper.supportunit.filter
{
    public class SupportFilterParamIndex : FilterParamIndexLookupableBase
    {
        public SupportFilterParamIndex(FilterSpecLookupable lookupable)
            : base(FilterOperator.EQUAL, lookupable)
        {
        }

        public override EventEvaluator Get(object expressionValue)
        {
            return null;
        }

        public override void Put(object expressionValue, EventEvaluator evaluator)
        {
        }

        public override void Remove(object expressionValue)
        {
        }

        public override int Count => 0;

        public override bool IsEmpty => true;

        public override IReaderWriterLock ReadWriteLock => null;

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
        }
    }
}
