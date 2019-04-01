///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.context;

namespace com.espertech.esper.core.service
{
    [Serializable]
    public class EPContextPartitionExtract 
    {
        private readonly ContextPartitionCollection _collection;
        private readonly EPContextPartitionImportable _importable;
        private readonly int _numNestingLevels;

        public EPContextPartitionExtract(ContextPartitionCollection collection, EPContextPartitionImportable importable, int numNestingLevels) 
        {
            _collection = collection;
            _importable = importable;
            _numNestingLevels = numNestingLevels;
        }

        public ContextPartitionCollection Collection
        {
            get { return _collection; }
        }

        public EPContextPartitionImportable Importable
        {
            get { return _importable; }
        }

        public int NumNestingLevels
        {
            get { return _numNestingLevels; }
        }
    }
}
