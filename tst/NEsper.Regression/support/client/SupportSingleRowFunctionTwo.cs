///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportSingleRowFunctionTwo
    {
        public static int Test(int i)
        {
            return i * i * i;
        }

        public static void TestSingleRow(
            string a,
            int b)
        {
        }
    }
} // end of namespace