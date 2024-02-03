///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        bool IsDirectAndSimple { get; }

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

        void EnumeratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        void CollectSchedules(IList<ScheduleHandleTracked> scheduleHandleCallbackProviders);
    }
} // end of namespace