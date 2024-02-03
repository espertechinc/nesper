///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.join.hint
{
    public class IndexHintSelectorSubquery : IndexHintSelector
    {
        public IndexHintSelectorSubquery(int subqueryNum)
        {
            SubqueryNum = subqueryNum;
        }

        public int SubqueryNum { get; private set; }

        #region IndexHintSelector Members

        public bool MatchesSubquery(int subqueryNumber)
        {
            return SubqueryNum == subqueryNumber;
        }

        #endregion
    }
}