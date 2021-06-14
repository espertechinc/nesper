namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportBeanDuckTypeTwo : SupportBeanDuckType
    {
        private readonly int _intValue;

        public SupportBeanDuckTypeTwo(int intValue)
        {
            _intValue = intValue;
        }

        public double ReturnDouble()
        {
            return 11.1234d;
        }

        public int MakeInteger()
        {
            return _intValue;
        }

        public object MakeCommon()
        {
            return new SupportBeanDuckTypeOne("mytext");
        }
    }
} // end of namespace