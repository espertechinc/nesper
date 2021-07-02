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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    /// <summary>
    ///     Result set processor prototype for the simplest case: no aggregation functions used in the select clause, and no
    ///     group-by.
    /// </summary>
    public class ResultSetProcessorSimpleForge : ResultSetProcessorFactoryForge
    {
        private readonly OutputLimitSpec _outputLimitSpec;

        public ResultSetProcessorSimpleForge(
            EventType resultEventType,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            OutputLimitSpec outputLimitSpec,
            ResultSetProcessorOutputConditionType? outputConditionType,
            bool isSorting,
            EventType[] eventTypes,
            Supplier<StateMgmtSetting> outputAllHelperSettings)
        {
            ResultEventType = resultEventType;
            OptionalHavingNode = optionalHavingNode;
            IsSelectRStream = isSelectRStream;
            _outputLimitSpec = outputLimitSpec;
            OutputConditionType = outputConditionType;
            IsSorting = isSorting;
            EventTypes = eventTypes;
            this.OutputAllHelperSettings = outputAllHelperSettings;
        }

        public EventType ResultEventType { get; }

        public bool IsSelectRStream { get; }

        public ExprForge OptionalHavingNode { get; }

        public bool IsOutputLast =>
            _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public ResultSetProcessorOutputConditionType? OutputConditionType { get; }

        public int NumStreams => EventTypes.Length;

        public EventType[] EventTypes { get; }

        public bool IsSorting { get; }

        public Type InterfaceClass => typeof(ResultSetProcessorSimple);

        public Supplier<StateMgmtSetting> OutputAllHelperSettings { get; }

        public void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(bool),
                "HasHavingClause",
                typeof(ResultSetProcessorSimple),
                classScope,
                node => node.GetterBlock.BlockReturn(Constant(OptionalHavingNode != null)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(OptionalHavingNode, classScope, instance);
            instance.Properties.AddProperty(
                typeof(ExprEvaluatorContext),
                "ExprEvaluatorContext",
                GetType(),
                classScope,
                node => node.GetterBlock.BlockReturn(MEMBER_EXPREVALCONTEXT));
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.GetEnumeratorViewCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedViewCodegen(this, method);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedJoinCodegen(this, method);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            // no action
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            // no action
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorSimpleImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
                this,
                classScope,
                method,
                instance);
        }

        public void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorSimpleImpl.StopMethodCodegen(method, instance);
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            // no clearing aggregations
        }

        public string InstrumentedQName => "ResultSetProcessSimple";
    }
} // end of namespace