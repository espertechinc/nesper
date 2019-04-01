///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Result set processor prototye for the hand-through case:
	/// no aggregation functions used in the select clause, and no group-by, no having and ordering.
	/// </summary>
	public class ResultSetProcessorHandThroughFactory : ResultSetProcessorFactory
	{
	    private readonly SelectExprProcessor _selectExprProcessor;
	    private readonly bool _isSelectRStream;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="selectExprProcessor">for processing the select expression and generting the final output rowsa row per group even if groups didn't change
	    /// </param>
	    /// <param name="selectRStream">true if remove stream events should be generated</param>
	    public ResultSetProcessorHandThroughFactory(SelectExprProcessor selectExprProcessor, bool selectRStream)
        {
	        _selectExprProcessor = selectExprProcessor;
	        _isSelectRStream = selectRStream;
	    }

	    public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
	        return new ResultSetProcessorHandThrough(this, _selectExprProcessor, agentInstanceContext);
	    }

	    public EventType ResultEventType
	    {
	        get { return _selectExprProcessor.ResultEventType; }
	    }

	    public bool HasAggregation
	    {
	        get { return false; }
	    }

	    public bool IsSelectRStream
	    {
	        get { return _isSelectRStream; }
	    }

	    public ResultSetProcessorType ResultSetProcessorType
	    {
	        get { return ResultSetProcessorType.HANDTHROUGH; }
	    }
	}
} // end of namespace
