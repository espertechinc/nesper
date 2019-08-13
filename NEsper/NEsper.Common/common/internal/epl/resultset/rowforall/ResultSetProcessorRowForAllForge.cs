///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowforall
{
    /// <summary>
    ///     Result set processor prototype for the case: aggregation functions used in the select clause, and no group-by,
    ///     and all properties in the select clause are under an aggregation function.
    /// </summary>
    public class ResultSetProcessorRowForAllForge : ResultSetProcessorFactoryForge
    {
        private readonly ResultSetProcessorOutputConditionType? outputConditionType;
        private readonly SelectExprProcessorForge selectExprProcessorForge;

        public ResultSetProcessorRowForAllForge(
            EventType resultEventType,
            SelectExprProcessorForge selectExprProcessorForge,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            bool isHistoricalOnly,
            OutputLimitSpec outputLimitSpec,
            bool hasOrderBy,
            ResultSetProcessorOutputConditionType? outputConditionType)
        {
            ResultEventType = resultEventType;
            this.selectExprProcessorForge = selectExprProcessorForge;
            OptionalHavingNode = optionalHavingNode;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            IsHistoricalOnly = isHistoricalOnly;
            OutputLimitSpec = outputLimitSpec;
            IsSorting = hasOrderBy;
            this.outputConditionType = outputConditionType;
        }

        public EventType ResultEventType { get; }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public ExprForge OptionalHavingNode { get; }

        public bool IsHistoricalOnly { get; }

        public bool IsSorting { get; }

        public OutputLimitSpec OutputLimitSpec { get; }

        public Type InterfaceClass => typeof(ResultSetProcessorRowForAll);

        public void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(AggregationService),
                "AggregationService",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(REF_AGGREGATIONSVC));
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(REF_AGENTINSTANCECONTEXT));
            instance.Properties.AddProperty(
                typeof(bool),
                "IsSelectRStream",
                typeof(ResultSetProcessorRowForAll),
                classScope,
                node => node.GetterBlock.BlockReturn(Constant(IsSelectRStream)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(OptionalHavingNode, classScope, instance);
            ResultSetProcessorRowForAllImpl.GetSelectListEventsAsArrayCodegen(this, classScope, instance);
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetIteratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.GetIteratorViewCodegen(this, classScope, method, instance);
        }

        public void GetIteratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.GetIteratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ApplyViewResultCodegen(method);
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ApplyJoinResultCodegen(method);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowForAllImpl.StopCodegen(method, instance);
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowForAllImpl.ClearCodegen(method);
        }

        public string InstrumentedQName => "ResultSetProcessUngroupedFullyAgg";
    }
} // end of namespace