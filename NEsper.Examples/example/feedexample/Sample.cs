///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.Support;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.FeedExample
{
	[TestFixture]
	public class Sample
	{
        [Test]
	    public void TestIt()
	    {
	        var stmtText =
	                "insert into ThroughputPerFeed " +
	                " select feed, count(*) as cnt " +
	                "from " + typeof(FeedEvent).FullName + ".win:time_batch(1 sec) " +
	                "group by feed";

	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

	        var runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
	        var stmt = runtime.DeployStatement(stmtText);
            stmt.Events += DisplayEvents;

	        /*
	        while(true)
	        {
	            FeedEvent event;
	            event = new FeedEvent(FeedEnum.FEED_A, "IBM", 70);
	            engine.GetEPRuntime().SendEvent(event);
	            event = new FeedEvent(FeedEnum.FEED_B, "IBM", 70);
	            engine.GetEPRuntime().SendEvent(event);
	        }
	        */
	    }

        public void DisplayEvents(Object sender, UpdateEventArgs e)
        {
            foreach (var @event in e.NewEvents)
            {
                Console.WriteLine("feed {0} is count {1}",
                                  @event.Get("feed"),
                                  @event.Get("cnt"));
            }
        }
	}

} // End of namespace
