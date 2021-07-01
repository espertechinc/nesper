///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationStateFactoryForgePlugin : AggregationStateFactoryForge
    {
        private readonly AggregationForgeFactoryAccessPlugin forgeFactory;
        private readonly AggregationMultiFunctionStateModeManaged mode;
        private AggregatorAccessPlugin access;

        public AggregationStateFactoryForgePlugin(
            AggregationForgeFactoryAccessPlugin forgeFactory,
            AggregationMultiFunctionStateModeManaged mode)
        {
            this.forgeFactory = forgeFactory;
            this.mode = mode;
        }

        public void InitAccessForge(
            int col,
            bool join,
            CodegenCtor ctor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            access = new AggregatorAccessPlugin(
                col,
                join,
                ctor,
                membersColumnized,
                classScope,
                forgeFactory.AggregationExpression.OptionalFilter,
                mode);
        }

        public AggregatorAccess Aggregator {
            get => access;
        }

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return AggregatorAccessPlugin.CodegenGetAccessTableState(column);
        }

        public ExprNode Expression {
            get => forgeFactory.AggregationExpression;
        }
    }
} // end of namespace