///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanObject
    {
        public SupportBeanObject()
        {
        }

        public SupportBeanObject(Object one)
        {
            One = one;
        }

        public object Five { get; set; }

        public object Four { get; set; }

        public object One { get; set; }

        public object Six { get; set; }

        public object Three { get; set; }

        public object Two { get; set; }
    }
}
