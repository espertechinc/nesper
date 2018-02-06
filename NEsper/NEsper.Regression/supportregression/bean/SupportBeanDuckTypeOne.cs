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
    public class SupportBeanDuckTypeOne : SupportBeanDuckType
    {
        private readonly String _stringValue;
    
        public SupportBeanDuckTypeOne(String stringValue)
        {
            _stringValue = stringValue;
        }
    
        public String MakeString() {
            return _stringValue;
        }
    
        public Object MakeCommon() {
            return new SupportBeanDuckTypeTwo(-1);
        }
    
        public double ReturnDouble() {
            return 12.9876d;
        }
    }
}
