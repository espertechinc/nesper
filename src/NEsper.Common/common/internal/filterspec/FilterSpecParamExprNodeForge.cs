///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents an arbitrary expression node returning a boolean value as a filter parameter in
    ///     an <seealso cref = "FilterSpecActivatable"/> filter specification.
    /// </summary>
    public class FilterSpecParamExprNodeForge : FilterSpecParamForge
    {
        private readonly IDictionary<string, Pair<EventType, string>> _arrayEventTypes;
        private readonly StatementCompileTimeServices _compileTimeServices;
        private readonly ExprNode _exprNode;
        private readonly bool _hasFilterStreamSubquery;
        private readonly bool _hasTableAccess;
        private readonly bool _hasVariable;
        private readonly StreamTypeService _streamTypeService;
        private readonly IDictionary<string, Pair<EventType, string>> _taggedEventTypes;

        public FilterSpecParamExprNodeForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            ExprNode exprNode,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            bool hasSubquery,
            bool hasTableAccess,
            bool hasVariable,
            StatementCompileTimeServices compileTimeServices) : base(lookupable, filterOperator)
        {
            if (filterOperator != FilterOperator.BOOLEAN_EXPRESSION) {
                throw new ArgumentException("Invalid filter operator for filter expression node");
            }

            _exprNode = exprNode;
            _taggedEventTypes = taggedEventTypes;
            _arrayEventTypes = arrayEventTypes;
            _streamTypeService = streamTypeService;
            _hasFilterStreamSubquery = hasSubquery;
            _hasTableAccess = hasTableAccess;
            _hasVariable = hasVariable;
            _compileTimeServices = compileTimeServices;
        }

        public int FilterBoolExprId { get; set; } = -1;

        /// <summary>
        ///     Returns the expression node of the boolean expression this filter parameter represents.
        /// </summary>
        /// <value>expression node</value>
        public ExprNode ExprNode => _exprNode;

        public override string ToString()
        {
            return base.ToString() + "  exprNode=" + _exprNode;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is FilterSpecParamExprNodeForge other)) {
                return false;
            }

            if (!base.Equals(other)) {
                return false;
            }

            if (_exprNode != other._exprNode) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + _exprNode.GetHashCode();
            return result;
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            if (FilterBoolExprId == -1) {
                throw new IllegalStateException("Unassigned filter boolean expression path num");
            }

            var method = parent.MakeChild(typeof(FilterSpecParamExprNode), GetType(), classScope);
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("filterOperator", EnumValue(typeof(FilterOperator), filterOperator.GetName()));
            
            // var getFilterValue = CodegenMethod.MakeParentNode(typeof(FilterValueSetParam), GetType(), classScope)
            //     .AddParam(GET_FILTER_VALUE_FP);
            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(GET_FILTER_VALUE_FP);

            // getFilterValue-FilterSpecParamExprNode code
            // CodegenExpressionNewAnonymousClass param = NewAnonymousClass(
            //     method.Block,
            //     typeof(FilterSpecParamExprNode),
            //     Arrays.AsList(Ref("lookupable"), Ref("op")));
            
            if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) ||
                (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty())) {
                var size = _taggedEventTypes?.Count ?? 0;
                size += _arrayEventTypes?.Count ?? 0;
                getFilterValue.Block.DeclareVar<EventBean[]>(
                    "events",
                    NewArrayByLength(typeof(EventBean), Constant(size + 1)));
                var count = 1;
                if (_taggedEventTypes != null) {
                    foreach (var tag in _taggedEventTypes.Keys) {
                        getFilterValue.Block.AssignArrayElement(
                            "events",
                            Constant(count),
                            ExprDotMethod(REF_MATCHEDEVENTMAP, "GetMatchingEventByTag", Constant(tag)));
                        count++;
                    }
                }

                if (_arrayEventTypes != null) {
                    foreach (var entry in _arrayEventTypes) {
                        var compositeEventType = entry.Value.First;
                        var compositeEventTypeMember = classScope.AddDefaultFieldUnshared(
                            true,
                            typeof(EventType),
                            EventTypeUtility.ResolveTypeCodegen(
                                compositeEventType,
                                EPStatementInitServicesConstants.REF));
                        var factory =
                            classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
                        var matchingAsMap = ExprDotName(REF_MATCHEDEVENTMAP, "MatchingEventsAsMap");
                        var mapBean = ExprDotMethod(
                            factory,
                            "AdapterForTypedMap",
                            matchingAsMap,
                            compositeEventTypeMember);
                        getFilterValue.Block.AssignArrayElement("events", Constant(count), mapBean);
                        count++;
                    }
                }
            }
            else {
                getFilterValue.Block.DeclareVar<EventBean[]>("events", ConstantNull());
            }

            getFilterValue.Block
                .DeclareVar<object>(
                    "value",
                    ExprDotMethod(
                        ExprDotName(Ref("node"), "FilterBooleanExpressionFactory"),
                        "Make",
                        Ref("node"), // FilterSpecParamExprNode filterSpecParamExprNode
                        Ref("events"), // EventBean[] events
                        REF_EXPREVALCONTEXT, // ExprEvaluatorContext exprEvaluatorContext
                        ExprDotName(REF_EXPREVALCONTEXT, "AgentInstanceId"), // int agentInstanceId
                        REF_STMTCTXFILTEREVALENV))
                .BlockReturn(FilterValueSetParamImpl.CodegenNew(Ref("value")));

            var proxyFilterSpecParamExprNode = NewInstance<ProxyFilterSpecParamExprNode>(
                Ref("lookupable"),
                Ref("filterOperator"));
            
            // expression evaluator
            var evaluator = ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                _exprNode.Forge,
                method,
                GetType(),
                classScope);
            // setter calls
            method.Block
                .DeclareVar<ProxyFilterSpecParamExprNode>("node", proxyFilterSpecParamExprNode)
                .SetProperty(Ref("node"), "ProcGetFilterValue", getFilterValue)
                .SetProperty(Ref("node"), "ExprText",
                    Constant(
                        StringValue.StringDelimitedTo60Char(
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_exprNode))))
                .SetProperty(Ref("node"), "ExprNode", evaluator)
                .SetProperty(Ref("node"), "HasVariable", Constant(_hasVariable))
                .SetProperty(Ref("node"), "HasFilterStreamSubquery", Constant(_hasFilterStreamSubquery))
                .SetProperty(Ref("node"), "FilterBoolExprId", Constant(FilterBoolExprId))
                .SetProperty(Ref("node"), "HasTableAccess", Constant(_hasTableAccess))
                .SetProperty(
                    Ref("node"),
                    "FilterBooleanExpressionFactory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERBOOLEANEXPRESSIONFACTORY))
                .SetProperty(
                    Ref("node"),
                    "UseLargeThreadingProfile",
                    Constant(
                        _compileTimeServices.Configuration.Common.Execution.ThreadingProfile == ThreadingProfile.LARGE));
            var providedTypes = ProvidedTypesStartingStreamOne();
            if (providedTypes != null) {
                method.Block.DeclareVar<EventType[]>(
                    "providedTypes",
                    NewArrayByLength(typeof(EventType), Constant(providedTypes.Length)));
                for (var i = 1; i < providedTypes.Length; i++) {
                    method.Block.AssignArrayElement(
                        "providedTypes",
                        Constant(i),
                        EventTypeUtility.ResolveTypeCodegen(providedTypes[i], EPStatementInitServicesConstants.REF));
                }

                method.Block.SetProperty(Ref("node"), "EventTypesProvidedBy", Ref("providedTypes"));
            }

            // register boolean expression so it can be found
            method.Block.Expression(
                ExprDotMethodChain(symbols.GetAddInitSvc(method))
                    .Get(EPStatementInitServicesConstants.FILTERSHAREDBOOLEXPRREGISTERY)
                    .Add("RegisterBoolExpr", Ref("node")));
            
            method.Block.MethodReturn(Ref("node"));
            return LocalMethod(method);
        }

        public EventType[] ProvidedTypesStartingStreamOne()
        {
            if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) ||
                (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty())) {
                var size = _taggedEventTypes?.Count ?? 0;
                size += _arrayEventTypes?.Count ?? 0;
                var providedTypes = new EventType[size + 1];
                for (var i = 1; i < _streamTypeService.StreamNames.Length; i++) {
                    var tag = _streamTypeService.StreamNames[i];
                    var eventType = FindMayNull(tag, _taggedEventTypes);
                    if (eventType == null) {
                        eventType = FindMayNull(tag, _arrayEventTypes);
                    }

                    if (eventType == null) {
                        throw new IllegalStateException("Failed to find event type for tag '" + tag + "'");
                    }

                    providedTypes[i] = eventType;
                    // note: we leave index zero at null as that is the current event itself
                }

                return providedTypes;
            }

            return null;
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("expression '")
                .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_exprNode))
                .Append("'");
        }

        public static string ValueExprToString(string expression)
        {
            return "expression '" + expression + "'";
        }

        private EventType FindMayNull(
            string tag,
            IDictionary<string, Pair<EventType, string>> tags)
        {
            if (tags == null || !tags.ContainsKey(tag)) {
                return null;
            }

            return tags.Get(tag).First;
        }

        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes => _taggedEventTypes;
    }
} // end of namespace