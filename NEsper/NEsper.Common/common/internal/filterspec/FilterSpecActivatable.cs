///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.filterspec
{
	/// <summary>
	/// Contains the filter criteria to sift through events. The filter criteria are the event class to look for and
	/// a set of parameters (attribute names, operators and constant/range values).
	/// </summary>
	public class FilterSpecActivatable {
	    private readonly EventType filterForEventType;
	    private readonly string filterForEventTypeName;
	    private readonly FilterSpecParam[][] parameters;
	    private readonly PropertyEvaluator optionalPropertyEvaluator;
	    private readonly int filterCallbackId;

	    /// <summary>
	    /// Constructor - validates parameter list against event type, throws exception if invalid
	    /// property names or mismatcing filter operators are found.
	    /// </summary>
	    /// <param name="eventType">is the event type</param>
	    /// <param name="filterParameters">is a list of filter parameters</param>
	    /// <param name="eventTypeName">is the name of the event type</param>
	    /// <param name="optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
	    /// <param name="filterCallbackId">filter id</param>
	    /// <throws>IllegalArgumentException if validation invalid</throws>
	    public FilterSpecActivatable(EventType eventType, string eventTypeName, FilterSpecParam[][] filterParameters,
	                                 PropertyEvaluator optionalPropertyEvaluator, int filterCallbackId) {
	        this.filterForEventType = eventType;
	        this.filterForEventTypeName = eventTypeName;
	        this.parameters = filterParameters;
	        this.optionalPropertyEvaluator = optionalPropertyEvaluator;
	        if (filterCallbackId == -1) {
	            throw new ArgumentException("Filter callback id is unassigned");
	        }
	        this.filterCallbackId = filterCallbackId;
	    }

	    /// <summary>
	    /// Returns type of event to filter for.
	    /// </summary>
	    /// <returns>event type</returns>
	    public EventType FilterForEventType
	    {
	        get => filterForEventType;
	    }

	    /// <summary>
	    /// Returns list of filter parameters.
	    /// </summary>
	    /// <returns>list of filter params</returns>
	    public FilterSpecParam[][] Parameters
	    {
	        get => parameters;
	    }

	    /// <summary>
	    /// Returns the event type name.
	    /// </summary>
	    /// <returns>event type name</returns>
	    public string FilterForEventTypeName
	    {
	        get => filterForEventTypeName;
	    }

	    /// <summary>
	    /// Return the evaluator for property value if any is attached, or none if none attached.
	    /// </summary>
	    /// <returns>property evaluator</returns>
	    public PropertyEvaluator OptionalPropertyEvaluator
	    {
	        get => optionalPropertyEvaluator;
	    }

	    /// <summary>
	    /// Returns the result event type of the filter specification.
	    /// </summary>
	    /// <value>event type</value>
	    public EventType ResultEventType
	    {
	        get
	        {
	            if (optionalPropertyEvaluator != null)
	            {
	                return optionalPropertyEvaluator.FragmentEventType;
	            }
	            else
	            {
	                return filterForEventType;
	            }
	        }
	    }

	    public FilterValueSetParam[][] GetValueSet(
	        MatchedEventMap matchedEvents, 
	        FilterValueSetParam[][] addendum, 
	        ExprEvaluatorContext exprEvaluatorContext, 
	        StatementContextFilterEvalEnv filterEvalEnv)
	    {
	        FilterValueSetParam[][] valueList = EvaluateValueSet(parameters, matchedEvents, exprEvaluatorContext, filterEvalEnv);
	        if (addendum != null)
	        {
	            valueList = FilterAddendumUtil.MultiplyAddendum(addendum, valueList);
	        }
	        return valueList;
	    }


	    public static FilterValueSetParam[][] EvaluateValueSet(
	        FilterSpecParam[][] parameters, 
	        MatchedEventMap matchedEvents, 
	        AgentInstanceContext agentInstanceContext)
	    {
	        return EvaluateValueSet(parameters, matchedEvents, agentInstanceContext, agentInstanceContext.StatementContextFilterEvalEnv);
	    }

	    public static FilterValueSetParam[][] EvaluateValueSet(
	        FilterSpecParam[][] parameters, 
	        MatchedEventMap matchedEvents, 
	        ExprEvaluatorContext exprEvaluatorContext, 
	        StatementContextFilterEvalEnv filterEvalEnv)
	    {
	        FilterValueSetParam[][] valueList = new FilterValueSetParam[parameters.Length][];
	        for (int i = 0; i < parameters.Length; i++)
	        {
	            valueList[i] = new FilterValueSetParam[parameters[i].Length];
	            PopulateValueSet(valueList[i], matchedEvents, parameters[i], exprEvaluatorContext, filterEvalEnv);
	        }
	        return valueList;
	    }

        private static void PopulateValueSet(
	        FilterValueSetParam[] valueList, 
	        MatchedEventMap matchedEvents, 
	        FilterSpecParam[] specParams, 
	        ExprEvaluatorContext exprEvaluatorContext, 
	        StatementContextFilterEvalEnv filterEvalEnv)
	    {
	        // Ask each filter specification parameter for the actual value to filter for
	        int count = 0;
	        foreach (FilterSpecParam specParam in specParams) {
	            var filterForValue = specParam.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
	            valueList[count] = new FilterValueSetParamImpl(
	                specParam.Lookupable, specParam.FilterOperator, filterForValue);
	            count++;
	        }
	    }


	    public override String ToString()
	    {
	        var stringBuilder = new StringBuilder();
	        return stringBuilder
	            .Append("FilterSpecActivatable type=" + this.filterForEventType)
	            .Append(" parameters=" + parameters.RenderAny())
	            .ToString();
	    }


        public override bool Equals(Object obj)
	    {
	        if (this == obj)
	        {
	            return true;
	        }

	        if (!(obj is FilterSpecActivatable)) {
	            return false;
	        }

	        FilterSpecActivatable other = (FilterSpecActivatable)obj;
	        if (!EqualsTypeAndFilter(other))
	        {
	            return false;
	        }

	        if ((this.optionalPropertyEvaluator == null) && (other.optionalPropertyEvaluator == null)) {
	            return true;
	        }
	        if ((this.optionalPropertyEvaluator != null) && (other.optionalPropertyEvaluator == null)) {
	            return false;
	        }
	        if ((this.optionalPropertyEvaluator == null) && (other.optionalPropertyEvaluator != null)) {
	            return false;
	        }

	        return this.optionalPropertyEvaluator.CompareTo(other.optionalPropertyEvaluator);
	    }


        /// <summary>
        /// Returns the values for the filter, using the supplied result events to ask filter parameters
        /// for the value to filter for.
        /// </summary>
        /// <returns>filter values</returns>
        public bool EqualsTypeAndFilter(FilterSpecActivatable other) {
	        if (this.filterForEventType != other.filterForEventType) {
	            return false;
	        }
	        if (this.parameters.Length != other.parameters.Length) {
	            return false;
	        }

	        for (int i = 0; i < this.parameters.Length; i++) {
	            FilterSpecParam[] lineThis = this.parameters[i];
	            FilterSpecParam[] lineOther = other.parameters[i];
	            if (lineThis.Length != lineOther.Length) {
	                return false;
	            }

	            for (int j = 0; j < lineThis.Length; j++) {
	                if (!lineThis[j].Equals(lineOther[j])) {
	                    return false;
	                }
	            }
	        }
	        return true;
	    }
        
        public override int GetHashCode() {
	        int hashCode = filterForEventType.GetHashCode();
	        foreach (FilterSpecParam[] paramLine in parameters) {
	            foreach (FilterSpecParam param in paramLine) {
	                hashCode ^= 31 * param.GetHashCode();
	            }
	        }
	        return hashCode;
	    }

	    public int FilterCallbackId
	    {
	        get => filterCallbackId;
	    }

	    public string GetFilterText() {
	        StringWriter writer = new StringWriter();
	        writer.Write(FilterForEventType.Name);
	        if (Parameters != null && Parameters.Length > 0) {
	            writer.Write('(');
	            string delimiter = "";
	            foreach (FilterSpecParam[] paramLine in Parameters) {
	                writer.Write(delimiter);
	                WriteFilter(writer, paramLine);
	                delimiter = " or ";
	            }
	            writer.Write(')');
	        }
	        return writer.ToString();
	    }

	    private static void WriteFilter(StringWriter writer, FilterSpecParam[] paramLine) {
	        string delimiter = "";
	        foreach (FilterSpecParam param in paramLine) {
	            writer.Write(delimiter);
	            writer.Write(param.Lookupable.Expression);
	            writer.Write(param.FilterOperator.GetTextualOp());
	            writer.Write("...");
	            delimiter = ",";
	        }
	    }
	}
} // end of namespace