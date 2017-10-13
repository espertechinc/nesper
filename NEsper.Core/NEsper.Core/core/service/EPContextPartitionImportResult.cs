///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.service
{
    [Serializable]
    public class EPContextPartitionImportResult 
    {
        private readonly IDictionary<int, int> _existingToImported;
        private readonly IDictionary<int, int> _allocatedToImported;

        public EPContextPartitionImportResult(IDictionary<int, int> existingToImported, IDictionary<int, int> allocatedToImported)
        {
            _existingToImported = existingToImported;
            _allocatedToImported = allocatedToImported;
        }

        public IDictionary<int, int> AllocatedToImported
        {
            get { return _allocatedToImported; }
        }

        public IDictionary<int, int> ExistingToImported
        {
            get { return _existingToImported; }
        }
    }
}
