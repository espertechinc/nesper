///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public partial class StreamSpecCompiler
    {
        public class FilterForFilterFactoryNodes : EvalNodeUtilFactoryFilter
        {
            public static readonly FilterForFilterFactoryNodes INSTANCE = new FilterForFilterFactoryNodes();

            public bool Consider(EvalForgeNode node)
            {
                return node is EvalFilterForgeNode;
            }
        }
    }
}