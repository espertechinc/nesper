///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regression.rowrecog
{
    public class SupportTestCaseHolder
    {
        public SupportTestCaseHolder(String measures, String pattern)
        {
            Measures = measures;
            Pattern = pattern;
            TestCases = new List<SupportTestCaseItem>();
        }

        public string Measures { get; private set; }

        public string Pattern { get; private set; }

        public List<SupportTestCaseItem> TestCases { get; private set; }

        public SupportTestCaseHolder Add(String testdataString, String[] expected)
        {
            TestCases.Add(new SupportTestCaseItem(testdataString, expected));
            return this;
        }
    }
}
