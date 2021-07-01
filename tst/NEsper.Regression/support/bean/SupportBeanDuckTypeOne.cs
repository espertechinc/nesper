///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanDuckTypeOne : SupportBeanDuckType
    {
        private readonly string _stringValue;

        public SupportBeanDuckTypeOne(string stringValue)
        {
            _stringValue = stringValue;
        }

        public double ReturnDouble()
        {
            return 12.9876d;
        }

        public string MakeString()
        {
            return _stringValue;
        }

        public object MakeCommon()
        {
            return new SupportBeanDuckTypeTwo(-1);
        }
    }
} // end of namespace