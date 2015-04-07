///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableUpdateStrategyReceiverDesc
    {
        public TableUpdateStrategyReceiverDesc(TableUpdateStrategyReceiver receiver, EventBeanUpdateHelper updateHelper, bool onMerge)
        {
            Receiver = receiver;
            UpdateHelper = updateHelper;
            IsOnMerge = onMerge;
        }

        public TableUpdateStrategyReceiver Receiver { get; private set; }

        public EventBeanUpdateHelper UpdateHelper { get; private set; }

        public bool IsOnMerge { get; private set; }
    }
}