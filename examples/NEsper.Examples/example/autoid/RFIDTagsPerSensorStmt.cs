///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.runtime.client;

using NEsper.Examples.Support;

namespace NEsper.Examples.AutoId
{
	public class RFIDTagsPerSensorStmt
	{
	    public static EPStatement Create(EPRuntime runtime)
	    {
            var stmt = "select ID as sensorId, coalesce(sum(countTags), 0) as numTagsPerSensor " +
                          "from AutoIdRFIDExample.win:time(60 sec) " +
                          "where Observation[0].Command = 'READ_PALLET_TAGS_ONLY' " +
                          "group by ID";
	
	        return runtime.DeployStatement(stmt);
	    }
	}
}
