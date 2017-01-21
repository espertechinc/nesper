///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;


namespace com.espertech.esper.core.service
{
    /// <summary>Converts column results into a Map of key-value pairs. </summary>
    public class DeliveryConvertorMap : DeliveryConvertor
    {
        private readonly String[] _columnNames;
    
        /// <summary>Ctor. </summary>
        /// <param name="columnNames">the names for columns</param>
        public DeliveryConvertorMap(String[] columnNames) {
            this._columnNames = columnNames;
        }
    
        public Object[] ConvertRow(Object[] columns) {
            IDictionary<String, Object> map = new Dictionary<String, Object>();
            for (int i = 0; i < columns.Length; i++)
            {
                map.Put(_columnNames[i], columns[i]);
            }
            return new Object[] {map};
        }
    }
}
