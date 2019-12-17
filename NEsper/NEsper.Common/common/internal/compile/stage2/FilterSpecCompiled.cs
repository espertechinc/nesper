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
            IList<FilterSpecParamForge>[] filterParameters,
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
        public FilterSpecParamForge[][] Parameters { get; }

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
            buffer.Append(" parameters=" + CompatExtensions.RenderAny(Parameters));
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
            int hashCode = FilterForEventType.GetHashCode();
            foreach (var paramLine in Parameters) {
                foreach (var param in paramLine) {
                    hashCode ^= 31 * param.GetHashCode();
                }
            }

            return hashCode;
        }

        protected internal static FilterSpecParamForge[][] SortRemoveDups(IList<FilterSpecParamForge>[] parameters)
        {
            var processed = new FilterSpecParamForge[parameters.Length][];
            for (var i = 0; i < parameters.Length; i++) {
                processed[i] = SortRemoveDups(parameters[i]);
            }

            return processed;
        }

        protected internal static FilterSpecParamForge[] SortRemoveDups(IList<FilterSpecParamForge> parameters)
        {
            if (parameters.IsEmpty()) {
                return FilterSpecParamForge.EMPTY_PARAM_ARRAY;
            }

            if (parameters.Count == 1) {
                return new FilterSpecParamForge[] {parameters[0]};
            }

            ArrayDeque<FilterSpecParamForge> result = new ArrayDeque<FilterSpecParamForge>();
            OrderedDictionary<FilterOperator, IList<FilterSpecParamForge>> map =
                new OrderedDictionary<FilterOperator, IList<FilterSpecParamForge>>(COMPARATOR_PARAMETERS);
            foreach (var parameter in parameters) {
                var list = map.Get(parameter.FilterOperator);
                if (list == null) {
                    list = new List<FilterSpecParamForge>();
                    map.Put(parameter.FilterOperator, list);
                }

                var hasDuplicate = false;
                foreach (var existing in list) {
                    if (existing.Lookupable.Equals(parameter.Lookupable)) {
                        hasDuplicate = true;
                        break;
                    }
                }

                if (hasDuplicate) {
                    continue;
                }

                list.Add(parameter);
            }

            foreach (KeyValuePair<FilterOperator, IList<FilterSpecParamForge>> entry in map) {
                result.AddAll(entry.Value);
            }

            return result.ToArray();
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
                .DeclareVar<FilterSpecParam[][]>(
                    "parameters",
                    LocalMethod(
                        FilterSpecParamForge.MakeParamArrayArrayCodegen(Parameters, classScope, method),
                        Ref("eventType"),
                        symbols.GetAddInitSvc(method)))
                .DeclareVar<FilterSpecActivatable>(
                    "activatable",
                    NewInstance<FilterSpecActivatable>(
                        SAIFFInitializeSymbolWEventType.REF_EVENTTYPE,
                        Constant(FilterForEventType.Name),
                        Ref("parameters"),
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
            ISet<FilterSpecParamExprNodeForge> boolExprs = new LinkedHashSet<FilterSpecParamExprNodeForge>();
            foreach (var spec in filterSpecCompileds) {
                spec.TraverseFilterBooleanExpr(v => boolExprs.Add(v));
            }

            boolExprs.AddAll(additionalBooleanExpressions);
            return new List<FilterSpecParamExprNodeForge>(boolExprs);
        }

        public void TraverseFilterBooleanExpr(Consumer<FilterSpecParamExprNodeForge> consumer)
        {
            foreach (var @params in Parameters) {
                foreach (var param in @params) {
                    if (param is FilterSpecParamExprNodeForge) {
                        consumer.Invoke((FilterSpecParamExprNodeForge) param);
                    }
                }
            }
        }
    }
} // end of namespace