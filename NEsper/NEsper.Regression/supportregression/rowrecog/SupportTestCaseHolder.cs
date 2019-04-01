///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.rowrecog {
    public class SupportTestCaseHolder {
        public SupportTestCaseHolder(string measures, string pattern) {
            Measures = measures;
            Pattern = pattern;
            TestCases = new List<SupportTestCaseItem>();
        }

        public string Measures { get; }

        public string Pattern { get; }

        public IList<SupportTestCaseItem> TestCases { get; }

        public SupportTestCaseHolder Add(string testdataString, string[] expected) {
            TestCases.Add(new SupportTestCaseItem(testdataString, expected));
            return this;
        }
    }
} // end of namespace