///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    [OutputTypes]
    [OutputType(Name = "line", Type = typeof(int))]
    [OutputType(Name = "wordCount", Type = typeof(int))]
    [OutputType(Name = "charCount", Type = typeof(int))]
    public class MyTokenizerCounter : DataFlowOperatorForge,
        DataFlowOperatorFactory,
        DataFlowOperator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataFlowContext] private EPDataFlowEmitter graphContext;

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new MyTokenizerCounter();
        }

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(MyTokenizerCounter));
        }

        public void OnInput(string line)
        {
            var tokens = line.Split('\t', ' ');
            var wordCount = tokens.Length;
            var charCount = tokens.Sum(token => token.Length);

            Log.Debug("Submitting stat words[" + wordCount + "] chars[" + charCount + "] for line '" + line + "'");
            graphContext.Submit(new object[] {1, wordCount, charCount});
        }
    }
} // end of namespace