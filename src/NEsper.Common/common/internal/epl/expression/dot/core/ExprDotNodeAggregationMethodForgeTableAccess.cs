///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeAggregationMethodForgeTableAccess : ExprDotNodeAggregationMethodForge
    {
        private readonly TableMetadataColumnAggregation _column;
        private readonly ExprTableAccessNodeSubprop _subprop;

        public ExprDotNodeAggregationMethodForgeTableAccess(
            ExprDotNodeImpl parent,
            string aggregationMethodName,
            ExprNode[] parameters,
            AggregationPortableValidation validation,
            ExprTableAccessNodeSubprop subprop,
            TableMetadataColumnAggregation column)
            : base(parent, aggregationMethodName, parameters, validation)
        {
            _subprop = subprop;
            _column = column;
        }

        protected override string TableName => _subprop.TableName;

        protected override string TableColumnName => _column.ColumnName;

        public override bool IsLocalInlinedClass => false;

        protected override CodegenExpression EvaluateCodegen(
            string readerMethodName,
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(requiredType, typeof(ExprTableAccessNode), classScope);

            CodegenExpression eps = symbols.GetAddEps(method);
            var newData = symbols.GetAddIsNewData(method);
            CodegenExpression evalCtx = symbols.GetAddExprEvalCtx(method);

            var future = classScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                new CodegenFieldNameTableAccess(_subprop.TableAccessNumber),
                typeof(ExprTableEvalStrategy));
            method.Block
                .DeclareVar<AggregationRow>("row", ExprDotMethod(future, "GetAggregationRow", eps, newData, evalCtx))
                .IfRefNullReturnNull("row")
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        requiredType,
                        ExprDotMethod(
                            GetReader(classScope),
                            readerMethodName,
                            Constant(_column.Column),
                            Ref("row"),
                            symbols.GetAddEps(method),
                            symbols.GetAddIsNewData(method),
                            symbols.GetAddExprEvalCtx(method))));
            return LocalMethod(method);
        }

        protected override void ToEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            _subprop.ToPrecedenceFreeEPL(writer, flags);
        }
    }
} // end of namespace