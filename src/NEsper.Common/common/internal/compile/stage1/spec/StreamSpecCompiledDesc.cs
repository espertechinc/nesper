///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class StreamSpecCompiledDesc
    {
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;
        private readonly StreamSpecCompiled streamSpecCompiled;

        public StreamSpecCompiledDesc(
            StreamSpecCompiled streamSpecCompiled,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            this.streamSpecCompiled = streamSpecCompiled;
            this.additionalForgeables = additionalForgeables;
        }

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public StreamSpecCompiled StreamSpecCompiled => streamSpecCompiled;
    }
} // end of namespace