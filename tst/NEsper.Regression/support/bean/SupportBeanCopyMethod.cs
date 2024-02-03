///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanCopyMethod
    {
        public SupportBeanCopyMethod(
            string valOne,
            string valTwo)
        {
            ValOne = valOne;
            ValTwo = valTwo;
        }

        public string ValOne { get; set; }

        public string ValTwo { get; set; }

        public SupportBeanCopyMethod MyCopyMethod()
        {
            return new SupportBeanCopyMethod(ValOne, ValTwo);
        }
    }
} // end of namespace