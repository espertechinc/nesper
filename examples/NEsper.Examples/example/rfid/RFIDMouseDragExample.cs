///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


package net.esper.example.rfid;

import net.esper.client.Configuration;
import net.esper.client.EPServiceProviderManager;
import net.esper.client.EPServiceProvider;
import net.esper.client.UpdateListener;
import net.esper.client.time.TimerControlEvent;
import net.esper.event.EventBean;

import java.awt.Container;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;

import javax.swing.JFrame;

public class RFIDMouseDragExample extends JFrame
{
    private final static int WIDTH = 750;
    private final static int HEIGHT = 500;

    private DisplayCanvas canvas;

    public RFIDMouseDragExample() {
        super();

        // Setup engine
        Configuration config = new Configuration();
        config.addEventTypeAlias("LocationReport", LocationReport.class);

        EPServiceProvider epService = EPServiceProviderManager.getDefaultProvider(config);
        epService.initialize();

        LRMovingZoneStmt.createStmt(epService, 10, new UpdateListener()
        {
            public void update(EventBean[] newEvents, EventBean[] oldEvents)
            {
                for (int i = 0; i < newEvents.length; i++)
                {
                    System.out.println("New event for zone " + newEvents[i].get("Part.zone"));
                }
            }
        });

        // Setup window
        Container container = getContentPane();
        canvas = new DisplayCanvas(epService, WIDTH, HEIGHT);
        container.add(canvas);
        
        addWindowListener(new WindowAdapter() {
            public void windowClosing(WindowEvent e) {
                System.exit(0);
            }
        });
        setSize(WIDTH, HEIGHT);
        setVisible(true);
    }

    public static void main(String arg[]) {
        new RFIDMouseDragExample();
    }
}
