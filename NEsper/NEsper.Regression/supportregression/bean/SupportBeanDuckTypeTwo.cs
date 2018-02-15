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
    public class SupportBeanDuckTypeTwo : SupportBeanDuckType
    {
        private readonly int _intValue;
    
        public SupportBeanDuckTypeTwo(int intValue)
        {
            _intValue = intValue;
        }
    
        public int MakeInteger() {
            return _intValue;
        }
    
        public Object MakeCommon() {
            return new SupportBeanDuckTypeOne("mytext");
        }
    
        public double ReturnDouble() {
            return 11.1234d;
        }
    }
}
