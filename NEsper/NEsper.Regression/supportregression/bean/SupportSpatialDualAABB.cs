///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportSpatialDualAABB  {
        private SupportSpatialAABB one;
        private SupportSpatialAABB two;
    
        public SupportSpatialDualAABB(SupportSpatialAABB one, SupportSpatialAABB two) {
            this.one = one;
            this.two = two;
        }
    
        public SupportSpatialAABB GetOne() {
            return one;
        }
    
        public SupportSpatialAABB GetTwo() {
            return two;
        }
    }
} // end of namespace
