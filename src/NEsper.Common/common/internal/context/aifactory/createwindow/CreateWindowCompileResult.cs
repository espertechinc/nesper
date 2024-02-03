///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;


namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class CreateWindowCompileResult
    {
        private readonly FilterSpecCompiled filterSpecCompiled;
        private readonly EventType asEventType;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;

        public CreateWindowCompileResult(
            FilterSpecCompiled filterSpecCompiled,
            EventType asEventType,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            this.filterSpecCompiled = filterSpecCompiled;
            this.asEventType = asEventType;
            this.additionalForgeables = additionalForgeables;
        }

        public FilterSpecCompiled FilterSpecCompiled => filterSpecCompiled;

        public EventType AsEventType => asEventType;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;
    }
} // end of namespace