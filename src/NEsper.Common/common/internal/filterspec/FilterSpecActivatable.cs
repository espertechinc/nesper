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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

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
        /// <param name = "eventType">is the event type</param>
        /// <param name = "plan">plan is a list of filter parameters, i.e. paths and triplets</param>
        /// <param name = "eventTypeName">is the name of the event type</param>
        /// <param name = "optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
        /// <param name = "filterCallbackId">filter id</param>
        /// <throws>ArgumentException if validation invalid</throws>
        public FilterSpecActivatable(
            EventType eventType,
            string eventTypeName,
            FilterSpecPlan plan,
            PropertyEvaluator optionalPropertyEvaluator,
            int filterCallbackId)
        {
            FilterForEventType = eventType;
            FilterForEventTypeName = eventTypeName;
            Plan = plan;
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
        ///     Returns the filter plan.  The plan is a list of filter parameters, i.e. paths and triplets
        /// </summary>
        public FilterSpecPlan Plan { get; }

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

        /// <summary>
        /// Returns the values for the filter, using the supplied result events to ask filter parameters
        /// for the value to filter for.
        /// </summary>
        /// <param name = "matchedEvents">contains the result events to use for determining filter values</param>
        /// <param name = "addendum">context addendum</param>
        /// <param name = "exprEvaluatorContext">context</param>
        /// <param name = "filterEvalEnv">env</param>
        /// <returns>filter values, or null when negated</returns>
        public FilterValueSetParam[][] GetValueSet(
            MatchedEventMap matchedEvents,
            FilterValueSetParam[][] addendum,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var valueList = Plan.EvaluateValueSet(matchedEvents, exprEvaluatorContext, filterEvalEnv);
            if (addendum != null) {
                valueList = FilterAddendumUtil.MultiplyAddendum(addendum, valueList);
            }

            return valueList;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            return stringBuilder.Append("FilterSpecActivatable type=" + FilterForEventType)
                .Append(" parameters=" + Plan)
                .ToString();
        }

        public override bool Equals(object obj)
        {
            return this == obj; // identity only
        }

        public override int GetHashCode()
        {
            var hashCode = FilterForEventType.GetHashCode();
            foreach (var path in Plan.Paths) {
                foreach (var triplet in path.Triplets) {
                    hashCode ^= 31 * triplet.Param.GetHashCode();
                }
            }

            return hashCode;
        }

        private static void WriteFilter(
            TextWriter writer,
            FilterSpecPlanPath path)
        {
            var delimiter = "";
            foreach (var triplet in path.Triplets) {
                writer.Write(delimiter);
                writer.Write(triplet.Param.Lkupable.Expression);
                writer.Write(triplet.Param.FilterOperator.GetTextualOp());
                writer.Write("...");
                delimiter = ",";
            }
        }

        public string FilterText {
            get {
                var writer = new StringWriter();
                writer.Write(FilterForEventType.Name);
                if (Plan.Paths != null && Plan.Paths.Length > 0) {
                    writer.Write('(');
                    var delimiter = "";
                    foreach (var path in Plan.Paths) {
                        writer.Write(delimiter);
                        WriteFilter(writer, path);
                        delimiter = " or ";
                    }

                    writer.Write(')');
                }

                return writer.ToString();
            }
        }
    }
} // end of namespace