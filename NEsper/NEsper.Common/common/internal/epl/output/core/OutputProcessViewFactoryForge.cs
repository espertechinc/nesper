///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public interface OutputProcessViewFactoryForge
    {
        bool IsCodeGenerated { get; }

        void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        void UpdateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void ProcessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void IteratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders);
    }
} // end of namespace