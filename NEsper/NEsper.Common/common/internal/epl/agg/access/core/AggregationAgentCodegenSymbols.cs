///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentCodegenSymbols : ExprForgeCodegenSymbol
    {
        public const string NAME_AGENTSTATE = "state";
        public static readonly CodegenExpressionRef REF_AGENTSTATE = Ref(NAME_AGENTSTATE);

        private CodegenExpressionRef optionalStateRef;

        public AggregationAgentCodegenSymbols(bool allowUnderlyingReferences, bool newDataValue)
            : base(allowUnderlyingReferences, newDataValue)

        {
        }

        public CodegenExpressionRef GetAddState(CodegenMethodScope scope)
        {
            if (optionalStateRef == null)
            {
                optionalStateRef = REF_AGENTSTATE;
            }

            scope.AddSymbol(optionalStateRef);
            return optionalStateRef;
        }

        public override void Provide(IDictionary<string, Type> symbols)
        {
            base.Provide(symbols);
            if (optionalStateRef != null)
            {
                symbols.Put(optionalStateRef.Ref, typeof(AggregationMultiFunctionState));
            }
        }
    }
} // end of namespace