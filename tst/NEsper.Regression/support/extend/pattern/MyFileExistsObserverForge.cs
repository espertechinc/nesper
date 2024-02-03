///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyFileExistsObserverForge : ObserverForge
    {
        protected MatchedEventConvertorForge convertor;
        protected ExprNode filenameExpression;

        public void SetObserverParameters(
            IList<ExprNode> observerParameters,
            MatchedEventConvertorForge convertor,
            ExprValidationContext validationContext)
        {
            var message = "File exists observer takes a single string filename parameter";
            if (observerParameters.Count != 1) {
                throw new ObserverParameterException(message);
            }

            if (!(observerParameters[0].Forge.EvaluationType == typeof(string))) {
                throw new ObserverParameterException(message);
            }

            filenameExpression = observerParameters[0];
            this.convertor = convertor;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var builder = new SAIFFInitializeBuilder(
                typeof(MyFileExistsObserverFactory),
                GetType(),
                "observerFactory",
                parent,
                symbols,
                classScope);
            return builder.Exprnode("filenameExpression", filenameExpression)
                .Expression("convertor", convertor.MakeAnonymous(builder.Method(), classScope))
                .Build();
        }

        public void CollectSchedule(
            short factoryNodeId,
            Func<short, CallbackAttribution> scheduleAttribution,
            IList<ScheduleHandleTracked> schedules)
        {
        }
    }
} // end of namespace