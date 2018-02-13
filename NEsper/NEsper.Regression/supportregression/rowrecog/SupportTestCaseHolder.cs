///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.rowrecog
{
    public class SupportTestCaseHolder {
    
        private string measures;
        private string pattern;
        private List<SupportTestCaseItem> testcases;
    
        public SupportTestCaseHolder(string measures, string pattern) {
            this.measures = measures;
            this.pattern = pattern;
            this.testcases = new List<SupportTestCaseItem>();
        }
    
        public string GetMeasures() {
            return measures;
        }
    
        public string GetPattern() {
            return pattern;
        }
    
        public List<SupportTestCaseItem> GetTestCases() {
            return testcases;
        }
    
        public SupportTestCaseHolder Add(string testdataString, string[] expected) {
            testcases.Add(new SupportTestCaseItem(testdataString, expected));
            return this;
        }
    }
} // end of namespace
