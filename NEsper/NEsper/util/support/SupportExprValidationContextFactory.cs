///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.util.support
{
    public class SupportExprValidationContextFactory
    {
        public static ExprValidationContext MakeEmpty(IContainer container)
        {
            return MakeEmpty(
                container, ConfigurationEngineDefaults.ThreadingProfile.NORMAL);
        }

        public static ExprValidationContext MakeEmpty(
            IContainer container,
            ConfigurationEngineDefaults.ThreadingProfile threadingProfile)
        {
            return new ExprValidationContext(
                container,
                null,
                new EngineImportServiceImpl(
                    false, false, false, false, null,
                    TimeZoneInfo.Local,
                    TimeAbacusMilliseconds.INSTANCE,
                    threadingProfile, null,
                    AggregationFactoryFactoryDefault.INSTANCE,
                    false, "default", null,
                    container.Resolve<ClassLoaderProvider>()),
                null, null, null, null, null,
                new SupportExprEvaluatorContext(container, null), null, null, 1, null, null, null,
                false, false, false, false, null, false);
        }

        public static ExprValidationContext Make(
            IContainer container, 
            StreamTypeService streamTypeService)
        {
            return new ExprValidationContext(
                container,
                streamTypeService,
                null, null, null, null, null, null, 
                new SupportExprEvaluatorContext(container, null), null, null,
                -1, null, null, null,
                false, false, false, false, null, false);
        }
    }
} // end of namespace