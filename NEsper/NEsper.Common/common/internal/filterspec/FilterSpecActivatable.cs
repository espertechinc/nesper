///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     Contains the filter criteria to sift through events. The filter criteria are the event class to look for and
    ///     a set of parameters (attribute names, operators and constant/range values).
    /// </summary>
    public class FilterSpecActivatable
    {
        /// <summary>
        ///     Constructor - validates parameter list against event type, throws exception if invalid
        ///     property names or mismatcing filter operators are found.
        /// </summary>
        /// <param name="eventType">is the event type</param>
        /// <param name="filterParameters">is a list of filter parameters</param>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
        /// <param name="filterCallbackId">filter id</param>
        /// <throws>ArgumentException if validation invalid</throws>
        public FilterSpecActivatable(
            EventType eventType,
            string eventTypeName,
            FilterSpecParam[][] filterParameters,
            PropertyEvaluator optionalPropertyEvaluator,
            int filterCallbackId)
        {
            FilterForEventType = eventType;
            FilterForEventTypeName = eventTypeName;
            Parameters = filterParameters;
            OptionalPropertyEvaluator = optionalPropertyEvaluator;
            if (filterCallbackId == -1) {
                throw new ArgumentException("Filter callback id is unassigned");
            }

            FilterCallbackId = filterCallbackId;
        }

        /// <summary>
        ///     Returns type of event to filter for.
        /// </summary>
        /// <returns>event type</returns>
        public EventType FilterForEventType { get; }

        /// <summary>
        ///     Returns list of filter parameters.
        /// </summary>
        /// <returns>list of filter params</returns>
        public FilterSpecParam[][] Parameters { get; }

        /// <summary>
        ///     Returns the event type name.
        /// </summary>
        /// <returns>event type name</returns>
        public string FilterForEventTypeName { get; }

        /// <summary>
        ///     Return the evaluator for property value if any is attached, or none if none attached.
        /// </summary>
        /// <returns>property evaluator</returns>
        public PropertyEvaluator OptionalPropertyEvaluator { get; }

        /// <summary>
        ///     Returns the result event type of the filter specification.
        /// </summary>
        /// <value>event type</value>
        public EventType ResultEventType {
            get {
                if (OptionalPropertyEvaluator != null) {
                    return OptionalPropertyEvaluator.FragmentEventType;
                }

                return FilterForEventType;
            }
        }

        public int FilterCallbackId { get; }

        public FilterValueSetParam[][] GetValueSet(
            MatchedEventMap matchedEvents,
            FilterValueSetParam[][] addendum,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var valueList = EvaluateValueSet(Parameters, matchedEvents, exprEvaluatorContext, filterEvalEnv);
            if (addendum != null) {
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
            var valueList = new FilterValueSetParam[parameters.Length][];
            for (var i = 0; i < parameters.Length; i++) {
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
            var count = 0;
            foreach (var specParam in specParams) {
                var filterForValue = specParam.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                valueList[count] = new FilterValueSetParamImpl(
                    specParam.Lookupable, specParam.FilterOperator, filterForValue);
                count++;
            }
        }


        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            return stringBuilder
                .Append("FilterSpecActivatable type=" + FilterForEventType)
                .Append(" parameters=" + Parameters.RenderAny())
                .ToString();
        }


        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecActivatable)) {
                return false;
            }

            var other = (FilterSpecActivatable) obj;
            if (!EqualsTypeAndFilter(other)) {
                return false;
            }

            if (OptionalPropertyEvaluator == null && other.OptionalPropertyEvaluator == null) {
                return true;
            }

            if (OptionalPropertyEvaluator != null && other.OptionalPropertyEvaluator == null) {
                return false;
            }

            if (OptionalPropertyEvaluator == null && other.OptionalPropertyEvaluator != null) {
                return false;
            }

            return OptionalPropertyEvaluator.CompareTo(other.OptionalPropertyEvaluator);
        }


        /// <summary>
        ///     Returns the values for the filter, using the supplied result events to ask filter parameters
        ///     for the value to filter for.
        /// </summary>
        /// <returns>filter values</returns>
        public bool EqualsTypeAndFilter(FilterSpecActivatable other)
        {
            if (FilterForEventType != other.FilterForEventType) {
                return false;
            }

            if (Parameters.Length != other.Parameters.Length) {
                return false;
            }

            for (var i = 0; i < Parameters.Length; i++) {
                var lineThis = Parameters[i];
                var lineOther = other.Parameters[i];
                if (lineThis.Length != lineOther.Length) {
                    return false;
                }

                for (var j = 0; j < lineThis.Length; j++) {
                    if (!lineThis[j].Equals(lineOther[j])) {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = FilterForEventType.GetHashCode();
            foreach (var paramLine in Parameters) {
                foreach (var param in paramLine) {
                    hashCode ^= 31 * param.GetHashCode();
                }
            }

            return hashCode;
        }

        public string GetFilterText()
        {
            var writer = new StringWriter();
            writer.Write(FilterForEventType.Name);
            if (Parameters != null && Parameters.Length > 0) {
                writer.Write('(');
                var delimiter = "";
                foreach (var paramLine in Parameters) {
                    writer.Write(delimiter);
                    WriteFilter(writer, paramLine);
                    delimiter = " or ";
                }

                writer.Write(')');
            }

            return writer.ToString();
        }

        private static void WriteFilter(
            TextWriter writer,
            FilterSpecParam[] paramLine)
        {
            var delimiter = "";
            foreach (var param in paramLine) {
                writer.Write(delimiter);
                writer.Write(param.Lookupable.Expression);
                writer.Write(param.FilterOperator.GetTextualOp());
                writer.Write("...");
                delimiter = ",";
            }
        }
    }
} // end of namespace