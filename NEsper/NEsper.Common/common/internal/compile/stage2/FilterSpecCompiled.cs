///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Contains the filter criteria to sift through events. The filter criteria are the event class to look for and
    ///     a set of parameters (attribute names, operators and constant/range values).
    /// </summary>
    public class FilterSpecCompiled
    {
        private static readonly FilterSpecParamComparator COMPARATOR_PARAMETERS = new FilterSpecParamComparator();

        private int filterCallbackId = -1;

        /// <summary>
        ///     Constructor - validates parameter list against event type, throws exception if invalid
        ///     property names or mismatcing filter operators are found.
        /// </summary>
        /// <param name="eventType">is the event type</param>
        /// <param name="filterParameters">is a list of filter parameters</param>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
        /// <throws>ArgumentException if validation invalid</throws>
        public FilterSpecCompiled(
            EventType eventType,
            string eventTypeName,
            FilterSpecPlanForge filterParameters,
            PropertyEvaluatorForge optionalPropertyEvaluator)
        {
            FilterForEventType = eventType;
            FilterForEventTypeName = eventTypeName;
            Parameters = SortRemoveDups(filterParameters);
            OptionalPropertyEvaluator = optionalPropertyEvaluator;
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
        public FilterSpecPlanForge Parameters { get; }

        /// <summary>
        ///     Returns the event type name.
        /// </summary>
        /// <returns>event type name</returns>
        public string FilterForEventTypeName { get; }

        /// <summary>
        ///     Return the evaluator for property value if any is attached, or none if none attached.
        /// </summary>
        /// <returns>property evaluator</returns>
        public PropertyEvaluatorForge OptionalPropertyEvaluator { get; }

        public int FilterCallbackId {
            set { this.filterCallbackId = value; }
        }

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

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("FilterSpecCompiled type=" + FilterForEventType);
            buffer.Append(" parameters=" + Parameters);
            return buffer.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecCompiled)) {
                return false;
            }

            var other = (FilterSpecCompiled) obj;
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
        ///     Compares only the type and filter portion and not the property evaluation portion.
        /// </summary>
        /// <param name="other">filter to compare</param>
        /// <returns>true if same</returns>
        public bool EqualsTypeAndFilter(FilterSpecCompiled other)
        {
            return Equals(FilterForEventType, other.FilterForEventType) && Parameters.EqualsFilter(other.Parameters);
        }

        public override int GetHashCode()
        {
            int hashCode = FilterForEventType.GetHashCode();
            foreach (FilterSpecPlanPathForge path in Parameters.Paths) {
                foreach (FilterSpecPlanPathTripletForge triplet in path.Triplets) {
                    hashCode ^= 31 * triplet.GetHashCode();
                }
            }

            return hashCode;
        }

        public static FilterSpecPlanForge SortRemoveDups(FilterSpecPlanForge parameters)
        {
            var processed = parameters.Paths
                .Select(v => SortRemoveDups(v))
                .ToArray();

            return new FilterSpecPlanForge(
                processed,
                parameters.FilterConfirm,
                parameters.FilterNegate,
                parameters.ConvertorForge);
        }

        public static FilterSpecPlanPathForge SortRemoveDups(FilterSpecPlanPathForge parameters)
        {
            if (parameters.Triplets.Length <= 1) {
                return parameters;
            }

            var map = new OrderedListDictionary<FilterOperator, IList<FilterSpecPlanPathTripletForge>>(COMPARATOR_PARAMETERS);

            foreach (FilterSpecPlanPathTripletForge parameter in parameters.Triplets) {
                if (!map.TryGetValue(parameter.Param.FilterOperator, out var list)) {
                    list = new List<FilterSpecPlanPathTripletForge>();
                    map[parameter.Param.FilterOperator] = list;
                }

                var hasDuplicate = list.Any(existing => Equals(existing.Param.Lookupable, parameter.Param.Lookupable));
                if (hasDuplicate) {
                    continue;
                }

                list.Add(parameter);
            }

            var result = new List<FilterSpecPlanPathTripletForge>();
            foreach (var entry in map) {
                result.AddAll(entry.Value);
            }

            return new FilterSpecPlanPathForge(
                result.ToArray(),
                parameters.PathNegate);
        }

        public CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FilterSpecActivatable), typeof(FilterSpecCompiled), classScope);

            if (filterCallbackId == -1) {
                throw new IllegalStateException("Unassigned filter callback id");
            }

            var propertyEval = OptionalPropertyEvaluator == null
                ? ConstantNull()
                : OptionalPropertyEvaluator.Make(method, symbols, classScope);
            method.Block
                .DeclareVar<EventType>(
                    "eventType",
                    EventTypeUtility.ResolveTypeCodegen(FilterForEventType, EPStatementInitServicesConstants.REF))
                .DeclareVar<FilterSpecPlan>(
                    "plan",
                    LocalMethod(
                        Parameters.CodegenWithEventType(method, classScope),
                        Ref("eventType"),
                        symbols.GetAddInitSvc(method)))
                .DeclareVar<FilterSpecActivatable>(
                    "activatable",
                    NewInstance<FilterSpecActivatable>(
                        SAIFFInitializeSymbolWEventType.REF_EVENTTYPE,
                        Constant(FilterForEventType.Name),
                        Ref("plan"),
                        propertyEval,
                        Constant(filterCallbackId)))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERSPECACTIVATABLEREGISTRY)
                        .Add("Register", Ref("activatable")))
                .MethodReturn(Ref("activatable"));

            return method;
        }

        public static IList<FilterSpecParamExprNodeForge> MakeExprNodeList(
            IList<FilterSpecCompiled> filterSpecCompileds,
            IList<FilterSpecParamExprNodeForge> additionalBooleanExpressions)
        {
            var boolExprs = new LinkedHashSet<FilterSpecParamExprNodeForge>();
            foreach (var spec in filterSpecCompileds) {
                spec.TraverseFilterBooleanExpr(v => boolExprs.Add(v));
            }

            boolExprs.AddAll(additionalBooleanExpressions);
            return new List<FilterSpecParamExprNodeForge>(boolExprs);
        }

        public void TraverseFilterBooleanExpr(Consumer<FilterSpecParamExprNodeForge> consumer)
        {
            foreach (var path in Parameters.Paths) {
                foreach (var triplet in path.Triplets) {
                    if (triplet.Param is FilterSpecParamExprNodeForge filterSpecParamExprNodeForge) {
                        consumer.Invoke(filterSpecParamExprNodeForge);
                    }
                }
            }
        }
    }
} // end of namespace