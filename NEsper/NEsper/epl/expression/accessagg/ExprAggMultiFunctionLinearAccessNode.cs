///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.accessagg
{
    [Serializable]
	public class ExprAggMultiFunctionLinearAccessNode 
        : ExprAggregateNodeBase 
        , ExprEvaluatorEnumeration
        , ExprAggregateAccessMultiValueNode
	{
	    private readonly AggregationStateType _stateType;
	    [NonSerialized] private EventType _containedType;
	    [NonSerialized] private Type _scalarCollectionComponentType;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public ExprAggMultiFunctionLinearAccessNode(AggregationStateType stateType)
	        : base(false)
	    {
	        _stateType = stateType;
	    }

	    public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
	    {
	        return ValidateAggregationInternal(validationContext, null);
	    }

	    public AggregationMethodFactory ValidateAggregationParamsWBinding(ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccessColumn)
        {
	        return ValidateAggregationInternal(validationContext, tableAccessColumn);
	    }

	    private AggregationMethodFactory ValidateAggregationInternal(ExprValidationContext validationContext, TableMetadataColumnAggregation optionalBinding)
        {
	        LinearAggregationFactoryDesc desc;

	        var positionalParams = PositionalParams;

	        // handle table-access expression (state provided, accessor needed)
	        if (optionalBinding != null) {
                desc = HandleTableAccess(positionalParams, _stateType, validationContext, optionalBinding);
	        }
	        // handle create-table statements (state creator and default accessor, limited to certain options)
	        else if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE) {
                desc = HandleCreateTable(positionalParams, _stateType, validationContext);
	        }
	        // handle into-table (state provided, accessor and agent needed, validation done by factory)
	        else if (validationContext.IntoTableName != null) {
                desc = HandleIntoTable(positionalParams, _stateType, validationContext);
	        }
	        // handle standalone
	        else {
                desc = HandleNonIntoTable(positionalParams, _stateType, validationContext);
	        }

	        _containedType = desc.EnumerationEventType;
	        _scalarCollectionComponentType = desc.ScalarCollectionType;

	        return desc.Factory;
	    }

	    private LinearAggregationFactoryDesc HandleNonIntoTable(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext)
        {
	        var streamTypeService = validationContext.StreamTypeService;
	        int streamNum;
	        Type resultType;
	        ExprEvaluator evaluator;
	        ExprNode evaluatorIndex = null;
	        bool istreamOnly;
	        EventType containedType;
	        Type scalarCollectionComponentType = null;

	        // validate wildcard use
	        var isWildcard = childNodes.Length == 0 || childNodes.Length > 0 && childNodes[0] is ExprWildcard;
	        if (isWildcard) {
	            ExprAggMultiFunctionUtil.ValidateWildcardStreamNumbers(validationContext.StreamTypeService, stateType.ToString().ToLower());
	            streamNum = 0;
	            containedType = streamTypeService.EventTypes[0];
	            resultType = containedType.UnderlyingType;
	            TableMetadata tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(containedType);
	            evaluator = ExprNodeUtility.MakeUnderlyingEvaluator(0, resultType, tableMetadata);
	            istreamOnly = GetIstreamOnly(streamTypeService, 0);
	            if ((stateType == AggregationStateType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
	                throw MakeUnboundValidationEx(stateType);
	            }
	        }
	        // validate "stream.*"
	        else if (childNodes.Length > 0 && childNodes[0] is ExprStreamUnderlyingNode) {
	            streamNum = ExprAggMultiFunctionUtil.ValidateStreamWildcardGetStreamNum(childNodes[0]);
	            istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
	            if ((stateType == AggregationStateType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
	                throw MakeUnboundValidationEx(stateType);
	            }
	            var type = streamTypeService.EventTypes[streamNum];
	            containedType = type;
	            resultType = type.UnderlyingType;
	            TableMetadata tableMetadata = validationContext.TableService.GetTableMetadataFromEventType(type);
	            evaluator = ExprNodeUtility.MakeUnderlyingEvaluator(streamNum, resultType, tableMetadata);
	        }
	        // validate when neither wildcard nor "stream.*"
	        else {
	            var child = childNodes[0];
	            var streams = ExprNodeUtility.GetIdentStreamNumbers(child);
	            if ((streams.IsEmpty() || (streams.Count > 1))) {
	                throw new ExprValidationException(GetErrorPrefix(stateType) + " requires that any child expressions evaluate properties of the same stream; Use 'firstever' or 'lastever' or 'nth' instead");
	            }
	            streamNum = streams.First();
	            istreamOnly = GetIstreamOnly(streamTypeService, streamNum);
	            if ((stateType == AggregationStateType.WINDOW) && istreamOnly && !streamTypeService.IsOnDemandStreams) {
	                throw MakeUnboundValidationEx(stateType);
	            }
	            resultType = childNodes[0].ExprEvaluator.ReturnType;
	            evaluator = childNodes[0].ExprEvaluator;
	            if (streamNum >= streamTypeService.EventTypes.Length) {
	                containedType = streamTypeService.EventTypes[0];
	            }
	            else {
	                containedType = streamTypeService.EventTypes[streamNum];
	            }
	            scalarCollectionComponentType = resultType;
	        }

	        if (childNodes.Length > 1) {
	            if (stateType == AggregationStateType.WINDOW) {
	                throw new ExprValidationException(GetErrorPrefix(stateType) + " does not accept an index expression; Use 'first' or 'last' instead");
	            }
	            evaluatorIndex = childNodes[1];
	            if (evaluatorIndex.ExprEvaluator.ReturnType != typeof(int?)) {
	                throw new ExprValidationException(GetErrorPrefix(stateType) + " requires an index expression that returns an integer value");
	            }
	        }

	        // determine accessor
	        AggregationAccessor accessor;
	        if (evaluatorIndex != null) {
	            var isFirst = stateType == AggregationStateType.FIRST;
	            var constant = -1;
	            if (evaluatorIndex.IsConstantResult) {
	                constant = evaluatorIndex.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null)).AsInt();
	            }
	            accessor = new AggregationAccessorFirstLastIndexWEval(streamNum, evaluator, evaluatorIndex.ExprEvaluator, constant, isFirst);
	        }
	        else {
	            if (stateType == AggregationStateType.FIRST) {
	                accessor = new AggregationAccessorFirstWEval(streamNum, evaluator);
	            }
	            else if (stateType == AggregationStateType.LAST) {
	                accessor = new AggregationAccessorLastWEval(streamNum, evaluator);
	            }
	            else if (stateType == AggregationStateType.WINDOW) {
	                accessor = new AggregationAccessorWindowWEval(streamNum, evaluator, resultType);
	            }
	            else {
	                throw new IllegalStateException("Access type is undefined or not known as code '" + stateType + "'");
	            }
	        }

	        var accessorResultType = resultType;
	        if (stateType == AggregationStateType.WINDOW) {
	            accessorResultType = TypeHelper.GetArrayType(resultType);
	        }

	        var isFafWindow = streamTypeService.IsOnDemandStreams && stateType == AggregationStateType.WINDOW;
	        TableMetadata tableMetadataX = validationContext.TableService.GetTableMetadataFromEventType(containedType);
	        if (tableMetadataX == null && !isFafWindow && (istreamOnly || streamTypeService.IsOnDemandStreams)) {
	            var factoryX = new ExprAggMultiFunctionLinearAccessNodeFactoryMethod(this, containedType, accessorResultType, streamNum);
	            return new LinearAggregationFactoryDesc(factoryX, containedType, scalarCollectionComponentType);
	        }

	        var stateKey = new AggregationStateKeyWStream(streamNum, containedType, AggregationStateTypeWStream.DATAWINDOWACCESS_LINEAR, new ExprNode[0]);

            AggregationStateFactory stateFactory = validationContext.EngineImportService.AggregationFactoryFactory.MakeLinear(
                validationContext.StatementExtensionSvcContext, this, streamNum);

	        var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, accessorResultType, containedType,
	                stateKey, stateFactory, AggregationAgentDefault.INSTANCE);
	        var enumerationType = scalarCollectionComponentType == null ? containedType : null;
	        return new LinearAggregationFactoryDesc(factory, enumerationType, scalarCollectionComponentType);
	    }

	    private LinearAggregationFactoryDesc HandleCreateTable(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext)
        {
	        var message = "For tables columns, the " + stateType.GetName().ToLower() + " aggregation function requires the 'window(*)' declaration";
	        if (stateType != AggregationStateType.WINDOW) {
	            throw new ExprValidationException(message);
	        }
	        if (childNodes.Length == 0 || childNodes.Length > 1 || !(childNodes[0] is ExprWildcard)) {
	            throw new ExprValidationException(message);
	        }
	        if (validationContext.StreamTypeService.StreamNames.Length == 0) {
	            throw new ExprValidationException(GetErrorPrefix(stateType) + " requires that the event type is provided");
	        }
	        var containedType = validationContext.StreamTypeService.EventTypes[0];
	        var componentType = containedType.UnderlyingType;
	        AggregationAccessor accessor = new AggregationAccessorWindowNoEval(componentType);
            AggregationStateFactory stateFactory = validationContext.EngineImportService.AggregationFactoryFactory.MakeLinear(validationContext.StatementExtensionSvcContext, this, 0);
            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, TypeHelper.GetArrayType(componentType), containedType, null, stateFactory, null);
	        return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
	    }

	    private LinearAggregationFactoryDesc HandleIntoTable(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext)
        {
	        var message = "For into-table use 'window(*)' or ''window(stream.*)' instead";
	        if (stateType != AggregationStateType.WINDOW) {
	            throw new ExprValidationException(message);
	        }
	        if (childNodes.Length == 0 || childNodes.Length > 1) {
	            throw new ExprValidationException(message);
	        }
	        if (validationContext.StreamTypeService.StreamNames.Length == 0) {
	            throw new ExprValidationException(GetErrorPrefix(stateType) + " requires that at least one stream is provided");
	        }
	        int streamNum;
	        if (childNodes[0] is ExprWildcard) {
	            if (validationContext.StreamTypeService.StreamNames.Length != 1) {
	                throw new ExprValidationException(GetErrorPrefix(stateType) + " with wildcard requires a single stream");
	            }
	            streamNum = 0;
	        }
	        else if (childNodes[0] is ExprStreamUnderlyingNode) {
	            var und = (ExprStreamUnderlyingNode) childNodes[0];
	            streamNum = und.StreamId;
	        }
	        else {
	            throw new ExprValidationException(message);
	        }
	        var containedType = validationContext.StreamTypeService.EventTypes[streamNum];
	        var componentType = containedType.UnderlyingType;
	        AggregationAccessor accessor = new AggregationAccessorWindowNoEval(componentType);
	        AggregationAgent agent;
	        if (streamNum == 0) {
	            agent = AggregationAgentDefault.INSTANCE;
	        }
	        else {
	            agent = new AggregationAgentRewriteStream(streamNum);
	        }
	        var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, TypeHelper.GetArrayType(componentType), containedType, null, null, agent);
	        return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
	    }

	    private LinearAggregationFactoryDesc HandleTableAccess(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccess)
	    {
	        if (stateType == AggregationStateType.FIRST || stateType == AggregationStateType.LAST) {
	            return HandleTableAccessFirstLast(childNodes, stateType, validationContext, tableAccess);
	        }
	        else if (stateType == AggregationStateType.WINDOW) {
	            return HandleTableAccessWindow(childNodes, stateType, validationContext, tableAccess);
	        }
	        throw new IllegalStateException("Unrecognized type " + stateType);
	    }

	    private LinearAggregationFactoryDesc HandleTableAccessFirstLast(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccess)
        {
	        var original = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) tableAccess.Factory;
	        var resultType = original.ContainedEventType.UnderlyingType;
	        AggregationAccessor defaultAccessor = 
                stateType == AggregationStateType.FIRST
                ? (AggregationAccessor) AggregationAccessorFirstNoEval.INSTANCE
                : (AggregationAccessor) AggregationAccessorLastNoEval.INSTANCE;
	        if (childNodes.Length == 0) {
	            var factoryAccess = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, defaultAccessor, resultType, original.ContainedEventType, null, null, null);
	            return new LinearAggregationFactoryDesc(factoryAccess, factoryAccess.ContainedEventType, null);
	        }
	        if (childNodes.Length == 1) {
	            if (childNodes[0] is ExprWildcard) {
	                var factoryAccess = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, defaultAccessor, resultType, original.ContainedEventType, null, null, null);
	                return new LinearAggregationFactoryDesc(factoryAccess, factoryAccess.ContainedEventType, null);
	            }
	            if (childNodes[0] is ExprStreamUnderlyingNode) {
	                throw new ExprValidationException("Stream-wildcard is not allowed for table column access");
	            }
	            // Expressions apply to events held, thereby validate in terms of event value expressions
	            var paramNode = childNodes[0];
	            var streams = TableServiceUtil.StreamTypeFromTableColumn(tableAccess, validationContext.StreamTypeService.EngineURIQualifier);
	            var localValidationContext = new ExprValidationContext(streams, validationContext);
	            paramNode = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode, localValidationContext);
	            var paramNodeEval = paramNode.ExprEvaluator;
	            AggregationAccessor accessor;
	            if (stateType == AggregationStateType.FIRST) {
	                accessor = new AggregationAccessorFirstWEval(0, paramNodeEval);
	            }
	            else {
	                accessor = new AggregationAccessorLastWEval(0, paramNodeEval);
	            }
	            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, paramNodeEval.ReturnType, original.ContainedEventType, null, null, null);
	            return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
	        }
	        if (childNodes.Length == 2) {
	            var isFirst = stateType == AggregationStateType.FIRST;
	            var constant = -1;
	            var indexEvalNode = childNodes[1];
	            if (indexEvalNode.IsConstantResult) {
	                constant = indexEvalNode.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null)).AsInt();
	            }
	            var evaluatorIndex = indexEvalNode.ExprEvaluator;
	            if (evaluatorIndex.ReturnType != typeof(int?)) {
	                throw new ExprValidationException(GetErrorPrefix(stateType) + " requires a constant index expression that returns an integer value");
	            }
	            AggregationAccessor accessor = new AggregationAccessorFirstLastIndexNoEval(evaluatorIndex, constant, isFirst);
	            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, resultType, original.ContainedEventType, null, null, null);
	            return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
	        }
	        throw new ExprValidationException("Invalid number of parameters");
	    }

	    private LinearAggregationFactoryDesc HandleTableAccessWindow(ExprNode[] childNodes, AggregationStateType stateType, ExprValidationContext validationContext, TableMetadataColumnAggregation tableAccess)
	    {
	        var original = (ExprAggMultiFunctionLinearAccessNodeFactoryAccess) tableAccess.Factory;
	        if (childNodes.Length == 0 ||
	           (childNodes.Length == 1 && childNodes[0] is ExprWildcard)) {
	            var componentType = original.ContainedEventType.UnderlyingType;
	            AggregationAccessor accessor = new AggregationAccessorWindowNoEval(componentType);
	            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this, accessor, TypeHelper.GetArrayType(componentType), original.ContainedEventType, null, null, null);
	            return new LinearAggregationFactoryDesc(factory, factory.ContainedEventType, null);
	        }
	        if (childNodes.Length == 1) {
	            // Expressions apply to events held, thereby validate in terms of event value expressions
	            var paramNode = childNodes[0];
	            var streams = TableServiceUtil.StreamTypeFromTableColumn(tableAccess, validationContext.StreamTypeService.EngineURIQualifier);
	            var localValidationContext = new ExprValidationContext(streams, validationContext);
	            paramNode = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, paramNode, localValidationContext);
	            var paramNodeEval = paramNode.ExprEvaluator;
	            var factory = new ExprAggMultiFunctionLinearAccessNodeFactoryAccess(this,
	                    new AggregationAccessorWindowWEval(0, paramNodeEval, paramNodeEval.ReturnType), TypeHelper.GetArrayType(paramNodeEval.ReturnType), original.ContainedEventType, null, null, null);
	            return new LinearAggregationFactoryDesc(factory, null, paramNodeEval.ReturnType);
	        }
	        throw new ExprValidationException("Invalid number of parameters");
	    }

        internal static bool GetIstreamOnly(StreamTypeService streamTypeService, int streamNum)
        {
	        if (streamNum < streamTypeService.EventTypes.Length) {
	            return streamTypeService.IsIStreamOnly[streamNum];
	        }
	        // this could happen for match-recognize which has different stream types for selection and for aggregation
	        return streamTypeService.IsIStreamOnly[0];
	    }

        public override string AggregationFunctionName
        {
            get { return _stateType.ToString().ToLower(); }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write(_stateType.ToString().ToLower());
	        ExprNodeUtility.ToExpressionStringParams(writer, this.ChildNodes);
	    }

        public AggregationStateType StateType
        {
            get { return _stateType; }
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return base.AggregationResultFuture.GetCollectionOfEvents(Column, eventsPerStream, isNewData, context);
	    }

	    public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return base.AggregationResultFuture.GetCollectionScalar(Column, eventsPerStream, isNewData, context);
	    }

	    public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
	        if (_stateType == AggregationStateType.FIRST || _stateType == AggregationStateType.LAST) {
	            return null;
	        }
	        return _containedType;
	    }

        public Type ComponentTypeCollection
        {
            get { return _scalarCollectionComponentType; }
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
	        if (_stateType == AggregationStateType.FIRST || _stateType == AggregationStateType.LAST) {
	            return _containedType;
	        }
	        return null;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return base.AggregationResultFuture.GetEventBean(Column, eventsPerStream, isNewData, context);
	    }

	    protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
	        return false;
	    }

	    private static ExprValidationException MakeUnboundValidationEx(AggregationStateType stateType)
        {
	        return new ExprValidationException(GetErrorPrefix(stateType) + " requires that the aggregated events provide a remove stream; Please define a data window onto the stream or use 'firstever', 'lastever' or 'nth' instead");
	    }

	    private static string GetErrorPrefix(AggregationStateType stateType)
        {
	        return ExprAggMultiFunctionUtil.GetErrorPrefix(stateType.ToString().ToLower());
	    }
	}
} // end of namespace
