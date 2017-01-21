///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableMetadataColumnPairPlainCol : TableMetadataColumnPairBase
    {
        public TableMetadataColumnPairPlainCol(int dest, int source) : base(dest)
        {
            Source = source;
        }

        public int Source { get; private set; }
    }
}
