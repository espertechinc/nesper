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
using com.espertech.esper.schedule;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Implements method resolution.
    /// </summary>
    public class MethodResolutionServiceImpl : MethodResolutionService
    {
    	private readonly EngineImportService _engineImportService;
        private readonly TimeProvider _timeProvider;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineImportService">is the engine imports</param>
        /// <param name="timeProvider">returns time</param>
        public MethodResolutionServiceImpl(EngineImportService engineImportService,
                                           TimeProvider timeProvider)
    	{
            _engineImportService = engineImportService;
            _timeProvider = timeProvider;
        }

        public bool IsUdfCache
        {
            get { return _engineImportService.IsUdfCache; }
        }

        public bool IsDuckType
        {
            get { return _engineImportService.IsDuckType; }
        }

        public bool IsSortUsingCollator
        {
            get { return _engineImportService.IsSortUsingCollator; }
        }

        public MethodInfo ResolveMethod(String className, String methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType)
    			
        {
            return _engineImportService.ResolveMethod(className, methodName, paramTypes, allowEventBeanType, allowEventBeanCollType);
    	}

        public MethodInfo ResolveMethod(String className, String methodName)
    			
        {
            return _engineImportService.ResolveMethod(className, methodName);
    	}

        public MethodInfo ResolveNonStaticMethod(Type clazz, String methodName)
        {
            return _engineImportService.ResolveNonStaticMethod(clazz, methodName);
        }

        public ConstructorInfo ResolveCtor(Type clazz, Type[] paramTypes)
        {
            return _engineImportService.ResolveCtor(clazz, paramTypes);
        }

        public Type ResolveType(String className)
    			
        {
            return _engineImportService.ResolveType(className);
    	}

        public MethodInfo ResolveMethod(Type clazz, String methodName, Type[] paramTypes, bool[] allowEventBeanType, bool[] allowEventBeanCollType) 
        {
            return _engineImportService.ResolveMethod(clazz, methodName, paramTypes, allowEventBeanType, allowEventBeanCollType);
        }

        public AggregationMethod MakeCountAggregator(
            int agentInstanceId,
            int groupId,
            int aggregationId,
            bool isIgnoreNull,
            bool hasFilter)
        {
            if (!hasFilter)
            {
                if (isIgnoreNull)
                {
                    return new AggregatorCountNonNull();
                }
                return new AggregatorCount();
            }
            else
            {
                if (isIgnoreNull)
                {
                    return new AggregatorCountNonNullFilter();
                }
                return new AggregatorCountFilter();
            }
        }

        public AggregationFunctionFactory ResolveAggregationFactory(String functionName) 
        {
            return _engineImportService.ResolveAggregationFactory(functionName);
        }
    
        public Pair<Type, EngineImportSingleRowDesc> ResolveSingleRow(String functionName) 
        {
            return _engineImportService.ResolveSingleRow(functionName);
        }
    
        public AggregationMethod MakeSumAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter)
        {
            if (!hasFilter)
            {
                if ((type == typeof(decimal?)) || (type == typeof(decimal)))
                {
                    return new AggregatorSumDecimal();
                }
                if ((type == typeof(long?)) || (type == typeof(long)))
                {
                    return new AggregatorSumLong();
                }
                if ((type == typeof(int?)) || (type == typeof(int)))
                {
                    return new AggregatorSumInteger();
                }
                if ((type == typeof(double?)) || (type == typeof(double)))
                {
                    return new AggregatorSumDouble();
                }
                if ((type == typeof(float?)) || (type == typeof(float)))
                {
                    return new AggregatorSumFloat();
                }
                return new AggregatorSumNumInteger();
            }
            else
            {
                if ((type == typeof(decimal?)) || (type == typeof(decimal)))
                {
                    return new AggregatorSumDecimalFilter();
                }
                if ((type == typeof(long?)) || (type == typeof(long)))
                {
                    return new AggregatorSumLongFilter();
                }
                if ((type == typeof(int?)) || (type == typeof(int)))
                {
                    return new AggregatorSumIntegerFilter();
                }
                if ((type == typeof(double?)) || (type == typeof(double)))
                {
                    return new AggregatorSumDoubleFilter();
                }
                if ((type == typeof(float?)) || (type == typeof(float)))
                {
                    return new AggregatorSumFloatFilter();
                }
                return new AggregatorSumNumIntegerFilter();
            }
        }
    
        public Type GetSumAggregatorType(Type type)
        {
            if ((type == typeof(decimal?)) || (type == typeof(decimal)))
            {
                return typeof (decimal?);
            }
            if ((type == typeof(long?)) || (type == typeof(long)))
            {
                return typeof(long?);
            }
            if ((type == typeof(int?)) || (type == typeof(int)))
            {
                return typeof (int?);
            }
            if ((type == typeof(double?)) || (type == typeof(double)))
            {
                return typeof(double?);
            }
            if ((type == typeof(float?)) || (type == typeof(float)))
            {
                return typeof(float?);
            }
            return typeof (int?);
        }
    
        public AggregationMethod MakeDistinctAggregator(int agentInstanceId, int groupId, int aggregationId, AggregationMethod aggregationMethod, Type childType, bool hasFilter)
        {
            if (hasFilter)
            {
                return new AggregatorDistinctValueFilter(aggregationMethod);
            }
            return new AggregatorDistinctValue(aggregationMethod);
        }
    
        public AggregationMethod MakeAvgAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter)
        {
            if (hasFilter)
            {
                if ((type == typeof(decimal?)) || (type == typeof(decimal)))
                {
                    return new AggregatorAvgDecimalFilter(_engineImportService.DefaultMathContext);
                }
                return new AggregatorAvgFilter();
            }
            if ((type == typeof(decimal?)) || (type == typeof(decimal)))
            {
                return new AggregatorAvgDecimal(_engineImportService.DefaultMathContext);
            }
            return new AggregatorAvg();
        }
    
        public Type GetAvgAggregatorType(Type type)
        {
            return (type == typeof (decimal?)) || (type == typeof (decimal))
                ? typeof (decimal?)
                : typeof (double?);
        }

        public AggregationMethod MakeAvedevAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter)
        {
            if (!hasFilter) {
                return new AggregatorAvedev();
            }
            else {
                return new AggregatorAvedevFilter();
            }
        }
    
        public AggregationMethod MakeMedianAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter)
        {
            if (!hasFilter) {
                return new AggregatorMedian();
            }
            return new AggregatorMedianFilter();
        }
    
        public AggregationMethod MakeMinMaxAggregator(int agentInstanceId, int groupId, int aggregationId, MinMaxTypeEnum minMaxTypeEnum, Type targetType, bool isHasDataWindows, bool hasFilter)
        {
            if (!hasFilter) {
                if (!isHasDataWindows) {
                    return new AggregatorMinMaxEver(minMaxTypeEnum, targetType);
                }
                return new AggregatorMinMax(minMaxTypeEnum, targetType);
            }
            else {
                if (!isHasDataWindows) {
                    return new AggregatorMinMaxEverFilter(minMaxTypeEnum, targetType);
                }
                return new AggregatorMinMaxFilter(minMaxTypeEnum, targetType);
            }
        }
    
        public AggregationMethod MakeStddevAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter)
        {
            if (!hasFilter) {
                return new AggregatorStddev();
            }
            return new AggregatorStddevFilter();
        }
    
        public AggregationMethod MakeFirstEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter) {
            if (hasFilter) {
                return new AggregatorFirstEverFilter(type);
            }
            return new AggregatorFirstEver(type);
        }

        public AggregationMethod MakeCountEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, bool hasFilter, bool ignoreNulls)
        {
            if (!hasFilter)
            {
                if (ignoreNulls)
                {
                    return new AggregatorCountEverNonNull();
                }
                return new AggregatorCountEver();
            }
            else
            {
                if (ignoreNulls)
                {
                    return new AggregatorCountEverNonNullFilter();
                }
                return new AggregatorCountEverFilter();
            }
        }

        public AggregationMethod MakeLastEverValueAggregator(int agentInstanceId, int groupId, int aggregationId, Type type, bool hasFilter) {
            if (hasFilter) {
                return new AggregatorLastEverFilter(type);
            }
            return new AggregatorLastEver(type);
        }
    
        public AggregationMethod MakeRateAggregator(int agentInstanceId, int groupId, int aggregationId) {
            return new AggregatorRate();
        }
    
        public AggregationMethod MakeRateEverAggregator(int agentInstanceId, int groupId, int aggregationId, long interval) {
            return new AggregatorRateEver(interval, _timeProvider);
        }
    
        public AggregationMethod MakeNthAggregator(int agentInstanceId, int groupId, int aggregationId, Type returnType, int size) {
            return new AggregatorNth(returnType, size);
        }
    
        public AggregationMethod MakeLeavingAggregator(int agentInstanceId, int groupId, int aggregationId) {
            return new AggregatorLeaving();
        }
    
        public AggregationMethod[] NewAggregators(AggregationMethodFactory[] prototypes, int agentInstanceId) {
            return NewAggregatorsInternal(prototypes, agentInstanceId);
        }
    
        public AggregationMethod[] NewAggregators(AggregationMethodFactory[] prototypes, int agentInstanceId, Object groupKey, Object groupKeyBinding, AggregationGroupByRollupLevel groupByRollupLevel) {
            return NewAggregatorsInternal(prototypes, agentInstanceId);
        }
    
        public AggregationMethod[] NewAggregatorsInternal(AggregationMethodFactory[] prototypes, int agentInstanceId)
        {
            AggregationMethod[] row = new AggregationMethod[prototypes.Length];
            for (int i = 0; i < prototypes.Length; i++)
            {
                row[i] = prototypes[i].Make(this, agentInstanceId, -1, i);
            }
            return row;
        }
    
        public long GetCurrentRowCount(AggregationMethod[] aggregators, AggregationState[] groupStates)
        {
            return 0;   // since the aggregators are always fresh ones 
        }
    
        public void RemoveAggregators(int agentInstanceId, Object groupKey, Object groupKeyBinding, AggregationGroupByRollupLevel level)
        {
            // To be overridden by implementations that care when aggregators get removed
        }
    
        public AggregationState[] NewAccesses(int agentInstanceId, bool isJoin, AggregationStateFactory[] accessAggSpecs) {
            return NewAccessInternal(agentInstanceId, accessAggSpecs, isJoin, null);
        }
    
        public AggregationState[] NewAccesses(int agentInstanceId, bool isJoin, AggregationStateFactory[] accessAggSpecs, Object groupKey, Object groupKeyBinding, AggregationGroupByRollupLevel groupByRollupLevel) {
            return NewAccessInternal(agentInstanceId, accessAggSpecs, isJoin, groupKey);
        }
    
        private AggregationState[] NewAccessInternal(int agentInstanceId, AggregationStateFactory[] accessAggSpecs, bool isJoin, Object groupKey) {
            AggregationState[] row = new AggregationState[accessAggSpecs.Length];
            int i = 0;
            foreach (AggregationStateFactory spec in accessAggSpecs) {
                row[i] = spec.CreateAccess(this, agentInstanceId, 0, i, isJoin, groupKey);   // no group id assigned
                i++;
            }
            return row;
        }
    
        public AggregationState MakeAccessAggLinearNonJoin(int agentInstanceId, int groupId, int aggregationId, int streamNum) {
            return new AggregationStateImpl(streamNum);
        }
    
        public AggregationState MakeAccessAggLinearJoin(int agentInstanceId, int groupId, int aggregationId, int streamNum) {
            return new AggregationStateJoinImpl(streamNum);
        }
    
        public AggregationState MakeAccessAggSortedNonJoin(int agentInstanceId, int groupId, int aggregationId, AggregationStateSortedSpec spec) {
            return new AggregationStateSortedImpl(spec);
        }
    
        public AggregationState MakeAccessAggSortedJoin(int agentInstanceId, int groupId, int aggregationId, AggregationStateSortedSpec spec) {
            return new AggregationStateSortedJoin(spec);
        }
    
        public AggregationState MakeAccessAggMinMaxEver(int agentInstanceId, int groupId, int aggregationId, AggregationStateMinMaxByEverSpec spec) {
            return new AggregationStateMinMaxByEver(spec);
        }
    
        public AggregationState MakeAccessAggPlugin(int agentInstanceId, int groupId, int aggregationId, bool join, PlugInAggregationMultiFunctionStateFactory providerFactory, Object groupKey) {
            PlugInAggregationMultiFunctionStateContext context = new PlugInAggregationMultiFunctionStateContext(agentInstanceId, groupKey);
            return providerFactory.MakeAggregationState(context);
        }

        public AggregationState MakeCountMinSketch(int agentInstanceId, int groupId, int aggregationId, CountMinSketchSpec specification) {
            return new CountMinSketchAggState(CountMinSketchState.MakeState(specification), specification.Agent);
        }

        public void DestroyedAgentInstance(int agentInstanceId) {
            // no action require
        }

        public EngineImportService EngineImportService
        {
            get { return _engineImportService; }
        }

        public Object GetCriteriaKeyBinding(ExprEvaluator[] evaluators) {
            return null;    // no bindings
        }
    
        public Object GetGroupKeyBinding(ExprNode[] groupKeyExpressions, AggregationGroupByRollupDesc groupByRollupDesc) {
            return null;    // no bindings
        }

        public Object GetGroupKeyBinding(AggregationLocalGroupByPlan localGroupByPlan) {
            return null;    // no bindings
        }
    }
}
