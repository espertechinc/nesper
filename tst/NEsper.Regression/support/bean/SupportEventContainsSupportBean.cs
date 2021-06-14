///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportEventContainsSupportBean
    {
        public SupportEventContainsSupportBean(SupportBean sb)
        {
            Sb = sb;
        }

        [PropertyName("sb")]
        public SupportBean Sb { get; }
    }
} // end of namespace