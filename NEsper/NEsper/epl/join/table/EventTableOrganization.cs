///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.join.table
{
    public class EventTableOrganization
    {
        public EventTableOrganization(
            String indexName,
            bool unique,
            bool coercing,
            int streamNum,
            IList<string> expressions,
            EventTableOrganizationType type)
        {
            IndexName = indexName;
            IsUnique = unique;
            IsCoercing = coercing;
            StreamNum = streamNum;
            Expressions = expressions;
            OrganizationType = type;
        }

        public string IndexName { get; private set; }

        public bool IsUnique { get; private set; }

        public int StreamNum { get; private set; }

        public IList<string> Expressions { get; private set; }

        public EventTableOrganizationType OrganizationType { get; private set; }

        public bool IsCoercing { get; private set; }
   }
}