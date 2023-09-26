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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    public interface GuardForge
    {
        /// <summary>
        ///     Sets the guard object parameters.
        /// </summary>
        /// <param name="guardParameters">is a list of parameters</param>
        /// <param name="convertor">for converting a</param>
        /// <param name="services">services</param>
        /// <throws>GuardParameterException thrown to indicate a parameter problem</throws>
        void SetGuardParameters(
            IList<ExprNode> guardParameters,
            MatchedEventConvertorForge convertor,
            StatementCompileTimeServices services);

        void CollectSchedule(
            short factoryNodeId,
            Func<short, CallbackAttribution> callbackAttribution,
            IList<ScheduleHandleTracked> schedules);

        CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);
    }
} // end of namespace