///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
    [Serializable]
    public class SupportBeanAbstractSub
        : SupportBeanAbstractBase
    {
        public string V2 { get; set; }

        public SupportBeanAbstractSub(String v2)
        {
            V2 = v2;
        }
    }
}
