///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.queryplanbuild;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Specifies execution of a table lookup using the supplied plan for performing the lookup.
    /// </summary>
    public class TableLookupNodeForge : QueryPlanNodeForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tableLookupPlan">plan for performing lookup</param>
        public TableLookupNodeForge(TableLookupPlanForge tableLookupPlan)
        {
            TableLookupPlan = tableLookupPlan;
        }

        public TableLookupPlanForge TableLookupPlan { get; }

        /// <summary>
        ///     Returns lookup plan.
        /// </summary>
        /// <returns>lookup plan</returns>
        public TableLookupPlanForge LookupStrategySpec => TableLookupPlan;

        protected internal override void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "TableLookupNode " +
                " tableLookupPlan=" + TableLookupPlan);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<TableLookupNode>(TableLookupPlan.Make(parent, symbols, classScope));
        }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            usedIndexes.AddAll(TableLookupPlan.IndexNum);
        }

        public override void Accept(QueryPlanNodeForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace