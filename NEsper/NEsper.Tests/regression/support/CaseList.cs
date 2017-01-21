///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.regression.support
{
    public class CaseList
    {
        private readonly List<EventExpressionCase> _results;
    
        public CaseList()
        {
            _results = new List<EventExpressionCase>();
        }
    
        public void AddTest(EventExpressionCase desc)
        {
            _results.Add(desc);
        }

        public int Count
        {
            get { return _results.Count; }
        }

        public IList<EventExpressionCase> Results
        {
            get { return _results; }
        }
    }
}
