///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.table.ExprTableAccessNode.AccessEvaluationType;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableAccessNodeTopLevel : ExprTableAccessNode,
        ExprTypableReturnForge,
        ExprTypableReturnEval,
        ExprForge
    {
        private LinkedHashMap<string, object> eventType;

        public ExprTableAccessNodeTopLevel(string tableName)
            : base(tableName)
        {
        }

        public ExprTypableReturnEval TypableReturnEvaluator => this;

        public override ExprForge Forge => this;

        protected override string InstrumentationQName => "ExprTableTop";

        protected override CodegenExpression[] InstrumentationQParams => new[] {Constant(tableMeta.TableName)};

        public override ExprTableEvalStrategyFactoryForge TableAccessFactoryForge {
            get {
                var forge = new ExprTableEvalStrategyFactoryForge(tableMeta, groupKeyEvaluators);
                forge.StrategyEnum = tableMeta.IsKeyed
                    ? ExprTableEvalStrategyEnum.GROUPED_TOP
                    : ExprTableEvalStrategyEnum.UNGROUPED_TOP;
                return forge;
            }
        }

        public object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public object[][] EvaluateTypableMulti(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            throw new UnsupportedOperationException();
        }

        public override Type EvaluationType => typeof(IDictionary<object, object>);

        public IDictionary<string, object> RowProperties => eventType;

        public bool? IsMultirow => false;

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(EVALTYPABLESINGLE, this, typeof(object[]), parent, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            throw new UnsupportedOperationException("Typable-multi is not available for table top-level access");
        }

        public override ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        protected override void ValidateBindingInternal(ExprValidationContext validationContext)
        {
            ValidateGroupKeys(tableMeta, validationContext);
            eventType = new LinkedHashMap<string, object>();
            foreach (var entry in tableMeta.Columns) {
                var classResult = tableMeta.PublicEventType.GetPropertyType(entry.Key);
                eventType.Put(entry.Key, classResult);
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPLInternal(writer);
        }

        protected override bool EqualsNodeInternal(ExprTableAccessNode other)
        {
            return true;
        }
    }
} // end of namespace