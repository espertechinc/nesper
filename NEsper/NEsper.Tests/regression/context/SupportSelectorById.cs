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

namespace com.espertech.esper.regression.context
{
    public class SupportSelectorById : ContextPartitionSelectorById
    {
        private readonly ICollection<int> _ids;
    
        public SupportSelectorById(ICollection<int> ids)
        {
            _ids = ids;
        }
    
        public SupportSelectorById(int id)
        {
            _ids = Collections.SingletonList(id);
        }

        public ICollection<int> ContextPartitionIds
        {
            get { return _ids; }
        }
    }
}
