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
    public class SupportSelectorCategory : ContextPartitionSelectorCategory
    {
        private readonly ISet<string> _labels;

        public SupportSelectorCategory(ISet<string> labels)
        {
            _labels = labels;
        }

        public SupportSelectorCategory(string label)
        {
            if (label == null)
            {
                _labels = Collections.GetEmptySet<string>();
            }
            else
            {
                _labels = Collections.SingletonSet(label);
            }
        }

        public ICollection<string> Labels => _labels;
    }
} // end of namespace