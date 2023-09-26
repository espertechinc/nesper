///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	/// <summary>
	/// Performance test for the following problem and statements:
	/// &lt;quote&gt;If a given set of assets are not moving together from zone to zone, alert&lt;/quote&gt;
	/// &lt;p&gt;
	/// Statements:
	/// &lt;pre&gt;
	/// insert into CountZone_[Nx] select [Nx] as groupId, zone, Count(*) as cnt
	/// from LocationReport(assetId in ([aNx1], [aNx2], [aNx3])).std:unique('assetId')
	/// group by zone
	/// select * from pattern [every a=CountZone_[Nx](cnt in [1:2]) -&gt;
	/// (timer:interval(10 sec) and not CountZone_[Nx](cnt in (0, 3)))]
	/// &lt;/pre&gt;
	/// This performance test works as follows:
	/// &lt;OL&gt;
	/// &lt;LI&gt; Assume N is the number of asset groups (numAssetGroups), each group consisting of 3 assets
	/// &lt;LI&gt; Generate unique assets ids for N*3 assets, assign a random start zone for each asset group
	/// &lt;LI&gt; Create 2 times N statements: the first N statements count the assets per zones for the statement's asset group,
	/// and generates stream CountZone_[Nx] where Nx=0..N
	/// The second statement detects a pattern among each asset group's event stream CountZone_[Nx] where assets are split
	/// between zones for more then 10 seconds.
	/// &lt;LI&gt; Send one event for each asset to start of each asset in the assigned zone
	/// &lt;LI&gt; Create M number of callables and an executor services, assigning each callable a range of asset groups.
	/// For example, with 1000 asset groups and 3 callables (threads) then each callable gets 333 asset groups assigned to it. The callable only
	/// sends events for the assigned asset group. The main thread starts the executor service.
	/// &lt;LI&gt; Each callable enters a processing loop until a shutdown flag is set
	/// &lt;LI&gt; If a random number integer number modulo the ratio of zone moves is 1, then the callable moves one asset group from zone to zone.
	/// For this, it determines a random asset group and new zone and sends 3 events moving the 3 assets to the new zone.
	/// &lt;LI&gt; If a random number integer number modulo the ratio of zone splits is 1, then the callable moves 2 of the 3 assets in
	/// a random asset group to a new zone, and leaves one asset in the group in the old zone. It saves the asset group number
	/// in a collection since this information is needed to reconciled later.
	/// &lt;LI&gt; If neither random number matches, then the callable picks a random asset group and resends all 3 asset location
	/// report events for the current zone for that asset.
	/// &lt;LI&gt; The main thread runs for the given number of seconds, sleeps 1 seconds and compiles statistics for reporting
	/// by asking each callable for events generated
	/// &lt;LI&gt; At 15 seconds before the end of the test the main thread invokes a setter method on all callables to stop
	/// generating split asset groups.
	/// &lt;LI&gt; The main thread stops the executor service
	/// &lt;LI&gt; The main thread reconciles the events received by listeners with the asset groups that were split by any callables.
	/// &lt;/OL&gt;
	/// </summary>
	public class LRMovingSimMain
	{
	    private static final Log log = LogFactory.GetLog(typeof(LRMovingSimMain));

	    private final int numberOfThreads;
	    private final int numberOfAssetGroups;
	    private final int numberOfSeconds;
	    private boolean isAssert;

	    private EPServiceProvider epService;
	    private Random random = new Random();

	    public static void Main(String[] args) throws Exception
	    {
	        if (args.length < 3) {
	            System.out.Println("Arguments are: <number of threads> <number of asset groups> <number of seconds to run>");
	            System.out.Println("  number of threads: the number of threads sending events into the engine (e.g. 4)");
	            System.out.Println("  number of asset groups: number of groups tracked (e.g. 1000)");
	            System.out.Println("  number of seconds: the number of seconds the simulation runs (e.g. 60)");
	            System.Exit(-1);
	        }

	        int numberOfThreads;
	        try {
	            numberOfThreads = Integer.ParseInt(args[0]);
	        } catch (NullPointerException e) {
	            System.out.Println("Invalid number of threads:" + args[0]);
	            System.Exit(-2);
	            return;
	        }

	        int numberOfAssetGroups;
	        try {
	            numberOfAssetGroups = Integer.ParseInt(args[1]);
	        } catch (NumberFormatException e) {
	            System.out.Println("Invalid number of asset groups:" + args[1]);
	            System.Exit(-2);
	            return;
	        }

	        int numberOfSeconds;
	        try {
	            numberOfSeconds = Integer.ParseInt(args[2]);
	        } catch (NullPointerException e) {
	            System.out.Println("Invalid number of seconds to run:" + args[2]);
	            System.Exit(-2);
	            return;
	        }

	        // Run the sample
	        System.out.Println("Using " + numberOfThreads + " threads and " + numberOfAssetGroups + " asset groups, for " + numberOfSeconds + " seconds");
	        LRMovingSimMain simMain = new LRMovingSimMain(numberOfThreads, numberOfAssetGroups, numberOfSeconds, false);
	        simMain.Run();
	    }

	    public LRMovingSimMain(int numberOfThreads, int numberOfAssetGroups, int numberOfSeconds, boolean isAssert)
	    {
	        this.numberOfThreads = numberOfThreads;
	        this.numberOfAssetGroups = numberOfAssetGroups;
	        this.numberOfSeconds = numberOfSeconds;
	        this.isAssert = isAssert;
	    }

	    public void Run() throws Exception
	    {
	        Configuration config = new Configuration();
	        config.AddEventTypeAlias("LocationReport", typeof(LocationReport));

	        epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();

	        // Number of seconds the total test runs
	        int numSeconds = numberOfSeconds;    // usually 60

	        // Number of asset groups
	        int numAssetGroups = numberOfAssetGroups;      // usually 1000

	        // Number of threads
	        int numThreads = numberOfThreads;

	        // Ratio of events indicating that all assets moved to a new zone
	        int ratioZoneMove= 3;

	        // Ratio of events indicating that the asset group split between zones, i.e. only some assets in a group move to a new zone
	        int ratioZoneSplit = 1000000;       // usually 1000000;

	        TryPerf(numSeconds, numAssetGroups, numThreads, ratioZoneMove, ratioZoneSplit);
	    }

	    private void TryPerf(int numSeconds, int numAssetGroups, int numThreads, int ratioZoneMove, int ratioZoneSplit)
	            throws Exception
	    {
	        // Create Asset Ids and assign to a zone
	        log.Info(".tryPerf Creating asset ids");
	        String[][] assetIds = new String[numAssetGroups][3];
	        int[][] zoneIds = new int[numAssetGroups][3];
	        for (int i = 0; i < numAssetGroups; i++)
	        {
	            // Generate unique asset id over all groups
	            String assetPrefix = String.Format("%010d", i); // 10 digit zero padded, i.e. 00000001.n;
	            assetIds[i][0] = assetPrefix + "0";
	            assetIds[i][1] = assetPrefix + "1";
	            assetIds[i][2] = assetPrefix + "2";

	            int currentZone = Math.Abs(random.NextInt()) % AssetEventGenCallable.NUM_ZONES;
	            zoneIds[i][0] = currentZone;
	            zoneIds[i][1] = currentZone;
	            zoneIds[i][2] = currentZone;
	        }

	        // Create statements
	        log.Info(".tryPerf Creating " + numAssetGroups*2 + " statements for " + numAssetGroups + " asset groups");
	        AssetZoneSplitListener listeners[] = new AssetZoneSplitListener[numAssetGroups];
	        for (int i = 0; i < numAssetGroups; i++)
	        {
	            String streamName = "CountZone_" + i;
	            String assetIdList = "'" + assetIds[i][0] + "','" + assetIds[i][1] + "','" + assetIds[i][2] + "'";

	            String textOne = "insert into " + streamName +
	                    " select " + i + " as groupId, zone, Count(*) as cnt " +
	                    "from LocationReport(assetId in (" + assetIdList + ")).std:unique('assetId') " +
	                    "group by zone";
	            EPStatement stmtOne = epService.GetEPAdministrator().CreateEQL(textOne);
	            // stmtOne.AddListener(new AssetGroupCountListener());  for debugging

	            String textTwo = "select * from pattern [" +
	                    "  every a=" + streamName + "(cnt in [1:2]) ->" +
	                    "  (timer:interval(10 sec) and not " + streamName + "(cnt in (0, 3)))]";
	            EPStatement stmtTwo = epService.GetEPAdministrator().CreateEQL(textTwo);
	            listeners[i] = new AssetZoneSplitListener();
	            stmtTwo.AddListener(listeners[i]);
	        }

	        // First, send an event for each asset with it's current zone
	        log.Info(".tryPerf Sending one event for each asset");
	        for (int i = 0; i < assetIds.length; i++)
	        {
	            for (int j = 0; j < assetIds[i].length; j++)
	            {
	                LocationReport report = new LocationReport(assetIds[i][j], zoneIds[i][j]);
	                epService.GetEPRuntime().SendEvent(report);
	            }
	        }

	        // Reset listeners
	        for (int i = 0; i < listeners.length; i++)
	        {
	            listeners[i].Reset();
	        }

	        // Create threadpool
	        log.Info(".tryPerf Starting " + numThreads + " threads");
	        ExecutorService threadPool = Executors.NewFixedThreadPool(numThreads);
	        Future future[] = new Future[numThreads];
	        AssetEventGenCallable callables[] = new AssetEventGenCallable[numThreads];
	        Integer[][] assetGroupsForThread = GetGroupsPerThread(numAssetGroups, numThreads);

	        for (int i = 0; i < numThreads; i++)
	        {
	            callables[i] = new AssetEventGenCallable(epService, assetIds, zoneIds, assetGroupsForThread[i], ratioZoneMove, ratioZoneSplit);
	            Future<Boolean> f = threadPool.Submit(callables[i]);
	            future[i] = f;
	        }

	        // Create threadpool
	        log.Info(".tryPerf Running for " + numSeconds + " seconds");
	        long startTime = PerformanceObserver.MilliTime;
	        long currTime;
	        double deltaSeconds;
	        int lastTotalEvents = 0;
	        do
	        {
	            // sleep
	            Thread.Sleep(1000);
	            currTime = PerformanceObserver.MilliTime;
	            deltaSeconds = (currTime - startTime) / 1000.0;

	            // report statistics
	            int totalEvents = 0;
	            int totalZoneMoves = 0;
	            int totalZoneSplits = 0;
	            int totalZoneSame = 0;
	            for (int i = 0; i < callables.length; i++)
	            {
	                totalEvents += callables[i].GetNumEventsSend();
	                totalZoneMoves += callables[i].GetNumZoneMoves();
	                totalZoneSplits += callables[i].GetNumZoneSplits();
	                totalZoneSame += callables[i].GetNumSameZone();
	            }
	            double throughputOverall = totalEvents / deltaSeconds;
	            double totalLastBatch = totalEvents - lastTotalEvents;
	            log.Info("totalEvents=" + totalEvents +
	                    " delta=" + deltaSeconds +
	                    " throughputOverall=" + throughputOverall +
	                    " lastBatch=" + totalLastBatch +
	                    " zoneMoves=" + totalZoneMoves +
	                    " zoneSame=" + totalZoneSame +
	                    " zoneSplits=" + totalZoneSplits
	                    );
	            lastTotalEvents = totalEvents;

	            // If we are within 15 seconds of shutdown, stop generating zone splits
	            if ( ((numSeconds - deltaSeconds) < 15) && (callables[0].IsGenerateZoneSplit()))
	            {
	                log.Info(".tryPerf Setting stop split flag on threads");
	                for (int i = 0; i < callables.length; i++)
	                {
	                    callables[i].SetGenerateZoneSplit(false);
	                }
	            }
	        }
	        while (deltaSeconds < numSeconds);

	        log.Info(".tryPerf Shutting down threads");
	        for (int i = 0; i < callables.length; i++)
	        {
	            callables[i].SetShutdown(true);
	        }
	        threadPool.Shutdown();
	        threadPool.AwaitTermination(10, TimeUnit.SECONDS);

	        if (!isAssert)
	        {
	            return;
	        }

	        for (int i = 0; i < numThreads; i++)
	        {
	            if (!(Boolean) future[i].Get())
	            {
	                throw new RuntimeException("Invalid result of callable");
	            }
	        }

	        // Get groups split
	        Set<Integer> splitGroups = new HashSet<Integer>();
	        for (int i = 0; i < callables.length; i++)
	        {
	            splitGroups.AddAll(callables[i].GetSplitZoneGroups());
	        }
	        log.Info(".tryPerf Generated splits were " + splitGroups + " groups");

	        // Compare to listeners
	        foreach (Integer groupId in splitGroups)
	        {
	            if (listeners[groupId].GetCallbacks().Size() == 0)
	            {
	                throw new RuntimeException("Invalid result for listener, expected split group");
	            }
	        }
	    }

	    // Subdivide say 1000 groups into 3 threads, i.e. 0 - 333, 334 to 666, 667 - 999 (roughly)
	    private Integer[][] GetGroupsPerThread(int numGroups, int numThreads)
	    {
	        Integer[][] result = new Integer[numThreads][];
	        int bucketSize = numGroups / numThreads;
	        for (int i = 0; i < numThreads; i++)
	        {
	            int start = i * bucketSize;
	            int end = start + bucketSize;
	            List<Integer> groups = new ArrayList<Integer>();

	            for (int j = start; j < end; j++)
	            {
	                groups.Add(j);
	            }

	            result[i] = groups.ToArray(new Integer[0]);
	            log.Info(".tryPerf Thread " + i + " getting groups " + result[i][0] + " to " + result[i][result[i].length - 1]);
	        }
	        return result;
	    }
	}
} // End of namespace
