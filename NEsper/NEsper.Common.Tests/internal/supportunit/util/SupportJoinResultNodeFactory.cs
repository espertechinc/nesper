///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportJoinResultNodeFactory
    {
        private readonly IContainer _container;

        public SupportJoinResultNodeFactory(IContainer container)
        {
            _container = container;
        }

        public static SupportJoinResultNodeFactory GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new SupportJoinResultNodeFactory(container));
        }

        public static void RegisterSingleton(IContainer container)
        {
            container.Register<SupportJoinResultNodeFactory>(
                xx => new SupportJoinResultNodeFactory(container),
                Lifespan.Singleton);
        }

        public IList<Node>[] MakeOneStreamResult(
            int numStreams,
            int fillStream,
            int numNodes,
            int numEventsPerNode)
        {
            var resultsPerStream = new IList<Node>[numStreams];
            resultsPerStream[fillStream] = new List<Node>();

            for (var i = 0; i < numNodes; i++)
            {
                var node = MakeNode(i, numEventsPerNode);
                resultsPerStream[fillStream].Add(node);
            }

            return resultsPerStream;
        }

        public Node MakeNode(
            int streamNum,
            int numEventsPerNode)
        {
            var node = new Node(streamNum);
            node.Events = MakeEventSet(numEventsPerNode);
            return node;
        }

        public ISet<EventBean> MakeEventSet(
            int numObjects)
        {
            if (numObjects == 0)
            {
                return null;
            }

            ISet<EventBean> set = new HashSet<EventBean>();
            for (var i = 0; i < numObjects; i++)
            {
                set.Add(MakeEvent());
            }

            return set;
        }

        public ISet<EventBean>[] MakeEventSets(int[] numObjectsPerSet)
        {
            ISet<EventBean>[] sets = new HashSet<EventBean>[numObjectsPerSet.Length];
            for (var i = 0; i < numObjectsPerSet.Length; i++)
            {
                if (numObjectsPerSet[i] == 0)
                {
                    continue;
                }

                sets[i] = MakeEventSet(numObjectsPerSet[i]);
            }

            return sets;
        }

        public EventBean MakeEvent()
        {
            var theEvent = SupportEventBeanFactory.CreateObject(
                SupportEventTypeFactory.GetInstance(_container), new SupportBean());
            return theEvent;
        }

        public EventBean[] MakeEvents(
            int numEvents)
        {
            var events = new EventBean[numEvents];
            for (var i = 0; i < events.Length; i++)
            {
                events[i] = MakeEvent();
            }

            return events;
        }

        public EventBean[][] ConvertTo2DimArr(IList<EventBean[]> rowList)
        {
            var result = new EventBean[rowList.Count][];

            var count = 0;
            foreach (var row in rowList)
            {
                result[count++] = row;
            }

            return result;
        }
    }
} // end of namespace