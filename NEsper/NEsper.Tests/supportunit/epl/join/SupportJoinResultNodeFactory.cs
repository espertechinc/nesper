///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.supportunit.events;


namespace com.espertech.esper.supportunit.epl.join
{
    public class SupportJoinResultNodeFactory
    {
        public static IList<Node>[] MakeOneStreamResult(int numStreams, int fillStream, int numNodes, int numEventsPerNode)
        {
            IList<Node>[] resultsPerStream = new IList<Node>[numStreams];
            resultsPerStream[fillStream] = new List<Node>();
    
            for (int i = 0; i < numNodes; i++)
            {
                Node node = MakeNode(i, numEventsPerNode);
                resultsPerStream[fillStream].Add(node);
            }
    
            return resultsPerStream;
        }
    
        public static Node MakeNode(int streamNum, int numEventsPerNode)
        {
            Node node = new Node(streamNum);
            node.Events = MakeEventSet(numEventsPerNode);
            return node;
        }
    
        public static ISet<EventBean> MakeEventSet(int numObjects)
        {
            if (numObjects == 0)
            {
                return null;
            }
            ISet<EventBean> set = new HashSet<EventBean>();
            for (int i = 0; i < numObjects; i++)
            {
                set.Add(MakeEvent());
            }
            return set;
        }

        public static ISet<EventBean>[] MakeEventSets(int[] numObjectsPerSet)
        {
            ISet<EventBean>[] sets = new HashSet<EventBean>[numObjectsPerSet.Length];
            for (int i = 0; i < numObjectsPerSet.Length; i++)
            {
                if (numObjectsPerSet[i] == 0)
                {
                    continue;
                }
                sets[i] = MakeEventSet(numObjectsPerSet[i]);
            }
            return sets;
        }
    
        public static EventBean MakeEvent()
        {
            EventBean theEvent = SupportEventBeanFactory.CreateObject(new Object());
            return theEvent;
        }
    
        public static EventBean[] MakeEvents(int numEvents)
        {
            EventBean[] events = new EventBean[numEvents];
            for (int i = 0; i < events.Length; i++)
            {
                events[i] = MakeEvent();
            }
            return events;
        }
    
        public static EventBean[][] ConvertTo2DimArr(IList<EventBean[]> rowList)
        {
            EventBean[][] result = new EventBean[rowList.Count][];
    
            int count = 0;
            foreach (EventBean[] row in rowList)
            {
                result[count++] = row;
            }
    
            return result;
        }
    }
}
