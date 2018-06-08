///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.rowrecog {
    public class SupportTestCaseItem {
        public SupportTestCaseItem(string testdata, string[] expected) {
            Testdata = testdata;
            Expected = expected;
        }

        public string Testdata { get; }

        public string[] Expected { get; }
    }
} // end of namespace