///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class LogicalChannelProducingPortCompiled
    {
        public LogicalChannelProducingPortCompiled()
        {
        }

        public LogicalChannelProducingPortCompiled(
            int producingOpNum, string producingOpPrettyPrint, string streamName, int streamNumber,
            GraphTypeDesc graphTypeDesc, bool hasPunctuation)
        {
            ProducingOpNum = producingOpNum;
            ProducingOpPrettyPrint = producingOpPrettyPrint;
            StreamName = streamName;
            StreamNumber = streamNumber;
            GraphTypeDesc = graphTypeDesc;
            HasPunctuation = hasPunctuation;
        }

        public string ProducingOpPrettyPrint { get; set; }

        public int ProducingOpNum { get; set; }

        public string StreamName { get; set; }

        public int StreamNumber { get; set; }

        public bool HasPunctuation { get; set; }

        public GraphTypeDesc GraphTypeDesc { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    typeof(LogicalChannelProducingPortCompiled), GetType(), "c", parent, symbols, classScope)
                .Constant("producingOpNum", ProducingOpNum)
                .Constant("producingOpPrettyPrint", ProducingOpPrettyPrint)
                .Constant("streamName", StreamName)
                .Constant("streamNumber", StreamNumber)
                .Method("graphTypeDesc", method => GraphTypeDesc.Make(method, symbols, classScope))
                .Constant("hasPunctuation", HasPunctuation)
                .Build();
        }

        public override string ToString()
        {
            return "LogicalChannelProducingPort{" +
                   "op=" + ProducingOpPrettyPrint + '\'' +
                   ", streamName='" + StreamName + '\'' +
                   ", portNumber=" + StreamNumber +
                   ", hasPunctuation=" + HasPunctuation +
                   '}';
        }
    }
} // end of namespace