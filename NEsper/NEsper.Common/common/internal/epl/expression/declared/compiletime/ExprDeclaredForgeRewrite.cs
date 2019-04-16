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
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    public class ExprDeclaredForgeRewrite : ExprDeclaredForgeBase
    {
        private readonly int[] streamAssignments;

        public ExprDeclaredForgeRewrite(
            ExprDeclaredNodeImpl parent,
            ExprForge innerForge,
            bool isCache,
            int[] streamAssignments,
            bool audit,
            string statementName)
            : base(parent, innerForge, isCache, audit, statementName)

        {
            this.streamAssignments = streamAssignments;
        }

        public override EventBean[] GetEventsPerStreamRewritten(EventBean[] eps)
        {
            // rewrite streams
            EventBean[] events = new EventBean[streamAssignments.Length];
            for (int i = 0; i < streamAssignments.Length; i++) {
                events[i] = eps[streamAssignments[i]];
            }

            return events;
        }

        public override ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        protected override CodegenExpression CodegenEventsPerStreamRewritten(
            CodegenExpression eventsPerStream,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenBlock block = codegenMethodScope.MakeChild(typeof(EventBean[]), typeof(ExprDeclaredForgeRewrite), codegenClassScope)
                .AddParam(typeof(EventBean[]), "eps").Block
                .DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), Constant(streamAssignments.Length)));
            for (int i = 0; i < streamAssignments.Length; i++) {
                block.AssignArrayElement("events", Constant(i), ArrayAtIndex(@Ref("eps"), Constant(streamAssignments[i])));
            }

            return LocalMethodBuild(block.MethodReturn(@Ref("events"))).Pass(eventsPerStream).Call();
        }
    }
} // end of namespace