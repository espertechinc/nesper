///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.lookup
{
    public class EventTableCreateIndexDesc
    {
        public EventTableCreateIndexDesc(IList<IndexedPropDesc> hashProps, IList<IndexedPropDesc> btreeProps, bool unique)
        {
            HashProps = hashProps;
            BtreeProps = btreeProps;
            IsUnique = unique;
        }

        public IList<IndexedPropDesc> HashProps { get; private set; }

        public IList<IndexedPropDesc> BtreeProps { get; private set; }

        public bool IsUnique { get; private set; }

        public static EventTableCreateIndexDesc FromMultiKey(IndexMultiKey multiKey)
        {
            return new EventTableCreateIndexDesc(
                multiKey.HashIndexedProps,
                multiKey.RangeIndexedProps,
                multiKey.IsUnique);
        }
    }
}
