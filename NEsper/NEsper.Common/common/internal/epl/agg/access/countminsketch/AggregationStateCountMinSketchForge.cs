///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationStateCountMinSketchForge : AggregationStateFactoryForge
    {
        internal readonly ExprAggMultiFunctionCountMinSketchNode parent;
        internal readonly CountMinSketchSpecForge specification;
        internal AggregatorAccessCountMinSketch aggregator;

        public AggregationStateCountMinSketchForge(
            ExprAggMultiFunctionCountMinSketchNode parent,
            CountMinSketchSpecForge specification)
        {
            this.parent = parent;
            this.specification = specification;
        }

        public void InitAccessForge(
            int col,
            bool join,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            aggregator = new AggregatorAccessCountMinSketch(this, col, rowCtor, membersColumnized, classScope);
        }

        public AggregatorAccess Aggregator => aggregator;

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return AggregatorAccessCountMinSketch.CodegenGetAccessTableState(column, parent, classScope);
        }

        public ExprNode Expression => parent;
    }
} // end of namespace