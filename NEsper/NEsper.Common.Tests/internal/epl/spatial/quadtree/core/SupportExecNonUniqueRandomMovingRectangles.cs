///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.core.SupportQuadTreeUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportExecNonUniqueRandomMovingRectangles
    {
        public static void RunAssertion<L>(SupportQuadTreeToolNonUnique<L> tools)
        {
            SupportQuadTreeConfig[] configs = {
                new SupportQuadTreeConfig(0, 0, 100, 100, 4, 20),
                new SupportQuadTreeConfig(0, 0, 10, 10, 4, 20),
                new SupportQuadTreeConfig(0, 0, 10, 10, 100, 20),
                new SupportQuadTreeConfig(0, 0, 10, 10, 4, 100)
            };

            foreach (var config in configs)
            {
                RunAssertion(1000, 2000, 5, config, tools);
                RunAssertion(2, 1000, 1, config, tools);
                RunAssertion(1000, 1000, 1, config, tools);
            }

            // test performance
            long start = PerformanceObserver.MilliTime;
            RunAssertion(1000, 1000, 10, new SupportQuadTreeConfig(0, 0, 100, 100, 4, 20), tools);
            var delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(1000), () => "Time taken: " + delta); // rough and leaves room for GC etc, just a performance smoke test
        }

        private static void RunAssertion<L>(
            int numPoints,
            int numMoves,
            int queryFrameSize,
            SupportQuadTreeConfig config,
            SupportQuadTreeToolNonUnique<L> tools)
        {
            var random = new Random();
            L quadTree = tools.factory.Invoke(config);
            var points = tools.generator.Generate(random, numPoints, config.X, config.Y, config.Width, config.Height);

            foreach (var point in points)
            {
                tools.adderNonUnique.Invoke(quadTree, point);
            }

            for (var i = 0; i < numMoves; i++)
            {
                SupportRectangleWithId moved = points[(random.Next(points.Count))];
                Move(moved, quadTree, random, config, tools);

                var startX = moved.X - queryFrameSize;
                var startY = moved.Y - queryFrameSize;
                double widthQ = queryFrameSize * 2;
                double heightQ = queryFrameSize * 2;
                ICollection<object> values = tools.querier.Invoke(quadTree, startX, startY, widthQ, heightQ);
                AssertIds(points, values, startX, startY, widthQ, heightQ, tools.pointInsideChecking);
            }
        }

        private static void Move<L>(
            SupportRectangleWithId rectangle,
            L quadTree,
            Random random,
            SupportQuadTreeConfig config,
            SupportQuadTreeToolNonUnique<L> tools)
        {
            tools.remover.Invoke(quadTree, rectangle);

            double newX;
            double newY;
            while (true)
            {
                int direction = random.Next(4);
                newX = rectangle.X;
                newY = rectangle.Y;
                if (direction == 0)
                {
                    newX--;
                }
                else if (direction == 1)
                {
                    newY--;
                }
                else if (direction == 2)
                {
                    newX++;
                }
                else if (direction == 3)
                {
                    newY++;
                }

                if (tools.pointInsideChecking)
                {
                    if (BoundingBox.ContainsPoint(config.X, config.Y, config.Width, config.Height, newX, newY))
                    {
                        break;
                    }
                }
                else
                {
                    if (BoundingBox.IntersectsBoxIncludingEnd(config.X, config.Y, config.MaxX, config.MaxY, newX, newY, rectangle.W, rectangle.H))
                    {
                        break;
                    }
                }
            }

            // Comment-me-in:
            // log.info("Moving " + point.getId() + " from " + printPoint(point.getX(), point.getY()) + " to " + printPoint(newX, newY));

            rectangle.X = newX;
            rectangle.Y = newY;
            tools.adderNonUnique.Invoke(quadTree, rectangle);
        }
    }
} // end of namespace