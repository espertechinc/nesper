///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupDuplicateException : Exception
    {
        public GroupByRollupDuplicateException(int[] indexes)
        {
            Indexes = indexes;
        }

        protected GroupByRollupDuplicateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public int[] Indexes { get; private set; }
    }
}