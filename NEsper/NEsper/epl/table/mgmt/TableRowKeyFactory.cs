///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableRowKeyFactory
    {
        private readonly int[] _keyColIndexes;
    
        public TableRowKeyFactory(int[] keyColIndexes)
        {
            if (keyColIndexes.Length == 0)
            {
                throw new ArgumentException("No key indexed provided");
            }
            _keyColIndexes = keyColIndexes;
        }
    
        public object GetTableRowKey(object[] data)
        {
            if (_keyColIndexes.Length == 1)
            {
                return data[_keyColIndexes[0]];
            }
            var key = new object[_keyColIndexes.Length];
            for (var i = 0; i < _keyColIndexes.Length; i++)
            {
                key[i] = data[_keyColIndexes[i]];
            }
            return new MultiKeyUntyped(key);
        }
    }
}
