///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationGroupByRollupDescForge
    {
        public AggregationGroupByRollupDescForge(AggregationGroupByRollupLevelForge[] levels)
        {
            Levels = levels;

            var count = 0;
            foreach (var level in levels) {
                if (!level.IsAggregationTop) {
                    count++;
                }
            }

            NumLevelsAggregation = count;
        }

        public AggregationGroupByRollupLevelForge[] Levels { get; }

        public int NumLevelsAggregation { get; }

        public int NumLevels => Levels.Length;

        public CodegenExpression Codegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var level = new CodegenExpression[Levels.Length];
            for (var i = 0; i < Levels.Length; i++) {
                level[i] = Levels[i].Codegen(parent, classScope);
            }

            return NewInstance(typeof(AggregationGroupByRollupDesc), NewArrayWithInit(typeof(AggregationGroupByRollupLevel), level));
        }
    }
} // end of namespace