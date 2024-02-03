///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    /// <summary>
    /// Result set processor prototype for the hand-through case:
    /// no aggregation functions used in the select clause, and no group-by, no having and ordering.
    /// </summary>
    public class ResultSetProcessorHandThroughFactoryForge : ResultSetProcessorFactoryForgeBase
    {
        public ResultSetProcessorHandThroughFactoryForge(
            EventType resultEventType,
            EventType[] typesPerStream,
            bool isSelectRStream) : base(resultEventType, typesPerStream)
        {
            IsSelectRStream = isSelectRStream;
        }

        public bool IsSelectRStream { get; }

        public override Type InterfaceClass => typeof(ResultSetProcessor);

        public override void InstanceCodegen(
            CodegenInstanceAux instance,
            CodegenClassScope classScope,
            CodegenCtor factoryCtor,
            IList<CodegenTypedParam> factoryMembers)
        {
            throw NotImplemented();
        }

        public override void ProcessViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ProcessJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void GetEnumeratorViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void GetEnumeratorJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ProcessOutputLimitedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ProcessOutputLimitedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ApplyViewResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ApplyJoinResultCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ContinueOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ContinueOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ProcessOutputLimitedLastAllNonBufferedViewCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ProcessOutputLimitedLastAllNonBufferedJoinCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void AcceptHelperVisitorCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void StopMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            throw NotImplemented();
        }

        public override void ClearMethodCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            throw NotImplemented();
        }

        public override string InstrumentedQName => "ResultSetProcessSimple";

        private UnsupportedOperationException NotImplemented()
        {
            throw new UnsupportedOperationException("Implemented by " + typeof(ResultSetProcessorHandThroughImpl));
        }
    }
} // end of namespace