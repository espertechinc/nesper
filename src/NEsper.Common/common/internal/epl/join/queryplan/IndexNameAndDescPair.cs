///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class IndexNameAndDescPair
    {
        public IndexNameAndDescPair(
            string tableName,
            string indexDesc)
        {
            IndexName = tableName;
            IndexDesc = indexDesc;
        }

        public string IndexName { get; private set; }

        public string IndexDesc { get; private set; }
    }
}