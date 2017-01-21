///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.lookup
{
    public class LookupStrategyDesc
    {
        public LookupStrategyDesc(LookupStrategyType lookupStrategy, String[] expressionsTexts)
        {
            LookupStrategy = lookupStrategy;
            ExpressionsTexts = expressionsTexts;
        }

        public LookupStrategyType LookupStrategy { get; private set; }

        public string[] ExpressionsTexts { get; private set; }

        public override String ToString()
        {
            return "LookupStrategyDesc{" +
                    "lookupStrategy=" + LookupStrategy +
                    ", expressionsTexts=" + (ExpressionsTexts == null ? null : ExpressionsTexts.Render()) +
                    '}';
        }
    }
}