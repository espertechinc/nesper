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
    ///     Strategy for subselects with "=/!=/&gt;&lt; ALL".
    /// </summary>
    public abstract class SubselectForgeNREqualsBase : SubselectForgeNRBase
    {
        internal readonly Coercer coercer;
        internal readonly bool isNot;

        public SubselectForgeNREqualsBase(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            bool isNot,
            Coercer coercer)
            : base(
                subselect,
                valueEval,
                selectEval,
                resultWhenNoMatchingEvents)
        {
            this.isNot = isNot;
            this.coercer = coercer;
        }
    }
} // end of namespace