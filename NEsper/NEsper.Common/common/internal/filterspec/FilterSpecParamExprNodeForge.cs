///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
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
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
	/// <summary>
	/// This class represents an arbitrary expression node returning a boolean value as a filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
	/// </summary>
	public sealed class FilterSpecParamExprNodeForge : FilterSpecParamForge {
	    private readonly ExprNode _exprNode;
	    private readonly LinkedHashMap<string, Pair<EventType, string>> _taggedEventTypes;
	    private readonly LinkedHashMap<string, Pair<EventType, string>> _arrayEventTypes;
	    private readonly StreamTypeService _streamTypeService;
	    private readonly bool _hasVariable;
	    private readonly bool _hasFilterStreamSubquery;
	    private readonly bool _hasTableAccess;
	    private readonly StatementCompileTimeServices _compileTimeServices;

	    private int _filterBoolExprId = -1;

	    public FilterSpecParamExprNodeForge(ExprFilterSpecLookupableForge lookupable,
	                                        FilterOperator filterOperator,
	                                        ExprNode exprNode,
	                                        LinkedHashMap<string, Pair<EventType, string>> taggedEventTypes,
	                                        LinkedHashMap<string, Pair<EventType, string>> arrayEventTypes,
	                                        StreamTypeService streamTypeService,
	                                        bool hasSubquery,
	                                        bool hasTableAccess,
	                                        bool hasVariable,
	                                        StatementCompileTimeServices compileTimeServices)
	           
	           	 : base(lookupable, filterOperator)
	           {
	        if (filterOperator != FilterOperator.BOOLEAN_EXPRESSION) {
	            throw new ArgumentException("Invalid filter operator for filter expression node");
	        }
	        this._exprNode = exprNode;
	        this._taggedEventTypes = taggedEventTypes;
	        this._arrayEventTypes = arrayEventTypes;
	        this._streamTypeService = streamTypeService;
	        this._hasFilterStreamSubquery = hasSubquery;
	        this._hasTableAccess = hasTableAccess;
	        this._hasVariable = hasVariable;
	        this._compileTimeServices = compileTimeServices;
	    }

	    /// <summary>
	    /// Returns the expression node of the boolean expression this filter parameter represents.
	    /// </summary>
	    /// <returns>expression node</returns>
	    public ExprNode ExprNode {
	        get => _exprNode;	    }

	    /// <summary>
	    /// Returns the map of tag/stream names to event types that the filter expressions map use (for patterns)
	    /// </summary>
	    /// <returns>map</returns>
	    public LinkedHashMap<string, Pair<EventType, string>> GetTaggedEventTypes() {
	        return _taggedEventTypes;
	    }

	    public override string ToString() {
	        return base.ToString() + "  exprNode=" + _exprNode.ToString();
	    }

	    public override bool Equals(object obj) {
	        if (this == obj) {
	            return true;
	        }

	        if (!(obj is FilterSpecParamExprNodeForge)) {
	            return false;
	        }

	        FilterSpecParamExprNodeForge other = (FilterSpecParamExprNodeForge) obj;
	        if (!base.Equals(other)) {
	            return false;
	        }

	        if (_exprNode != other._exprNode) {
	            return false;
	        }

	        return true;
	    }

	    public override int GetHashCode() {
	        int result = base.GetHashCode();
	        result = 31 * result + _exprNode.GetHashCode();
	        return result;
	    }

	    public int FilterBoolExprId {
	        get => _filterBoolExprId;
	    }

	    public void SetFilterBoolExprId(int filterBoolExprId) {
	        this._filterBoolExprId = filterBoolExprId;
	    }

	    public override CodegenMethod MakeCodegen(CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbolWEventType symbols) {
	        if (_filterBoolExprId == -1) {
	            throw new IllegalStateException("Unassigned filter boolean expression path num");
	        }

	        CodegenMethod method = parent.MakeChild(typeof(FilterSpecParamExprNode), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(ExprFilterSpecLookupable), "lookupable", LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
	                .DeclareVar(typeof(FilterOperator), "op", EnumValue(typeof(FilterOperator), filterOperator.Name()));

	        // getFilterValue-FilterSpecParamExprNode code
	        CodegenExpressionNewAnonymousClass param = NewAnonymousClass(method.Block, typeof(FilterSpecParamExprNode), Arrays.AsList(@Ref("lookupable"), @Ref("op")));
	        CodegenMethod getFilterValue = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), classScope).AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
	        param.AddMethod("getFilterValue", getFilterValue);

	        if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) || (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty())) {
	            int size = (_taggedEventTypes != null) ? _taggedEventTypes.Count : 0;
	            size += (_arrayEventTypes != null) ? _arrayEventTypes.Count : 0;
	            getFilterValue.Block.DeclareVar(typeof(EventBean[]), "events", NewArrayByLength(typeof(EventBean), Constant(size + 1)));

	            int count = 1;
	            if (_taggedEventTypes != null) {
	                foreach (string tag in _taggedEventTypes.Keys) {
	                    getFilterValue.Block.AssignArrayElement("events", Constant(count), ExprDotMethod(REF_MATCHEDEVENTMAP, "getMatchingEventByTag", Constant(tag)));
	                    count++;
	                }
	            }

	            if (_arrayEventTypes != null) {
	                foreach (KeyValuePair<string, Pair<EventType, string>> entry in _arrayEventTypes) {
	                    EventType compositeEventType = entry.Value.First;
	                    CodegenExpressionField compositeEventTypeMember = classScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(compositeEventType, EPStatementInitServicesConstants.REF));
	                    CodegenExpressionField factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
	                    CodegenExpression matchingAsMap = ExprDotMethod(REF_MATCHEDEVENTMAP, "getMatchingEventsAsMap");
	                    CodegenExpression mapBean = ExprDotMethod(factory, "adapterForTypedMap", matchingAsMap, compositeEventTypeMember);
	                    getFilterValue.Block.AssignArrayElement("events", Constant(count), mapBean);
	                    count++;
	                }
	            }
	        } else {
	            getFilterValue.Block.DeclareVar(typeof(EventBean[]), "events", ConstantNull());
	        }

	        getFilterValue.Block
	                .MethodReturn(ExprDotMethod(@Ref("filterBooleanExpressionFactory"), "make",
	                        @Ref("this"), // FilterSpecParamExprNode filterSpecParamExprNode
	                        @Ref("events"), // EventBean[] events
	                        REF_EXPREVALCONTEXT, // ExprEvaluatorContext exprEvaluatorContext
	                        ExprDotMethod(REF_EXPREVALCONTEXT, "getAgentInstanceId"), // int agentInstanceId
	                        REF_STMTCTXFILTEREVALENV));

	        // expression evaluator
	        CodegenExpressionNewAnonymousClass evaluator = ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(_exprNode.Forge, method, this.GetType(), classScope);

	        // setter calls
	        method.Block
	                .DeclareVar(typeof(FilterSpecParamExprNode), "node", param)
	                .ExprDotMethod(@Ref("node"), "setExprText", Constant(StringValue.StringDelimitedTo60Char(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_exprNode))))
	                .ExprDotMethod(@Ref("node"), "setExprNode", evaluator)
	                .ExprDotMethod(@Ref("node"), "setHasVariable", Constant(_hasVariable))
	                .ExprDotMethod(@Ref("node"), "setHasFilterStreamSubquery", Constant(_hasFilterStreamSubquery))
	                .ExprDotMethod(@Ref("node"), "setFilterBoolExprId", Constant(_filterBoolExprId))
	                .ExprDotMethod(@Ref("node"), "setHasTableAccess", Constant(_hasTableAccess))
	                .ExprDotMethod(@Ref("node"), "setFilterBooleanExpressionFactory", ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPStatementInitServicesConstants.GETFILTERBOOLEANEXPRESSIONFACTORY))
	                .ExprDotMethod(@Ref("node"), "setUseLargeThreadingProfile", Constant(_compileTimeServices.Configuration.Common.Execution.ThreadingProfile == ThreadingProfile.LARGE));

	        if ((_taggedEventTypes != null && !_taggedEventTypes.IsEmpty()) || (_arrayEventTypes != null && !_arrayEventTypes.IsEmpty())) {
	            int size = (_taggedEventTypes != null) ? _taggedEventTypes.Count : 0;
	            size += (_arrayEventTypes != null) ? _arrayEventTypes.Count : 0;
	            method.Block.DeclareVar(typeof(EventType[]), "providedTypes", NewArrayByLength(typeof(EventType), Constant(size + 1)));
	            for (int i = 1; i < _streamTypeService.StreamNames.Length; i++) {
	                string tag = _streamTypeService.StreamNames[i];
	                EventType eventType = FindMayNull(tag, _taggedEventTypes);
	                if (eventType == null) {
	                    eventType = FindMayNull(tag, _arrayEventTypes);
	                }
	                if (eventType == null) {
	                    throw new IllegalStateException("Failed to find event type for tag '" + tag + "'");
	                }
	                method.Block.AssignArrayElement("providedTypes", Constant(i), EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF));
	                // note: we leave index zero at null as that is the current event itself
	            }
	            method.Block.ExprDotMethod(@Ref("node"), "setEventTypesProvidedBy", @Ref("providedTypes"));
	        }

	        // register boolean expression so it can be found
	        method.Block.Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPStatementInitServicesConstants.GETFILTERSHAREDBOOLEXPRREGISTERY).Add("registerBoolExpr", @Ref("node")));

	        method.Block.MethodReturn(@Ref("node"));
	        return method;
	    }

	    private EventType FindMayNull(string tag, LinkedHashMap<string, Pair<EventType, string>> tags) {
	        if (tags == null || !tags.ContainsKey(tag)) {
	            return null;
	        }
	        return tags.Get(tag).First;
	    }
	}
} // end of namespace