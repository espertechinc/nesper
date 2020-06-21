///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class CreateWindowCompileResult
    {
        public CreateWindowCompileResult(
            FilterSpecCompiled filterSpecCompiled,
            SelectClauseSpecRaw selectClauseSpecRaw,
            EventType asEventType,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            FilterSpecCompiled = filterSpecCompiled;
            SelectClauseSpecRaw = selectClauseSpecRaw;
            AsEventType = asEventType;
            AdditionalForgeables = additionalForgeables;
        }

        public FilterSpecCompiled FilterSpecCompiled { get; }

        public SelectClauseSpecRaw SelectClauseSpecRaw { get; }

        public EventType AsEventType { get; }
        
        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }
    }
} // end of namespace