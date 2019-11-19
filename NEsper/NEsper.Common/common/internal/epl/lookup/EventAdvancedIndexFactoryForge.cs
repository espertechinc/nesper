///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public interface EventAdvancedIndexFactoryForge
    {
        EventAdvancedIndexFactory RuntimeFactory { get; }
        bool ProvidesIndexForOperation(string operationName);

        SubordTableLookupStrategyFactoryQuadTreeForge GetSubordinateLookupStrategy(
            string operationName,
            IDictionary<int, ExprNode> expressions,
            bool isNWOnTrigger,
            int numOuterstreams);

        CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope);
    }
} // end of namespace