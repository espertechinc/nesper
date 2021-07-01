///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Contains the query plan for all streams.
    /// </summary>
    public class QueryPlanForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="indexSpecs">specs for indexes to create</param>
        /// <param name="execNodeSpecs">specs for execution nodes to create</param>
        public QueryPlanForge(
            QueryPlanIndexForge[] indexSpecs,
            QueryPlanNodeForge[] execNodeSpecs)
        {
            IndexSpecs = indexSpecs;
            ExecNodeSpecs = execNodeSpecs;
        }

        /// <summary>
        ///     Return index specs.
        /// </summary>
        /// <returns>index specs</returns>
        public QueryPlanIndexForge[] IndexSpecs { get; }

        /// <summary>
        ///     Return execution node specs.
        /// </summary>
        /// <returns>execution node specs</returns>
        public QueryPlanNodeForge[] ExecNodeSpecs { get; }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public string ToQueryPlan()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("QueryPlanNode\n");
            buffer.Append(QueryPlanIndexForge.Print(IndexSpecs));
            buffer.Append(QueryPlanNodeForge.Print(ExecNodeSpecs));
            return buffer.ToString();
        }

        public CodegenExpression Make(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<QueryPlan>(
                MakeIndexes(method, symbols, classScope),
                MakeStrategies(method, symbols, classScope));
        }

        private CodegenExpression MakeStrategies(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenMakeableUtil.MakeArray(
                "spec",
                typeof(QueryPlanNode),
                ExecNodeSpecs,
                GetType(),
                parent,
                symbols,
                classScope);
        }

        private CodegenExpression MakeIndexes(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenMakeableUtil.MakeArray(
                "indexes",
                typeof(QueryPlanIndex),
                IndexSpecs,
                GetType(),
                parent,
                symbols,
                classScope);
        }
    }
} // end of namespace