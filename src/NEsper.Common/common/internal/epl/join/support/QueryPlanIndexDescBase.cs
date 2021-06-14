///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public abstract class QueryPlanIndexDescBase
    {
        private readonly IndexNameAndDescPair[] tables;

        protected QueryPlanIndexDescBase(IndexNameAndDescPair[] tables)
        {
            this.tables = tables;
        }

        public IndexNameAndDescPair[] Tables {
            get => tables;
        }
    }
} // end of namespace