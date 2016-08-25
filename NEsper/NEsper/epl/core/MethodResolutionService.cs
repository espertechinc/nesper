///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.plugin;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Service for resolving methods and aggregation functions, and for creating managing aggregation instances.
	/// </summary>
	public interface MethodResolutionService
	{
	    /// <summary>
	    /// Returns true to cache UDF results for constant parameter sets.
	    /// </summary>
	    /// <value>cache UDF results config</value>
	    bool IsUdfCache { get; }

	    bool IsDuckType { get; }

	    bool IsSortUsingCollator { get; }

	    /// <summary>
	    /// Resolves a given method name and list of parameter types to an instance or static method exposed by the given class.
	    /// </summary>
	    /// <param name="clazz">is the class to look for a fitting method</param>
	    /// <param name="methodName">is the method name</param>
	    /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
	    /// <returns>method this resolves to</returns>
	    /// <throws>EngineImportException if the method cannot be resolved to a visible static or instance method</throws>
	    MethodInfo ResolveMethod(Type clazz, string methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType) ;

	    /// <summary>
	    /// Resolves matching available constructors to a list of parameter types to an instance or static method exposed by the given class.
	    /// </summary>
	    /// <param name="clazz">is the class to look for a fitting method</param>
	    /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
	    /// <returns>method this resolves to</returns>
	    /// <throws>EngineImportException if the method cannot be resolved to a visible static or instance method</throws>
	    ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes) ;

	    /// <summary>
	    /// Resolves a given class, method and list of parameter types to a static method.
	    /// </summary>
	    /// <param name="className">is the class name to use</param>
	    /// <param name="methodName">is the method name</param>
	    /// <param name="paramTypes">is parameter types match expression sub-nodes</param>
	    /// <param name="allowEventBeanType">allow event bean type footprint</param>
	    /// <returns>method this resolves to</returns>
	    /// <throws>EngineImportException if the method cannot be resolved to a visible static method</throws>
        MethodInfo ResolveMethod(string className, string methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType);

	    /// <summary>
	    /// Resolves a given class and method name to a static method, not allowing overloaded methods
	    /// and expecting the method to be found exactly once with zero or more parameters.
	    /// </summary>
	    /// <param name="className">is the class name to use</param>
	    /// <param name="methodName">is the method name</param>
	    /// <returns>method this resolves to</returns>
	    /// <throws>EngineImportException if the method cannot be resolved to a visible static method, or if the method exists morethen once with different parameters
	    /// </throws>
	    MethodInfo ResolveMethod(string className, string methodName) ;

	    /// <summary>
	    /// Resolves a given class and method name to a non-static method, not allowing overloaded methods
	    /// and expecting the method to be found exactly once with zero or more parameters.
	    /// </summary>
	    /// <param name="clazz">is the clazz to use</param>
	    /// <param name="methodName">is the method name</param>
	    /// <returns>method this resolves to</returns>
	    /// <throws>EngineImportException if the method cannot be resolved to a visible static method, or if the method exists morethen once with different parameters
	    /// </throws>
        MethodInfo ResolveNonStaticMethod(Type clazz, string methodName);

	    /// <summary>
	    /// Resolves a given class name, either fully qualified and simple and imported to a class.
	    /// </summary>
	    /// <param name="className">is the class name to use</param>
	    /// <returns>class this resolves to</returns>
	    /// <throws>EngineImportException if there was an error resolving the class</throws>
	    Type ResolveType(string className, bool forAnnotation) ;

	    /// <summary>
	    /// Returns a plug-in aggregation function factory for a given configured aggregation function name.
	    /// </summary>
	    /// <param name="functionName">is the aggregation function name</param>
	    /// <returns>aggregation-factory</returns>
	    /// <throws>EngineImportUndefinedException is the function name cannot be found</throws>
	    /// <throws>EngineImportException if there was an error resolving class information</throws>
	    AggregationFunctionFactory ResolveAggregationFactory(string functionName) ;

	    /// <summary>
	    /// Used at statement compile-time to try and resolve a given function name into an
	    /// single-row function. Matches function name case-neutral.
	    /// </summary>
	    /// <param name="functionName">is the function name</param>
	    /// <throws>EngineImportUndefinedException if the function is not a configured single-row function</throws>
	    /// <throws>EngineImportException if the function providing class could not be loaded or doesn't match</throws>
	    Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(string functionName) ;

	    /// <summary>
	    /// Makes a new count-aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="isIgnoreNull">is true to ignore nulls, or false to count nulls  @return aggregator</param>
	    AggregationMethod MakeCountAggregator(int agentInstanceId, int groupId, int aggregationId, bool isIgnoreNull, bool hasFilter);

	    /// <summary>
	    /// Makes a new first-value aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="type">of value  @return aggregator</param>
	    AggregationMethod MakeFirstEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter);

        /// <summary>
        /// Makes a new countever-value aggregator.
        /// </summary>
        /// <param name="agentInstanceId">The agent instance identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="aggregationId">The aggregation identifier.</param>
        /// <param name="hasFilter">if set to <c>true</c> [has filter].</param>
        /// <param name="ignoreNulls">if set to <c>true</c> [ignore nulls].</param>
        /// <returns></returns>
        AggregationMethod MakeCountEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter, bool ignoreNulls);

	    /// <summary>
	    /// Makes a new countever-value aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="ignoreNulls"></param>
	    AggregationMethod MakeLastEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter);

	    /// <summary>
	    /// Makes a new sum-aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="type">is the type to be summed up, i.e. float, long etc.  @return aggregator</param>
	    AggregationMethod MakeSumAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter);

	    Type GetSumAggregatorType(Type inputValueType);

	    /// <summary>
	    /// Makes a new distinct-value-aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="aggregationMethod">is the inner aggregation method</param>
	    /// <param name="childType">is the return type of the inner expression to aggregate, if any   @return aggregator</param>
	    AggregationMethod MakeDistinctAggregator(int agentInstanceId, int groupId, int aggregationId, AggregationMethod aggregationMethod, Type childType, bool hasFilter);

	    /// <summary>
	    /// Makes a new avg-aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="type">the expression return type  @return aggregator</param>
	    AggregationMethod MakeAvgAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter);
	    Type GetAvgAggregatorType(Type childType);

	    /// <summary>
	    /// Makes a new avedev-aggregator.
	    /// </summary>
	    /// <returns>aggregator</returns>
	    AggregationMethod MakeAvedevAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter);

	    /// <summary>
	    /// Makes a new median-aggregator.
	    /// </summary>
	    /// <returns>aggregator</returns>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="hasFilter"></param>
	    AggregationMethod MakeMedianAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter, Type childType);

	    /// <summary>
	    /// Makes a new min-max-aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="minMaxType">dedicates whether to do min or max</param>
	    /// <param name="targetType">is the type to max or min</param>
	    /// <param name="isHasDataWindows">true for has data windows    @return aggregator to use</param>
	    AggregationMethod MakeMinMaxAggregator(int agentInstanceId, int groupId, int aggregationId, MinMaxTypeEnum minMaxType, Type targetType, bool isHasDataWindows, bool hasFilter);

	    /// <summary>
	    /// Makes a new stddev-aggregator.
	    /// </summary>
	    /// <returns>aggregator</returns>
	    AggregationMethod MakeStddevAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter);

	    /// <summary>
	    /// Makes a new rate-aggregator.
	    /// </summary>
	    /// <returns>aggregator</returns>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    AggregationMethod MakeRateAggregator(int agentInstanceId, int groupId, int aggregationId);

	    /// <summary>
	    /// Makes a new rate-aggregator.
	    /// </summary>
	    /// <param name="interval">seconds</param>
	    /// <returns>aggregator to use</returns>
	    AggregationMethod MakeRateEverAggregator(int agentInstanceId, int groupId, int aggregationId, long interval);

	    /// <summary>
	    /// Makes a Nth element aggregator.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    /// <param name="returnType">of aggregation</param>
	    /// <param name="size">of elements   @return aggregator</param>
	    AggregationMethod MakeNthAggregator(int agentInstanceId, int groupId, int aggregationId, Type returnType, int size);

	    /// <summary>
	    /// Make leaving agg.
	    /// </summary>
	    /// <returns>agg</returns>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupId"></param>
	    /// <param name="aggregationId"></param>
	    AggregationMethod MakeLeavingAggregator(int agentInstanceId, int groupId, int aggregationId);

	    /// <summary>
	    /// Returns a new set of aggregators given an existing _prototype-set of aggregators for a given context partition and group key.
	    /// </summary>
	    /// <param name="prototypes">is the prototypes</param>
	    /// <param name="agentInstanceId">context partition</param>
	    /// <param name="groupKey">is the key to group-by for</param>
	    /// <param name="groupByRollupLevel"></param>
	    /// <returns>new set of aggregators for this group</returns>
	    AggregationMethod[] NewAggregators(AggregationMethodFactory[] prototypes, int agentInstanceId, object groupKey, object groupKeyBinding, AggregationGroupByRollupLevel groupByRollupLevel);

	    /// <summary>
	    /// Returns a new set of aggregators given an existing _prototype-set of aggregators for a given context partition (no groups).
	    /// </summary>
	    /// <param name="agentInstanceId">context partition</param>
	    /// <returns>new set of aggregators for this group</returns>
	    AggregationMethod[] NewAggregators(AggregationMethodFactory[] aggregators, int agentInstanceId);

	    /// <summary>
	    /// Opportunity to remove aggregations for a group.
	    /// </summary>
	    /// <param name="agentInstanceId"></param>
	    /// <param name="groupKey">that is no longer used</param>
	    /// <param name="level"></param>
	    void RemoveAggregators(int agentInstanceId, object groupKey, object groupKeyBinding, AggregationGroupByRollupLevel level);

	    /// <summary>
	    /// Returns the current row count of an aggregation, for use with resilience.
	    /// </summary>
	    /// <param name="aggregators">aggregators</param>
	    /// <returns>row count</returns>
	    long GetCurrentRowCount(AggregationMethod[] aggregators, AggregationState[] states);

	    void DestroyedAgentInstance(int agentInstanceId);

	    EngineImportService EngineImportService { get; }

	    AggregationState[] NewAccesses(int agentInstanceId, bool isJoin, AggregationStateFactory[] accessAggSpecs, AggregationServicePassThru passThru);

	    AggregationState[] NewAccesses(int agentInstanceId, bool isJoin, AggregationStateFactory[] accessAggSpecs, object groupKey, object groupKeyBinding, AggregationGroupByRollupLevel groupByRollupLevel, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggLinearNonJoin(int agentInstanceId, int groupId, int aggregationId, int streamNum, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggLinearJoin(int agentInstanceId, int groupId, int aggregationId, int streamNum, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggSortedNonJoin(int agentInstanceId, int groupId, int aggregationId, AggregationStateSortedSpec spec, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggSortedJoin(int agentInstanceId, int groupId, int aggregationId, AggregationStateSortedSpec spec, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggMinMaxEver(int agentInstanceId, int groupId, int aggregationId, AggregationStateMinMaxByEverSpec spec, AggregationServicePassThru passThru);

	    AggregationState MakeAccessAggPlugin(int agentInstanceId, int groupId, int aggregationId, bool join, PlugInAggregationMultiFunctionStateFactory providerFactory, object groupKey);

	    AggregationState MakeCountMinSketch(int agentInstanceId, int groupId, int aggregationId, CountMinSketchSpec specification);

	    object GetCriteriaKeyBinding(ExprEvaluator[] evaluators);

	    object GetGroupKeyBinding(ExprNode[] groupKeyExpressions, AggregationGroupByRollupDesc groupByRollupDesc);

	    object GetGroupKeyBinding(AggregationLocalGroupByPlan localGroupByPlan);
	}
} // end of namespace
