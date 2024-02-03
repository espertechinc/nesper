///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportSelectorByHashCode : ContextPartitionSelectorHash
    {
        public SupportSelectorByHashCode(ICollection<int> hashes)
        {
            Hashes = hashes;
        }

        public SupportSelectorByHashCode(int single)
        {
            Hashes = Collections.SingletonSet(single);
        }

        public ICollection<int> Hashes { get; }

        public static SupportSelectorByHashCode FromSetOfAll(int num)
        {
            ISet<int> set = new HashSet<int>();
            for (var i = 0; i < num; i++) {
                set.Add(i);
            }

            return new SupportSelectorByHashCode(set);
        }
    }
} // end of namespace