///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalForgeFactory : DatetimeMethodProviderForgeFactory
    {
        public IntervalForge GetForge(
            StreamTypeService streamTypeService,
            DatetimeMethodDesc method,
            string methodNameUsed,
            IList<ExprNode> parameters,
            TimeAbacus timeAbacus,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            return new IntervalForgeImpl(
                method,
                methodNameUsed,
                streamTypeService,
                parameters,
                timeAbacus,
                tableCompileTimeResolver);
        }
    }
} // end of namespace