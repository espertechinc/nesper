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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    /// <summary>
    ///     Result set processor prototype for the case: aggregation functions used in the select clause, and no group-by,
    ///     and not all of the properties in the select clause are under an aggregation function.
    /// </summary>
    public class ResultSetProcessorRowPerEventForge : ResultSetProcessorFactoryForge
    {
        private readonly ResultSetProcessorOutputConditionType? outputConditionType;
        private readonly OutputLimitSpec outputLimitSpec;
        private readonly SelectExprProcessorForge selectExprProcessorForge;

        public ResultSetProcessorRowPerEventForge(
            EventType resultEventType,
            SelectExprProcessorForge selectExprProcessorForge,
            ExprForge optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            bool isHistoricalOnly,
            OutputLimitSpec outputLimitSpec,
            ResultSetProcessorOutputConditionType? outputConditionType,
            bool hasOrderBy)
        {
            ResultEventType = resultEventType;
            this.selectExprProcessorForge = selectExprProcessorForge;
            OptionalHavingNode = optionalHavingNode;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            IsHistoricalOnly = isHistoricalOnly;
            this.outputLimitSpec = outputLimitSpec;
            this.outputConditionType = outputConditionType;
            IsSorting = hasOrderBy;
        }

        public EventType ResultEventType { get; }

        public ExprForge OptionalHavingNode { get; }

        public bool IsSelectRStream { get; }

        public bool IsUnidirectional { get; }

        public bool IsHistoricalOnly { get; }

        public bool IsOutputLast =>
            outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST;

        public bool IsOutputAll => outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL;

        public bool IsSorting { get; }

        public Type InterfaceClass => typeof(ResultSetProcessorRowPerEvent);

        public string InstrumentedQName => "ResultSetProcessUngroupedNonfullyAgg";

        public void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            instance.Properties.AddProperty(
                typeof(SelectExprProcessor),
                "SelectExprProcessor",
                typeof(ResultSetProcessorRowPerEvent),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(REF_SELECTEXPRPROCESSOR));
            instance.Properties.AddProperty(
                typeof(bool),
                "HasHavingClause",
                typeof(ResultSetProcessorRowPerEvent),
                classScope,
                propertyNode => propertyNode.GetterBlock.BlockReturn(Constant(OptionalHavingNode != null)));
            ResultSetProcessorUtil.EvaluateHavingClauseCodegen(OptionalHavingNode, classScope, instance);
        }

        public void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessViewResultCodegen(this, classScope, method, instance);
        }

        public void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessJoinResultCodegen(this, classScope, method, instance);
        }

        public void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.GetEnumeratorViewCodegen(this, classScope, method);
        }

        public void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.GetEnumeratorJoinCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedViewCodegen(this, classScope, method, instance);
        }

        public void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedJoinCodegen(this, classScope, method, instance);
        }

        public void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ApplyViewResultCodegen(method);
        }

        public void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ApplyJoinResultCodegen(method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ContinueOutputLimitedLastAllNonBufferedViewCodegen(this, method);
        }

        public void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ContinueOutputLimitedLastAllNonBufferedJoinCodegen(this, method);
        }

        public void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedLastAllNonBufferedViewCodegen(
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
            ResultSetProcessorRowPerEventImpl.ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
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
            ResultSetProcessorRowPerEventImpl.AcceptHelperVisitorCodegen(method, instance);
        }

        public void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerEventImpl.StopCodegen(method, instance);
        }

        public void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            ResultSetProcessorRowPerEventImpl.ClearMethodCodegen(method);
        }
    }
} // end of namespace