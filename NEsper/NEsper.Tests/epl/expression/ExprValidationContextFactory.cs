///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.core;

namespace com.espertech.esper.epl.expression
{
    public class ExprValidationContextFactory
    {
        public static ExprValidationContext MakeEmpty()
        {
            return MakeEmpty(ConfigurationEngineDefaults.ThreadingProfile.NORMAL);
        }

        public static ExprValidationContext MakeEmpty(ConfigurationEngineDefaults.ThreadingProfile threadingProfile)
        {
            return new ExprValidationContext(
                null,
                new MethodResolutionServiceImpl(
                    new EngineImportServiceImpl(false, false, false, false, null, TimeZoneInfo.Local, threadingProfile),
                    null),
                null, null, null, null, new SupportExprEvaluatorContext(null), null, null, 1, null, null, null,
                false, false, false, false, null, false);
        }

        public static ExprValidationContext Make(StreamTypeService streamTypeService)
        {
            return new ExprValidationContext(
                streamTypeService, null, null, null, null, null, new SupportExprEvaluatorContext(null), null, null, -1,
                null, null, null, false, false, false, false, null, false);
        }
    }
}
