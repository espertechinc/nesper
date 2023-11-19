///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeUnpackCollEventBeanTable : ExprDotForge,
        ExprDotEval
    {
        private readonly EPChainableType typeInfo;
        private readonly TableMetaData table;

        public ExprDotForgeUnpackCollEventBeanTable(
            EventType type,
            TableMetaData table)
        {
            typeInfo = EPChainableTypeHelper.CollectionOfSingleValue(
                table.PublicEventType.UnderlyingType);
            this.table = table;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var eventToPublic =
                TableDeployTimeResolver.MakeTableEventToPublicField(table, classScope, GetType());
            var refEPS = symbols.GetAddEps(parent);
            var refIsNewData = symbols.GetAddIsNewData(parent);
            var refExprEvalCtx = symbols.GetAddExprEvalCtx(parent);
            return StaticMethod(
                typeof(ExprDotForgeUnpackCollEventBeanTable),
                "ConvertToTableUnderling",
                inner,
                eventToPublic,
                refEPS,
                refIsNewData,
                refExprEvalCtx);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="eventToPublic">conversion</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new data flow</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <returns>events</returns>
        public static ICollection<object[]> ConvertToTableUnderling(
            object target,
            TableMetadataInternalEventToPublic eventToPublic,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }

            var events = target.Unwrap<EventBean>();
            var underlyings = new ArrayDeque<object[]>(events.Count);
            foreach (var @event in events) {
                underlyings.Add(eventToPublic.ConvertToUnd(@event, eventsPerStream, isNewData, exprEvaluatorContext));
            }

            return underlyings;
        }

        public EPChainableType TypeInfo => typeInfo;

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitUnderlyingEventColl();
        }

        public ExprDotEval DotEvaluator => this;

        public ExprDotForge DotForge => this;
    }
} // end of namespace