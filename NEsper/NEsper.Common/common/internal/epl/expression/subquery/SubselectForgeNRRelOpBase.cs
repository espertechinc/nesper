///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public abstract class SubselectForgeNRRelOpBase : SubselectForgeNRBase
    {
        internal readonly RelationalOpEnum.Computer computer;

        public SubselectForgeNRRelOpBase(
            ExprSubselectNode subselect,
            ExprForge valueEval,
            ExprForge selectEval,
            bool resultWhenNoMatchingEvents,
            RelationalOpEnum.Computer computer) 
            : base(subselect, valueEval, selectEval, resultWhenNoMatchingEvents)
        {
            this.computer = computer;
        }
    }
} // end of namespace