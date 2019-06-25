///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compiler.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class PropertyParserANTLR
    {
        public static bool IsNestedPropertyWithNonSimpleLead(EsperEPL2GrammarParser.EventPropertyContext ctx)
        {
            if (ctx.eventPropertyAtomic().Length == 1) {
                return false;
            }

            var atomic = ctx.eventPropertyAtomic()[0];
            return atomic.lb != null || atomic.lp != null || atomic.q1 != null;
        }
    }
} // end of namespace