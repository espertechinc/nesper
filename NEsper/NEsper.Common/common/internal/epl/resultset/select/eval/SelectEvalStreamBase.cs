///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalStreamBase : SelectExprProcessorForge
    {
        internal readonly SelectExprForgeContext context;
        internal readonly bool isUsingWildcard;
        internal readonly IList<SelectClauseStreamCompiledSpec> namedStreams;
        internal readonly EventType resultEventType;
        protected ExprEvaluator[] evaluators;

        public SelectEvalStreamBase(
            SelectExprForgeContext context,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard)
        {
            this.context = context;
            this.resultEventType = resultEventType;
            this.namedStreams = namedStreams;
            isUsingWildcard = usingWildcard;
        }

        public SelectExprForgeContext Context => context;

        public EventType ResultEventType => resultEventType;

        public abstract CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace