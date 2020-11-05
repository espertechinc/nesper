///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public interface ResultSetProcessorFactoryForge
    {
        Type InterfaceClass { get; }

        string InstrumentedQName { get; }

        void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers);

        void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method);
    }
} // end of namespace