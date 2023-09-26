///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionNullFactoryForge : OutputConditionFactoryForge
    {
        public static readonly OutputConditionNullFactoryForge INSTANCE = new OutputConditionNullFactoryForge();

        private OutputConditionNullFactoryForge()
        {
        }

        public CodegenExpression Make(
            CodegenMethodScope method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(OutputConditionNullFactory), "INSTANCE");
        }

        public void CollectSchedules(
            CallbackAttributionOutputRate callbackAttribution,
            IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace