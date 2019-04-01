///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextControllerStatementDesc
    {
        public ContextControllerStatementDesc(StatementLightweight lightweight, ContextMergeView contextMergeView)
        {
            Lightweight = lightweight;
            ContextMergeView = contextMergeView;
        }

        public StatementLightweight Lightweight { get; }

        public ContextMergeView ContextMergeView { get; }
    }
} // end of namespace