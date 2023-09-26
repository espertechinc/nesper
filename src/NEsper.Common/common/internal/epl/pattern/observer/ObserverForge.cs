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
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public interface ObserverForge
    {
        /// <summary>
        /// Sets the observer object parameters.
        /// </summary>
        /// <param name="observerParameters">is a list of parameters</param>
        /// <param name="convertor">for converting partial pattern matches to event-per-stream for expressions</param>
        /// <param name="validationContext">context</param>
        /// <throws>ObserverParameterException thrown to indicate a parameter problem</throws>
        void SetObserverParameters(
            IList<ExprNode> observerParameters,
            MatchedEventConvertorForge convertor,
            ExprValidationContext validationContext);

        CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        void CollectSchedule(
            short factoryNodeId,
            Func<short, CallbackAttribution> scheduleAttribution,
            IList<ScheduleHandleTracked> schedules);
    }
} // end of namespace