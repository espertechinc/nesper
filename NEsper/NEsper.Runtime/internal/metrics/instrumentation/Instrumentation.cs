///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

namespace com.espertech.esper.runtime.@internal.metrics.instrumentation
{
	public interface Instrumentation : InstrumentationCommon
    {
	    void QStimulantEvent(EventBean eventBean, string runtimeURI);

	    void AStimulantEvent();

	    void QStimulantTime(long currentTime, long target, long ultimateTarget, bool span, long? resolution, string runtimeURI);

	    void AStimulantTime();

	    void QEvent(EventBean eventBean, string runtimeURI, bool providedBySendEvent);

	    void AEvent();

	    void QEventCP(EventBean theEvent, EPStatementAgentInstanceHandle handle, long runtimeTime);

	    void AEventCP();

	    void QTime(long runtimeTime, string runtimeURI);

	    void ATime();

	    void QTimeCP(EPStatementAgentInstanceHandle handle, long runtimeTime);

	    void ATimeCP();

	    void QExprEquals(string text);

	    void AExprEquals(Boolean result);

	    void QOutputProcessNonBuffered(EventBean[] newData, EventBean[] oldData);

	    void AOutputProcessNonBuffered();

	    void QOutputProcessNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents);

	    void AOutputProcessNonBufferedJoin();

	    void QSelectClause(EventBean[] eventsPerStream, bool newData, bool synthesize, ExprEvaluatorContext exprEvaluatorContext);

	    void ASelectClause(bool newData, EventBean @event, object[] subscriberParameters);

	    void QExprBitwise(string text, BitWiseOpEnum bitWiseOpEnum);

	    void AExprBitwise(object result);

	    void QExprIdent(string fullUnresolvedName);

	    void AExprIdent(object result);

	    void QExprMath(string text, string op);

	    void AExprMath(object result);

	    void QExprRegexp(string text);

	    void AExprRegexp(Boolean result);

	    void QExprTypeof(string text);

	    void AExprTypeof(string typeName);

	    void QExprOr(string text);

	    void AExprOr(Boolean result);

	    void QExprIn(string text);

	    void AExprIn(Boolean result);

	    void QExprConcat(string text);

	    void AExprConcat(string result);

	    void QExprCoalesce(string text);

	    void AExprCoalesce(object value);

	    void QExprBetween(string text);

	    void AExprBetween(Boolean result);

	    void QExprCast(string text);

	    void AExprCast(object result);

	    void QExprCase(string text);

	    void AExprCase(object result);

	    void QExprArray(string text);

	    void AExprArray(object result);

	    void QExprEqualsAnyOrAll(string text);

	    void AExprEqualsAnyOrAll(Boolean result);

	    void QExprMinMaxRow(string text);

	    void AExprMinMaxRow(object result);

	    void QExprNew(string text);

	    void AExprNew(IDictionary<string, object> props);

	    void QExprNot(string text);

	    void AExprNot(Boolean result);

	    void QExprIStream(string text);

	    void AExprIStream(bool newData);

	    void QExprConst();

	    void AExprConst(object value);

	    void QExprPropExists(string text);

	    void AExprPropExists(bool exists);

	    void QExprRelOpAnyOrAll(string text, string op);

	    void AExprRelOpAnyOrAll(Boolean result);

	    void QExprRelOp(string text, string op);

	    void AExprRelOp(Boolean result);

	    void QExprStreamUndSelectClause(string text);

	    void AExprStreamUndSelectClause(EventBean @event);

	    void QExprIs(string text);

	    void AExprIs(bool result);

	    void QExprVariable(string text);

	    void AExprVariable(object value);

	    void QExprInstanceof(string text);

	    void AExprInstanceof(Boolean result);

	    void QExprTimestamp(string text);

	    void AExprTimestamp(long value);

	    void QExprContextProp(string text);

	    void AExprContextProp(object result);

	    void QExprPlugInSingleRow(string text, string declaringClass, string methodName, string returnTypeName, string[] parameterTypes);

	    void AExprPlugInSingleRow(object result);

	    void QExprDotChain(EPType targetTypeInfo, object target, int numUnpacking);

	    void AExprDotChain();

	    void QExprDot(string text);

	    void AExprDot(object result);

	    void QExprStreamUndMethod(string text);

	    void AExprStreamUndMethod(object result);

	    void QExprDotChainElement(int num, string methodType, string methodName);

	    void AExprDotChainElement(EPType typeInfo, object result);

	    void QExprPrev(string text, bool newData);

	    void AExprPrev(object result);

	    void QExprPrior(string text);

