///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalBaseFirstPropFromWrap : SelectEvalBaseFirstProp
    {
        internal readonly WrapperEventType wrapper;

        public SelectEvalBaseFirstPropFromWrap(
            SelectExprForgeContext selectExprForgeContext,
            WrapperEventType wrapper)
            : base(selectExprForgeContext, wrapper)

        {
            this.wrapper = wrapper;
        }
    }
} // end of namespace