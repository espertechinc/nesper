///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ForClauseSpec : MetaDefItem
    {
        public ForClauseSpec()
        {
            Clauses = new List<ForClauseItemSpec>();
        }

        public IList<ForClauseItemSpec> Clauses { get; set; }
    }
}
