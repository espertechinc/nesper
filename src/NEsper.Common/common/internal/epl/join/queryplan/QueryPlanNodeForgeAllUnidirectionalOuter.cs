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
using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class QueryPlanNodeForgeAllUnidirectionalOuter : QueryPlanNodeForge
    {
        private readonly int _streamNum;

        public QueryPlanNodeForgeAllUnidirectionalOuter(int streamNum)
        {
            this._streamNum = streamNum;
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<QueryPlanNodeAllUnidirectionalOuter>(Constant(_streamNum));
        }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
        }

        protected internal override void Print(IndentWriter writer)
        {
            writer.WriteLine("Unidirectional Full-Outer-Join-All Execution");
        }

        public override void Accept(QueryPlanNodeForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace