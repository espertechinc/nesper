///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBeanDuckTypeTwo : SupportBeanDuckType
    {
        private readonly int intValue;

        public SupportBeanDuckTypeTwo(int intValue)
        {
            this.intValue = intValue;
        }

        public double ReturnDouble()
        {
            return 11.1234d;
        }

        public int MakeInteger()
        {
            return intValue;
        }

        public object MakeCommon()
        {
            return new SupportBeanDuckTypeOne("mytext");
        }
    }
} // end of namespace
