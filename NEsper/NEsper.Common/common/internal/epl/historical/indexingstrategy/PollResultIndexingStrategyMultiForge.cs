///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyMultiForge : PollResultIndexingStrategyForge
    {
        private readonly PollResultIndexingStrategyForge[] indexingStrategies;
        private readonly int streamNum;

        public PollResultIndexingStrategyMultiForge(
            int streamNum,
            PollResultIndexingStrategyForge[] indexingStrategies)
        {
            this.streamNum = streamNum;
            this.indexingStrategies = indexingStrategies;
        }

        public string ToQueryPlan()
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var strategy in indexingStrategies) {
                writer.Write(delimiter);
                writer.Write(strategy.ToQueryPlan());
                delimiter = ", ";
            }

            return GetType().Name + " " + writer;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PollResultIndexingStrategyMulti), GetType(), classScope);

            method.Block.DeclareVar(
                typeof(PollResultIndexingStrategy[]), "strats",
                NewArrayByLength(typeof(PollResultIndexingStrategy), Constant(indexingStrategies.Length)));
            for (var i = 0; i < indexingStrategies.Length; i++) {
                method.Block.AssignArrayElement(
                    Ref("strats"), Constant(i), indexingStrategies[i].Make(method, symbols, classScope));
            }

            method.Block
                .DeclareVar(
                    typeof(PollResultIndexingStrategyMulti), "strat",
                    NewInstance(typeof(PollResultIndexingStrategyMulti)))
                .SetProperty(Ref("strat"), "IndexingStrategies", Ref("strats"))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace