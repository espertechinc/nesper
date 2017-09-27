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

        public override EventEvaluator Get(Object expressionValue)
        {
            return null;
        }

        public override void Put(Object expressionValue, EventEvaluator evaluator)
        {
        }

        public override bool Remove(Object expressionValue)
        {
            return true;
        }

        public override int Count
        {
            get { return 0; }
        }

        public override IReaderWriterLock ReadWriteLock
        {
            get { return null; }
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
        }
    }
}
