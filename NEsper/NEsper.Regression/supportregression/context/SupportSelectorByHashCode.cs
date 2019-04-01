///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.context
{
    public class SupportSelectorByHashCode : ContextPartitionSelectorHash
    {
        private readonly ISet<int> _hashes;

        public SupportSelectorByHashCode(ISet<int> hashes)
        {
            _hashes = hashes;
        }

        public SupportSelectorByHashCode(int single)
        {
            _hashes = Collections.SingletonSet(single);
        }

        public ICollection<int> Hashes => _hashes;
    }
} // end of namespace