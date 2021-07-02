///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    ///     A factory for row-limit processor instances.
    /// </summary>
    public class RowLimitProcessorFactory
    {
        private int currentOffset;
        private int currentRowLimit;

        private Variable numRowsVariable;
        private Variable offsetVariable;

        public Variable NumRowsVariable {
            set => numRowsVariable = value;
        }

        public Variable OffsetVariable {
            set => offsetVariable = value;
        }

        public int CurrentRowLimit {
            set => currentRowLimit = value;
        }

        public int CurrentOffset {
            set => currentOffset = value;
        }

        public RowLimitProcessor Instantiate(ExprEvaluatorContext exprEvaluatorContext)
        {
            VariableReader numRowsVariableReader = null;
            if (numRowsVariable != null) {
                numRowsVariableReader = exprEvaluatorContext.VariableManagementService.GetReader(
                    numRowsVariable.DeploymentId,
                    numRowsVariable.MetaData.VariableName,
                    exprEvaluatorContext.AgentInstanceId);
            }

            VariableReader offsetVariableReader = null;
            if (offsetVariable != null) {
                offsetVariableReader = exprEvaluatorContext.VariableManagementService.GetReader(
                    offsetVariable.DeploymentId,
                    offsetVariable.MetaData.VariableName,
                    exprEvaluatorContext.AgentInstanceId);
            }

            return new RowLimitProcessor(numRowsVariableReader, offsetVariableReader, currentRowLimit, currentOffset);
        }
    }
} // end of namespace