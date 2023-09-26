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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public abstract class ResultSetProcessorFactoryForgeBase : ResultSetProcessorFactoryForge
    {
        protected readonly EventType resultEventType;
        protected readonly EventType[] typesPerStream;

        public ResultSetProcessorFactoryForgeBase(
            EventType resultEventType,
            EventType[] typesPerStream)
        {
            this.resultEventType = resultEventType;
            this.typesPerStream = typesPerStream;
        }

        public EventType ResultEventType => resultEventType;

        public EventType[] TypesPerStream => typesPerStream;

        public abstract Type InterfaceClass { get; }
        
        public abstract string InstrumentedQName { get; }

        public abstract void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers);

        public abstract void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance);

        public abstract void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method);
    }
} // end of namespace