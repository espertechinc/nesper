///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalExprNodeForge : ContainedEventEvalForge
    {
        private readonly ExprForge evaluator;
        private readonly EventType eventType;

        public ContainedEventEvalExprNodeForge(ExprForge evaluator, EventType eventType)
        {
            this.evaluator = evaluator;
            this.eventType = eventType;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContainedEventEvalExprNode), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprEvaluator), "eval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(evaluator, method, GetType(), classScope))
                .DeclareVar(
                    typeof(EventType), "type",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(
                    NewInstance(
                        typeof(ContainedEventEvalExprNode), Ref("eval"), Ref("type"), symbols.GetAddInitSvc(method)));
            return LocalMethod(method);
        }
    }
} // end of namespace