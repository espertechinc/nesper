///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportSpatialDualAABB
    {
        public SupportSpatialDualAABB(
            SupportSpatialAABB one,
            SupportSpatialAABB two)
        {
            One = one;
            Two = two;
        }

        public SupportSpatialAABB One { get; }

        public SupportSpatialAABB Two { get; }
    }
} // end of namespace