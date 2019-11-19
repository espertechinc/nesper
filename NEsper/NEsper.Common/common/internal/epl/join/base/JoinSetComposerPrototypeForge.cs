///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public abstract class JoinSetComposerPrototypeForge
    {
        private readonly EventType[] streamTypes;
        private readonly ExprNode postJoinEvaluator;
        private readonly bool outerJoins;

        protected abstract Type Implementation();

        protected abstract void PopulateInline(
            CodegenExpression impl,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract QueryPlanForge OptionalQueryPlan { get; }

        protected JoinSetComposerPrototypeForge(
            EventType[] streamTypes,
            ExprNode postJoinEvaluator,
            bool outerJoins)
        {
            this.streamTypes = streamTypes;
            this.postJoinEvaluator = postJoinEvaluator;
            this.outerJoins = outerJoins;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(Implementation(), this.GetType(), classScope);

            method.Block
                .DeclareVar(Implementation(), "impl", NewInstance(Implementation()))
                .SetProperty(
                    Ref("impl"),
                    "StreamTypes",
                    EventTypeUtility.ResolveTypeArrayCodegen(streamTypes, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("impl"), "OuterJoins", Constant(outerJoins));

            if (postJoinEvaluator != null) {
                method.Block.SetProperty(
                    Ref("impl"),
                    "PostJoinFilterEvaluator",
                    ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                        postJoinEvaluator.Forge,
                        method,
                        this.GetType(),
                        classScope));
            }

            PopulateInline(@Ref("impl"), method, symbols, classScope);

            method.Block.MethodReturn(@Ref("impl"));

            return LocalMethod(method);
        }
    }
} // end of namespace