///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Coordinates between view factories and requested resource (by expressions) the availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateUnverified
    {
        private readonly IList<ExprPriorNode> _priorRequests;
        private readonly IList<ExprPreviousNode> _previousRequests;
    
        public ViewResourceDelegateUnverified()
        {
            _priorRequests = new List<ExprPriorNode>();
            _previousRequests = new List<ExprPreviousNode>();
        }

        public IList<ExprPriorNode> PriorRequests
        {
            get { return _priorRequests; }
        }

        public void AddPriorNodeRequest(ExprPriorNode priorNode) {
            _priorRequests.Add(priorNode);
        }
    
        public void AddPreviousRequest(ExprPreviousNode previousNode) {
            _previousRequests.Add(previousNode);
        }

        public IList<ExprPreviousNode> PreviousRequests
        {
            get { return _previousRequests; }
        }
    }
}