	    void AExprPrior(object result);

	    void QScheduleAdd(long currentTime, long afterMSec, ScheduleHandle handle, long slot);

	    void AScheduleAdd();

	    void QScheduleRemove(ScheduleHandle handle, long slot);

	    void AScheduleRemove();

	    void QFilterRemove(FilterHandle filterCallback, EventType eventType, FilterValueSetParam[][] parameters);

	    void AFilterRemove();

	    void QFilterAdd(EventType eventType, FilterValueSetParam[][] parameters, FilterHandle filterCallback);

	    void AFilterAdd();

	    void QExprAnd(string text);

	    void AExprAnd(Boolean result);

	    void QExprLike(string text);

	    void AExprLike(Boolean result);

	    void QResultSetProcessUngroupedFullyAgg();

	    void AResultSetProcessUngroupedFullyAgg(UniformPair<EventBean[]> pair);

	    void QAggregationUngroupedApplyEnterLeave(bool enter, int numAggregators, int numAccessStates);

	    void AAggregationUngroupedApplyEnterLeave(bool enter);

	    void QExprAggValue(string text);

	    void AExprAggValue(object value);

	    void QResultSetProcessGroupedRowPerGroup();

	    void AResultSetProcessGroupedRowPerGroup(UniformPair<EventBean[]> pair);

	    void QResultSetProcessComputeGroupKeys(bool enter, string[] groupKeyNodeExpressions, EventBean[] eventsPerStream);

	    void AResultSetProcessComputeGroupKeys(bool enter, object groupKeysPerEvent);

	    void QResultSetProcessUngroupedNonfullyAgg();

	    void AResultSetProcessUngroupedNonfullyAgg(UniformPair<EventBean[]> pair);

	    void QResultSetProcessGroupedRowPerEvent();

	    void AResultSetProcessGroupedRowPerEvent(UniformPair<EventBean[]> pair);

	    void QResultSetProcessSimple();

	    void AResultSetProcessSimple(UniformPair<EventBean[]> pair);

	    void QFilter(EventBean theEvent);

	    void AFilter(ICollection<FilterHandle> matches);

	    void QFilterHandleSetIndexes(IList<FilterParamIndexBase> indizes);

	    void AFilterHandleSetIndexes();

	    void QFilterReverseIndex(FilterParamIndexBase filterParamIndex, object propertyValue);

	    void AFilterReverseIndex(bool? match);

	    void QFilterBoolean(FilterParamIndexBooleanExpr filterParamIndexBooleanExpr);

	    void AFilterBoolean();

	    void QFilterBooleanExpr(int num, KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals);

	    void AFilterBooleanExpr(bool result);

	    void QExprDeclared(string text, string name, string expressionText, string[] parameterNames);

	    void AExprDeclared(object value);

	    void QInfraUpdate(EventBean beforeUpdate, EventBean[] eventsPerStream, int length, bool copy);

	    void AInfraUpdate(EventBean afterUpdate);

	    void QInfraUpdateRHSExpr(int index);

	    void AInfraUpdateRHSExpr(object result);

	    void QRouteBetweenStmt(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront);

	    void ARouteBetweenStmt();

	    void QScheduleEval(long currentTime);

	    void AScheduleEval(ICollection<ScheduleHandle> handles);

	    void QStatementResultExecute(UniformPair<EventBean[]> events, string deploymentId, int statementId, string statementName, long threadId);

	    void AStatementResultExecute();

	    void QOrderBy(EventBean[] events, string[] expressions, bool[] descending);

	    void AOrderBy(object values);

	    void QHavingClause(EventBean[] eventsPerStream);

	    void AHavingClause(Boolean pass);

	    void QExprSubselect(string text);

	    void AExprSubselect(object result);

	    void QExprTableSubpropAccessor(string text, string tableName, string subpropName, string aggregationExpression);

	    void AExprTableSubpropAccessor(object result);

	    void QExprTableSubproperty(string text, string tableName, string subpropName);

	    void AExprTableSubproperty(object result);

	    void QExprTableTop(string text, string tableName);

	    void AExprTableTop(object result);

	    void QaEngineManagementStmtStarted(string runtimeURI, string deploymentId, int statementId, string statementName, string epl, long runtimeTime);

	    void QaEngineManagementStmtStop(string runtimeURI, string deploymentId, int statementId, string statementName, string epl, long runtimeTime);

	    void QExprStreamUnd(string text);

	    void AExprStreamUnd(object result);

	    void QaFilterHandleSetCallbacks(ISet<FilterHandle> callbackSet);
	}

} // end of namespace