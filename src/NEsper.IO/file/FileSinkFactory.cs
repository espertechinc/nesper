///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esperio.file
{
    public class FileSinkFactory : DataFlowOperatorFactory
    {
        public ExprEvaluator File { get; set; }

        public ExprEvaluator Append { get; set; }

        public EventType EventType { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var fileName = DataFlowParameterResolution.ResolveStringRequired(
                "file", File, context);
            var appendFlag = DataFlowParameterResolution.ResolveWithDefault(
                "append", Append, false, context);
            return new FileSinkCSV(this, fileName, appendFlag);
        }
    }
} // end of namespace