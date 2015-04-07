///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.window;

namespace com.espertech.esper.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event function in an expression node tree.
    /// </summary>
    public class ExprPriorEvalStrategyRelativeAccess : ExprPriorEvalStrategyBase
    {
        [NonSerialized]
        private readonly RelativeAccessByEventNIndex _relativeAccess;

        public ExprPriorEvalStrategyRelativeAccess(RelativeAccessByEventNIndex relativeAccess)
        {
            _relativeAccess = relativeAccess;
        }

        public override EventBean GetSubstituteEvent(EventBean originalEvent, bool isNewData, int constantIndexNumber)
        {
            return _relativeAccess.GetRelativeToEvent(originalEvent, constantIndexNumber);
        }
    }
}
