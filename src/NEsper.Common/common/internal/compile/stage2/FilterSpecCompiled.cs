///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

        /// <summary>
        ///     Constructor - validates parameter list against event type, throws exception if invalid
        ///     property names or mismatching filter operators are found.
        /// </summary>
        /// <param name="eventType">is the event type</param>
        /// <param name="filterParameters">is a list of filter parameters</param>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="optionalPropertyEvaluator">optional if evaluating properties returned by filtered events</param>
        /// <throws>IllegalArgumentException if validation invalid</throws>
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

        public int FilterCallbackId { get; set; } = -1;

        /// <summary>
        ///     Returns type of event to filter for.
        /// </summary>
        /// <value>event type</value>
        public EventType FilterForEventType { get; }

        /// <summary>
        ///     Returns list of filter parameters.
        /// </summary>
        /// <value>list of filter params</value>
        public FilterSpecPlanForge Parameters { get; }

        /// <summary>
        ///     Returns the event type name.
        /// </summary>
        /// <value>event type name</value>
        public string FilterForEventTypeName { get; }

        /// <summary>
        ///     Return the evaluator for property value if any is attached, or none if none attached.
        /// </summary>
        /// <value>property evaluator</value>
        public PropertyEvaluatorForge OptionalPropertyEvaluator { get; }

        /// <summary>
        ///     Returns the result event type of the filter specification.
        /// </summary>
        /// <value>event type</value>
        public EventType ResultEventType =>
            OptionalPropertyEvaluator != null
                ? OptionalPropertyEvaluator.FragmentEventType
                : FilterForEventType;

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

            if (!(obj is FilterSpecCompiled other)) {
                return false;
            }

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
            if (FilterForEventType != other.FilterForEventType) {
                return false;
            }

            return Parameters.EqualsFilter(other.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = FilterForEventType.GetHashCode();
            foreach (var path in Parameters.Paths) {
                foreach (var triplet in path.Triplets) {
                    hashCode ^= 31 * triplet.GetHashCode();
                }
            }

            return hashCode;
        }

        internal static FilterSpecPlanForge SortRemoveDups(FilterSpecPlanForge parameters)
        {
            var processed = new FilterSpecPlanPathForge[parameters.Paths.Length];
            for (var i = 0; i < parameters.Paths.Length; i++) {
                processed[i] = SortRemoveDups(parameters.Paths[i]);
            }

            return new FilterSpecPlanForge(
                processed,
                parameters.FilterConfirm,
                parameters.FilterNegate,
                parameters.ConvertorForge);
        }

        internal static FilterSpecPlanPathForge SortRemoveDups(FilterSpecPlanPathForge parameters)
        {
            if (parameters.Triplets.Length <= 1) {
                return parameters;
            }

            var result = new ArrayDeque<FilterSpecPlanPathTripletForge>();
            var map =
                new SortedDictionary<FilterOperator, IList<FilterSpecPlanPathTripletForge>>(COMPARATOR_PARAMETERS);
            foreach (var parameter in parameters.Triplets) {
                var list = map.Get(parameter.Param.FilterOperator);
                if (list == null) {
                    list = new List<FilterSpecPlanPathTripletForge>();
                    map.Put(parameter.Param.FilterOperator, list);
                }

                var hasDuplicate = false;
                foreach (var existing in list) {
                    if (existing.Param.Lookupable.Equals(parameter.Param.Lookupable)) {
                        hasDuplicate = true;
                        break;
                    }
                }

                if (hasDuplicate) {
                    continue;
                }

                list.Add(parameter);
            }

            foreach (var entry in map) {
                result.AddAll(entry.Value);
            }

            var triplets = result.ToArray();
            return new FilterSpecPlanPathForge(triplets, parameters.PathNegate);
        }

        public CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FilterSpecActivatable), typeof(FilterSpecCompiled), classScope);

            if (FilterCallbackId == -1) {
                throw new IllegalStateException("Unassigned filter callback id");
            }

            var propertyEval = OptionalPropertyEvaluator == null
                ? ConstantNull()
                : OptionalPropertyEvaluator.Make(method, symbols, classScope);
            method.Block
                .DeclareVar<EventType>("eventType",
                    EventTypeUtility.ResolveTypeCodegen(FilterForEventType, EPStatementInitServicesConstants.REF))
                .DeclareVar<FilterSpecPlan>("plan",
                    Parameters.CodegenWithEventType(
                        method,
                        Ref("eventType"),
                        symbols.GetAddInitSvc(method),
                        classScope))
                .DeclareVar<FilterSpecActivatable>("activatable",
                    NewInstance(
                        typeof(FilterSpecActivatable),
                        SAIFFInitializeSymbolWEventType.REF_EVENTTYPE,
                        Constant(FilterForEventType.Name),
                        Ref("plan"),
                        propertyEval,
                        Constant(FilterCallbackId)))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERSPECACTIVATABLEREGISTRY)
                        .Add("Register", Ref("activatable")))
                .MethodReturn(Ref("activatable"));

            return method;
        }

        public static IList<FilterSpecParamExprNodeForge> MakeExprNodeList(
            IList<FilterSpecTracked> filterSpecCompileds,
            IList<FilterSpecParamExprNodeForge> additionalBooleanExpressions)
        {
            ISet<FilterSpecParamExprNodeForge> boolExprs = new LinkedHashSet<FilterSpecParamExprNodeForge>();
            foreach (var spec in filterSpecCompileds) {
                spec.FilterSpecCompiled.TraverseFilterBooleanExpr(_ => boolExprs.Add(_));
            }

            boolExprs.AddAll(additionalBooleanExpressions);
            return new List<FilterSpecParamExprNodeForge>(boolExprs);
        }

        public void TraverseFilterBooleanExpr(Consumer<FilterSpecParamExprNodeForge> consumer)
        {
            foreach (var path in Parameters.Paths) {
                foreach (var triplet in path.Triplets) {
                    if (triplet.Param is FilterSpecParamExprNodeForge forge) {
                        consumer.Invoke(forge);
                    }
                }
            }
        }
    }
} // end of namespace