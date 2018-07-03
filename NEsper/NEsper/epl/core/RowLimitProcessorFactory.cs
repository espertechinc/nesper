///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// A factory for row-limit processor instances.
	/// </summary>
	public class RowLimitProcessorFactory
    {
	    private readonly VariableMetaData _numRowsVariableMetaData;
	    private readonly VariableMetaData _offsetVariableMetaData;
	    private readonly int _currentRowLimit;
	    private readonly int _currentOffset;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rowLimitSpec">specification for row limit, or null if no row limit is defined</param>
        /// <param name="variableService">for retrieving variable state for use with row limiting</param>
        /// <param name="optionalContextName">Name of the optional context.</param>
        /// <exception cref="ExprValidationException">
        /// Limit clause variable by name '" + rowLimitSpec.NumRowsVariable + "' has not been declared
        /// or
        /// or
        /// Limit clause requires a variable of numeric type
        /// or
        /// Limit clause variable by name '" + rowLimitSpec.OptionalOffsetVariable + "' has not been declared
        /// or
        /// or
        /// Limit clause requires a variable of numeric type
        /// or
        /// Limit clause requires a positive offset
        /// </exception>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if row limit specification validation fails</throws>
        public RowLimitProcessorFactory(RowLimitSpec rowLimitSpec, VariableService variableService, string optionalContextName)
	    {
	        if (rowLimitSpec.NumRowsVariable != null)
	        {
	            _numRowsVariableMetaData = variableService.GetVariableMetaData(rowLimitSpec.NumRowsVariable);
	            if (_numRowsVariableMetaData == null) {
	                throw new ExprValidationException("Limit clause variable by name '" + rowLimitSpec.NumRowsVariable + "' has not been declared");
	            }
	            string message = VariableServiceUtil.CheckVariableContextName(optionalContextName, _numRowsVariableMetaData);
	            if (message != null) {
	                throw new ExprValidationException(message);
	            }
	            if (!TypeHelper.IsNumeric(_numRowsVariableMetaData.VariableType))
	            {
	                throw new ExprValidationException("Limit clause requires a variable of numeric type");
	            }
	        }
	        else
	        {
	            _numRowsVariableMetaData = null;
	            _currentRowLimit = rowLimitSpec.NumRows.GetValueOrDefault();

	            if (_currentRowLimit < 0)
	            {
	                _currentRowLimit = int.MaxValue;
	            }
	        }

	        if (rowLimitSpec.OptionalOffsetVariable != null)
	        {
	            _offsetVariableMetaData = variableService.GetVariableMetaData(rowLimitSpec.OptionalOffsetVariable);
	            if (_offsetVariableMetaData == null) {
	                throw new ExprValidationException("Limit clause variable by name '" + rowLimitSpec.OptionalOffsetVariable + "' has not been declared");
	            }
	            string message = VariableServiceUtil.CheckVariableContextName(optionalContextName, _offsetVariableMetaData);
	            if (message != null) {
	                throw new ExprValidationException(message);
	            }
	            if (!TypeHelper.IsNumeric(_offsetVariableMetaData.VariableType))
	            {
	                throw new ExprValidationException("Limit clause requires a variable of numeric type");
	            }
	        }
	        else
	        {
	            _offsetVariableMetaData = null;
	            if (rowLimitSpec.OptionalOffset != null)
	            {
	                _currentOffset = rowLimitSpec.OptionalOffset.GetValueOrDefault();

	                if (_currentOffset <= 0)
	                {
	                    throw new ExprValidationException("Limit clause requires a positive offset");
	                }
	            }
	            else
	            {
	                _currentOffset = 0;
	            }
	        }
	    }

	    public RowLimitProcessor Instantiate(AgentInstanceContext agentInstanceContext) {
	        VariableReader numRowsVariableReader = null;
	        if (_numRowsVariableMetaData != null) {
	            numRowsVariableReader = agentInstanceContext.StatementContext.VariableService.GetReader(_numRowsVariableMetaData.VariableName, agentInstanceContext.AgentInstanceId);
	        }

	        VariableReader offsetVariableReader = null;
	        if (_offsetVariableMetaData != null) {
	            offsetVariableReader = agentInstanceContext.StatementContext.VariableService.GetReader(_offsetVariableMetaData.VariableName, agentInstanceContext.AgentInstanceId);
	        }

	        return new RowLimitProcessor(numRowsVariableReader, offsetVariableReader,
	                _currentRowLimit, _currentOffset);
	    }
	}
} // end of namespace
