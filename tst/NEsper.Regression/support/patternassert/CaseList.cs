///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class CaseList
    {
        private readonly List<EventExpressionCase> results;

        public CaseList()
        {
            results = new List<EventExpressionCase>();
        }

        public int NumTests => results.Count;

        public IList<EventExpressionCase> Results => results;

        public void AddTest(EventExpressionCase desc)
        {
            results.Add(desc);
        }
    }
} // end of namespace