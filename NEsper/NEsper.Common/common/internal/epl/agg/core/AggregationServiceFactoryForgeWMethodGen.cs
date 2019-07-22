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

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationServiceFactoryForgeWMethodGen : AggregationServiceFactoryForge
    {
        AggregationCodegenRowLevelDesc RowLevelDesc { get; }

        void ProviderCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames);

        void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc);

        void RowWriteMethodCodegen(
            CodegenMethod method,
            int level);

        void RowReadMethodCodegen(
            CodegenMethod method,
            int level);

        void MakeServiceCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames);

        void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames);

        void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void GetCollectionOfEventsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void GetCollectionScalarCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void GetEventBeanCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods);

        void ApplyEnterCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames);

        void ApplyLeaveCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames);

        void StopMethodCodegen(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenMethod method);

        void SetRemovedCallbackCodegen(CodegenMethod method);

        void SetCurrentAccessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames);

        void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void AcceptCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void GetGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void GetGroupKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void AcceptGroupDetailCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void IsGroupedCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);
    }
} // end of namespace