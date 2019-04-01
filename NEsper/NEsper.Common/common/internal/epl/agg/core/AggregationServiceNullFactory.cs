///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.settings;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     A null object implementation of the AggregationService
    ///     interface.
    /// </summary>
    public class AggregationServiceNullFactory : AggregationServiceFactory,
        AggregationServiceFactoryForgeWMethodGen
    {
        public static readonly AggregationServiceNullFactory INSTANCE = new AggregationServiceNullFactory();

        private AggregationServiceNullFactory()
        {
        }

        public AggregationService MakeService(
            AgentInstanceContext agentInstanceContext, ImportServiceRuntime importService,
            bool isSubquery, int? subqueryNumber, int[] groupId)
        {
            return AggregationServiceNull.INSTANCE;
        }

        public void ProviderCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstance(classNames.ServiceFactory, Ref("this")));
        }

        public AggregationCodegenRowLevelDesc RowLevelDesc => AggregationCodegenRowLevelDesc.EMPTY;

        public void MakeServiceCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
            method.Block.MethodReturn(PublicConstValue(typeof(AggregationServiceNull), "INSTANCE"));
        }

        public void CtorCodegen(
            CodegenCtor ctor, IList<CodegenTypedParam> explicitMembers, CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
        }

        public void GetValueCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void GetEventBeanCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void GetCollectionOfEventsCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void ApplyEnterCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
        }

        public void StopMethodCodegen(AggregationServiceFactoryForgeWMethodGen forge, CodegenMethod method)
        {
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
        }

        public void ClearResultsCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void AcceptCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
        }

        public void GetGroupKeysCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void GetGroupKeyCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void AcceptGroupDetailCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
        }

        public void IsGroupedCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantFalse());
        }

        public void RowWriteMethodCodegen(CodegenMethod method, int level)
        {
        }

        public void RowReadMethodCodegen(CodegenMethod method, int level)
        {
        }
    }
} // end of namespace