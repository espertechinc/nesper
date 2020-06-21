///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.table
{
    public class ExprTableResetRowAggNode : ExprNodeBase,
        ExprForgeInstrumentable
    {
        public ExprTableResetRowAggNode(
            TableMetaData tableMetadata,
            int streamNum)
        {
            TableMetadata = tableMetadata;
            StreamNum = streamNum;
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public override ExprForge Forge => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public TableMetaData TableMetadata { get; }

        public int StreamNum { get; }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var method = parent.MakeChild(typeof(void), GetType(), codegenClassScope);
            method.Block.Expression(StaticMethod(typeof(ExprTableResetRowAggNode), "tableAggReset", Constant(StreamNum), symbols.GetAddEPS(method)));
            return LocalMethod(method);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return EvaluateCodegenUninstrumented(requiredType, parent, symbols, codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator(
                    (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw new UnsupportedOperationException("Cannot evaluate at compile time");
                    });
            }
        }

        public Type EvaluationType => typeof(void);

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(TableMetadata.TableName);
            writer.Write(".reset()");
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="streamNum">stream num</param>
        /// <param name="eventsPerStream">events</param>
        public static void TableAggReset(
            int streamNum,
            EventBean[] eventsPerStream)
        {
            var oa = (ObjectArrayBackedEventBean) eventsPerStream[streamNum];
            var row = ExprTableEvalStrategyUtil.GetRow(oa);
            row.Clear();
        }
    }
} // end of namespace