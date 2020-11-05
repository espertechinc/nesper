///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanDupProperty
    {
        public SupportBeanDupProperty(
            string myProperty,
            string MyProperty,
            string MYPROPERTY,
            string myproperty)
        {
            this.myProperty = myProperty;
            this.MyProperty = MyProperty;
            this.MYPROPERTY = MYPROPERTY;
            this.myproperty = myproperty;
        }

        public string myproperty { get; }

        public string myProperty { get; }

        public string MyProperty { get; }

        public string MYPROPERTY { get; }
    }
} // end of namespace