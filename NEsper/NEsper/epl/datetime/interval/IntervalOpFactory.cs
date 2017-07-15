///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.interval
{
    public class IntervalOpFactory : OpFactory {
        public IntervalOp GetOp(StreamTypeService streamTypeService, DatetimeMethodEnum method, string methodNameUsed, List<ExprNode> parameters, TimeZone timeZone, TimeAbacus timeAbacus)
                {
    
            return new IntervalOpImpl(method, methodNameUsed, streamTypeService, parameters, timeZone, timeAbacus);
        }
    
    }
} // end of namespace
