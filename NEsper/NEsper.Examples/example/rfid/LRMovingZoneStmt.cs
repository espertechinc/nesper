///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


package net.esper.example.rfid;

import net.esper.client.EPServiceProvider;
import net.esper.client.UpdateListener;
import net.esper.client.EPStatement;
import net.esper.event.EventBean;

public class LRMovingZoneStmt
{
    public static void createStmt(EPServiceProvider epService, int secTimeout, UpdateListener listener)
    {
        String textOne = "insert into CountZone " +
                "select zone, count(*) as cnt " +
                "from LocationReport.std:unique('assetId') " +
                "where assetId in ('A1', 'A2', 'A3') " +
                "group by zone";
        EPStatement stmtOne = epService.getEPAdministrator().createEQL(textOne);
        stmtOne.addListener(new UpdateListener()
        {
            public void update(EventBean[] newEvents, EventBean[] oldEvents)
            {
                for (int i = 0; i < newEvents.length; i++)
                {
                    System.out.println("Summary: zone " + newEvents[i].get("zone") + " count " + newEvents[i].get("cnt"));
                }
            }
        });

        String textTwo = "select Part.zone from pattern [" +
                "  every Part=CountZone(cnt in [1:2]) ->" +
                "  (timer:interval(" + secTimeout + " sec) and not CountZone(cnt in (0, 3)))]";
        EPStatement stmtTwo = epService.getEPAdministrator().createEQL(textTwo);
        stmtTwo.addListener(listener);        
    }
}
