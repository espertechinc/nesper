///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
	public class DeliveryConvertorMapWStatement : DeliveryConvertor
    {
	    private readonly string[] _columnNames;
	    private readonly EPStatement _statement;

	    public DeliveryConvertorMapWStatement(string[] columnNames, EPStatement statement)
        {
	        _columnNames = columnNames;
	        _statement = statement;
	    }

	    public object[] ConvertRow(object[] columns)
        {
	        var map = new Dictionary<string, object>();
	        for (int i = 0; i < columns.Length; i++)
	        {
	            map.Put(_columnNames[i], columns[i]);
	        }
	        return new object[] {_statement, map};
	    }
	}
} // end of namespace
