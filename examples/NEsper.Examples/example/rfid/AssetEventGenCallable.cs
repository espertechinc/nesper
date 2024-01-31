///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using java.util.concurrent;

using net.esper.client;

using org.apache.commons.logging;

namespace net.esper.example.rfid
{
	public class AssetEventGenCallable implements Callable<Boolean>
	{
	    public static final int NUM_ZONES = 20;

	    private static final Log log = LogFactory.GetLog(typeof(AssetEventGenCallable));
	    private final EPServiceProvider engine;
	    private final String[][] assetIds;
	    private final int[][] zoneIds;
	    private final Integer[] assetGroupsForThread;
	    private final int ratioZoneMove;
	    private final int ratioZoneSplit;

	    private int numEventsSend;
	    private int numZoneMoves;
	    private int numZoneSplits;
	    private int numSameZone;
	    private Set<Integer> splitZoneGroups = new HashSet<Integer>();
	    private Random random = new Random();

	    private boolean shutdown;
	    private boolean isGenerateZoneSplit;

	    public AssetEventGenCallable(EPServiceProvider engine, String[][] assetIds, int[][] zoneIds, Integer[] assetGroupsForThread, int ratioZoneMove, int ratioZoneSplit)
	    {
	        this.engine = engine;
	        this.assetIds = assetIds;
	        this.zoneIds = zoneIds;
	        this.assetGroupsForThread = assetGroupsForThread;
	        this.ratioZoneMove = ratioZoneMove;
	        this.ratioZoneSplit = ratioZoneSplit;
	        isGenerateZoneSplit = true;
	    }

	    public void SetShutdown(boolean shutdown)
	    {
	        this.shutdown = shutdown;
	    }

	    public void SetGenerateZoneSplit(boolean generateZoneSplit)
	    {
	        isGenerateZoneSplit = generateZoneSplit;
	    }

	    public boolean IsGenerateZoneSplit()
	    {
	        return isGenerateZoneSplit;
	    }

	    public int GetNumEventsSend()
	    {
	        return numEventsSend;
	    }

	    public Boolean Call() throws Exception
	    {
	        try
	        {
	            log.Info(".call Thread " + Thread.CurrentThread().GetId() + " starting");
	            While(!shutdown)
	            {
	                boolean isZoneMove = (random.NextInt() % ratioZoneMove) == 1;
	                boolean isZoneSplit = (random.NextInt() % ratioZoneSplit) == 1;
	                if (isZoneMove)
	                {
	                    DoZoneMove();
	                }
	                else if ((isZoneSplit) && (isGenerateZoneSplit))
	                {
	                    DoZoneSplit();
	                }
	                else
	                {
	                    DoSameZone();
	                }
	            }
	            log.Info(".call Thread " + Thread.CurrentThread().GetId() + " done");
	        }
	        catch (Exception ex)
	        {
	            log.Fatal("Error in thread " + Thread.CurrentThread().GetId(), ex);
	            return false;
	        }
	        return true;
	    }

	    private void DoZoneMove()
	    {
	        // Chose among one of the groups for this thread
	        int index = Math.Abs(random.NextInt()) % assetGroupsForThread.length;
	        int groupNum = assetGroupsForThread[index];

	        // If this is a currently-split group, don't reunion
	        if (splitZoneGroups.Contains(groupNum))
	        {
	            return;
	        }

	        // Determine zone to move to
	        int newZone;
	        do
	        {
	            newZone = Math.Abs(random.NextInt()) % NUM_ZONES;
	        }
	        while (zoneIds[groupNum][0] == newZone);

	        // Move all assets for this group to a new, random zone
	        for (int i = 0; i < assetIds[i].length; i++)
	        {
	            zoneIds[groupNum][i] = newZone;
	            LocationReport report = new LocationReport(assetIds[groupNum][i], newZone);
	            engine.GetEPRuntime().SendEvent(report);
	            numEventsSend++;
	        }
	        numZoneMoves++;
	    }

	    private void DoSameZone()
	    {
	        // Chose among one of the groups for this thread
	        int index = Math.Abs(random.NextInt()) % assetGroupsForThread.length;
	        int groupNum = assetGroupsForThread[index];

	        // If this is a currently-split group, don't reunion
	        if (splitZoneGroups.Contains(groupNum))
	        {
	            return;
	        }

	        // Re-send all assets for this group as the same zone
	        for (int i = 0; i < assetIds[i].length; i++)
	        {
	            LocationReport report = new LocationReport(assetIds[groupNum][i], zoneIds[groupNum][i]);
	            engine.GetEPRuntime().SendEvent(report);
	            numEventsSend++;
	        }
	        numSameZone++;
	    }

	    private void DoZoneSplit()
	    {
	        int groupNum;
	        do
	        {
	            int index = Math.Abs(random.NextInt()) % assetGroupsForThread.length;
	            groupNum = assetGroupsForThread[index];
	        }
	        while (splitZoneGroups.Contains(groupNum));
	        splitZoneGroups.Add(groupNum);

	        // Determine zone to move to
	        int oldZone = zoneIds[groupNum][0];
	        int newZone;
	        do
	        {
	            newZone = Math.Abs(random.NextInt()) % NUM_ZONES;
	        }
	        while (zoneIds[groupNum][0] == newZone);

	        log.Info(".doZoneSplit Split group " + groupNum + " to different zones, from zone " + oldZone + " to zone " + newZone);

	        // Move all assets for this group except the last asset to the new zone
	        for (int i = 0; i < assetIds[i].length - 1; i++)
	        {
	            zoneIds[groupNum][i] = newZone;
	            LocationReport report = new LocationReport(assetIds[groupNum][i], newZone);
	            engine.GetEPRuntime().SendEvent(report);
	            numEventsSend++;
	        }
	        numZoneSplits++;
	    }

	    public int GetNumZoneMoves()
	    {
	        return numZoneMoves;
	    }

	    public int GetNumZoneSplits()
	    {
	        return numZoneSplits;
	    }

	    public int GetNumSameZone()
	    {
	        return numSameZone;
	    }

	    public Set<Integer> GetSplitZoneGroups()
	    {
	        return splitZoneGroups;
	    }
	}
} // End of namespace
