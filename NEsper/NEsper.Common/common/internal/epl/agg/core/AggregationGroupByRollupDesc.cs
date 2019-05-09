///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationGroupByRollupDesc
    {
        public AggregationGroupByRollupDesc(AggregationGroupByRollupLevel[] levels)
        {
            Levels = levels;
            NumLevelsAggregation = levels.Count(level => !level.IsAggregationTop);
        }

        public AggregationGroupByRollupLevel[] Levels { get; }

        public int NumLevelsAggregation { get; }

        public int NumLevels => Levels.Length;

        public static AggregationGroupByRollupDesc Make(int[][] indexes)
        {
            var levels = new List<AggregationGroupByRollupLevel>();
            var countOffset = 0;
            var countNumber = -1;
            foreach (var mki in indexes) {
                countNumber++;
                if (mki.Length == 0) {
                    levels.Add(new AggregationGroupByRollupLevel(countNumber, -1, null));
                }
                else {
                    levels.Add(new AggregationGroupByRollupLevel(countNumber, countOffset, mki));
                    countOffset++;
                }
            }

            var levelsarr = levels.ToArray();
            return new AggregationGroupByRollupDesc(levelsarr);
        }

        public CodegenExpression Codegen()
        {
            var levels = Levels;
            var level = new CodegenExpression[levels.Length];
            for (var i = 0; i < levels.Length; i++) {
                level[i] = levels[i].ToExpression();
            }

            return CodegenExpressionBuilder.NewInstance(
                typeof(AggregationGroupByRollupDesc),
                CodegenExpressionBuilder.NewArrayWithInit(
                    typeof(AggregationGroupByRollupLevel), level));
        }
    }
}