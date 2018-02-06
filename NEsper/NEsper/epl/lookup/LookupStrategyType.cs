///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.lookup
{
    public enum LookupStrategyType
    {
        NULLROWS,
        FULLTABLESCAN,
        VDW,
        SINGLEPROP,
        SINGLEPROPUNIQUE,
        SINGLEPROPNONUNIQUE,
        MULTIPROP,
        RANGE,
        COMPOSITE,
        SINGLEEXPR,
        MULTIEXPR,
        INKEYWORDMULTIIDX,
        INKEYWORDSINGLEIDX,
        ADVANCED
    }
}
