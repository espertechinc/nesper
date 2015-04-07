///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Coordinates between view factories and requested resource (by expressions) the availability of view resources to expressions.
    /// </summary>
    public class ViewResourceDelegateVerifiedStream
    {
        private readonly IList<ExprPreviousNode> _previousRequests;
        private readonly IDictionary<int, IList<ExprPriorNode>> _priorRequests;
        private readonly ICollection<ExprPreviousMatchRecognizeNode> _matchRecognizePreviousRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewResourceDelegateVerifiedStream"/> class.
        /// </summary>
        /// <param name="previousRequests">The previous requests.</param>
        /// <param name="priorRequests">The prior requests.</param>
        /// <param name="matchRecognizePreviousRequests">The match recognize previous requests.</param>
        public ViewResourceDelegateVerifiedStream(
            IList<ExprPreviousNode> previousRequests,
            IDictionary<int, IList<ExprPriorNode>> priorRequests,
            ISet<ExprPreviousMatchRecognizeNode> matchRecognizePreviousRequests)
        {
            _previousRequests = previousRequests;
            _priorRequests = priorRequests;
            _matchRecognizePreviousRequests = matchRecognizePreviousRequests;
        }

        public IList<ExprPreviousNode> PreviousRequests
        {
            get { return _previousRequests; }
        }

        public IDictionary<int, IList<ExprPriorNode>> PriorRequests
        {
            get { return _priorRequests; }
        }

        public ICollection<ExprPreviousMatchRecognizeNode> MatchRecognizePreviousRequests
        {
            get { return _matchRecognizePreviousRequests; }
        }

        public IList<ExprPriorNode> PriorRequestsAsList
        {
            get
            {
                if (_priorRequests.IsEmpty())
                {
                    return Collections.GetEmptyList<ExprPriorNode>();
                }

                var nodes = new List<ExprPriorNode>();
                foreach (IList<ExprPriorNode> priorNodes in _priorRequests.Values)
                {
                    nodes.AddAll(priorNodes);
                }
                return nodes;
            }
        }
    }
}