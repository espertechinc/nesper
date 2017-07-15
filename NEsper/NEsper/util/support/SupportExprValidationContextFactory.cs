///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.util.support
{
    public class SupportExprValidationContextFactory {
        public static ExprValidationContext MakeEmpty() {
            return MakeEmpty(ConfigurationEngineDefaults.ThreadingProfile.NORMAL);
        }
    
        public static ExprValidationContext MakeEmpty(ConfigurationEngineDefaults.ThreadingProfile threadingProfile) {
            return new ExprValidationContext(null, new EngineImportServiceImpl(false, false, false, false, null, TimeZone.Default, TimeAbacusMilliseconds.INSTANCE, threadingProfile, null, AggregationFactoryFactoryDefault.INSTANCE), null, null, null, null, null, new SupportExprEvaluatorContext(null), null, null, 1, null, null, false, false, false, false, null, false);
        }
    
        public static ExprValidationContext Make(StreamTypeService streamTypeService) {
            return new ExprValidationContext(streamTypeService, null, null, null, null, null, null, new SupportExprEvaluatorContext(null), null, null, -1, null, null, false, false, false, false, null, false);
        }
    }
} // end of namespace
