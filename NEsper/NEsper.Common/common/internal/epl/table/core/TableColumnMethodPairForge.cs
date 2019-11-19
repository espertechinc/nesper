///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableColumnMethodPairForge
    {
        private readonly ExprForge[] forges;
        private readonly int column;
        private readonly ExprNode aggregationNode;

        public TableColumnMethodPairForge(
            ExprForge[] forges,
            int column,
            ExprNode aggregationNode)
        {
            this.forges = forges;
            this.column = column;
            this.aggregationNode = aggregationNode;
        }

        public int Column {
            get => column;
        }

        public ExprNode AggregationNode {
            get => aggregationNode;
        }

        public ExprForge[] Forges {
            get { return forges; }
        }

        public static CodegenExpression MakeArray(
            TableColumnMethodPairForge[] methodPairs,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression[] inits = new CodegenExpression[methodPairs.Length];
            for (int i = 0; i < inits.Length; i++) {
                inits[i] = methodPairs[i].Make(method, symbols, classScope);
            }

            return NewArrayWithInit(typeof(TableColumnMethodPairEval), inits);
        }

        private CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression eval;
            if (forges.Length == 0) {
                eval = ExprNodeUtilityCodegen.CodegenEvaluator(
                    new ExprConstantNodeImpl((object) null).Forge,
                    method,
                    this.GetType(),
                    classScope);
            }
            else if (forges.Length == 1) {
                eval = ExprNodeUtilityCodegen.CodegenEvaluator(forges[0], method, this.GetType(), classScope);
            }
            else {
                eval = ExprNodeUtilityCodegen.CodegenEvaluatorObjectArray(forges, method, this.GetType(), classScope);
            }

            return NewInstance<TableColumnMethodPairEval>(eval, Constant(column));
        }
    }
} // end of namespace