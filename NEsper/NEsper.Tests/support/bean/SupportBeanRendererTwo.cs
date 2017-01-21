///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
    public class SupportBeanRendererTwo
    {
        private String stringVal;
        private SupportEnum enumValue;
    
        public SupportBeanRendererTwo()
        {
        }
    
        public SupportEnum GetEnumValue()
        {
            return enumValue;
        }
    
        public void SetEnumValue(SupportEnum enumValue)
        {
            this.enumValue = enumValue;
        }
    
        public String GetStringVal()
        {
            return stringVal;
        }
    
        public void SetStringVal(String @stringVal)
        {
            this.stringVal = stringVal;
        }
    }
}
