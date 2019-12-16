///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Represents a in-subselect evaluation strategy.
    /// </summary>
    public abstract class SubselectForgeNREqualsInBase : SubselectForgeNRBase
    {
        internal readonly Coercer coercer;
        internal readonly bool isNotIn;

        public SubselectForgeNREqualsInBase(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNotIn,
            Coercer coercer)
            : base(
                subselect,
                valueEval,
                selectEval,
                resultWhenNoMatchingEvents)
        {
            this.isNotIn = isNotIn;
            this.coercer = coercer;
        }
    }
} // end of namespace