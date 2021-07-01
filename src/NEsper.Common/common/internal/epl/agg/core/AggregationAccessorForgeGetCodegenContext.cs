///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationAccessorForgeGetCodegenContext
    {
        public AggregationAccessorForgeGetCodegenContext(
            int column,
            CodegenClassScope classScope,
            AggregationStateFactoryForge accessStateForge,
            CodegenMethod method,
            CodegenNamedMethods namedMethods)
        {
            Column = column;
            ClassScope = classScope;
            AccessStateForge = accessStateForge;
            Method = method;
            NamedMethods = namedMethods;
        }

        public int Column { get; }

        public CodegenClassScope ClassScope { get; }

        public AggregationStateFactoryForge AccessStateForge { get; }

        public CodegenMethod Method { get; }

        public CodegenNamedMethods NamedMethods { get; }
    }
} // end of namespace