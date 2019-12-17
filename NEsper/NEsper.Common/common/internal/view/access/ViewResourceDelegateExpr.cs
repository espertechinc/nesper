///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    ///     Coordinates between view factories and requested resource (by expressions) the
    ///     availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateExpr
    {
        public ViewResourceDelegateExpr()
        {
            PriorRequests = new List<ExprPriorNode>();
            PreviousRequests = new List<ExprPreviousNode>();
        }

        public IList<ExprPriorNode> PriorRequests { get; }

        public IList<ExprPreviousNode> PreviousRequests { get; }

        public void AddPriorNodeRequest(ExprPriorNode priorNode)
        {
            PriorRequests.Add(priorNode);
        }

        public void AddPreviousRequest(ExprPreviousNode previousNode)
        {
            PreviousRequests.Add(previousNode);
        }
    }
} // end of namespace