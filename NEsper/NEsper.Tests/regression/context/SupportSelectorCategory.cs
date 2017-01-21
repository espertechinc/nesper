///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.context
{
    public class SupportSelectorCategory : ContextPartitionSelectorCategory
    {
        public SupportSelectorCategory(ICollection<String> labels)
        {
            Labels = labels;
        }

        public SupportSelectorCategory(String label)
        {
            if (label == null)
            {
                Labels = Collections.GetEmptySet<string>();
            }
            else
            {
                Labels = Collections.SingletonList(label);
            }
        }

        public ICollection<string> Labels { get; private set; }
    }
}
