///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalBase
    {
        internal readonly SelectExprForgeContext context;
        internal readonly EventType resultEventType;

        public SelectEvalBase(
            SelectExprForgeContext context,
            EventType resultEventType)
        {
            this.context = context;
            this.resultEventType = resultEventType;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }
    }
} // end of namespace