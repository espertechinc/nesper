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
    public class SupportSelectorCategory : ContextPartitionSelectorCategory
    {
        private readonly ISet<string> labels;

        public SupportSelectorCategory(ISet<string> labels)
        {
            this.labels = labels;
        }

        public SupportSelectorCategory(string label)
        {
            if (label == null) {
                labels = new EmptySet<string>();
            }
            else {
                labels = Collections.SingletonSet(label);
            }
        }

        public ICollection<string> Labels => labels;
    }
} // end of namespace