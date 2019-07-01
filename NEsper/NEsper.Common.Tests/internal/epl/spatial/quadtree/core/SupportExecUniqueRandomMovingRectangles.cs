///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.core.SupportQuadTreeUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportExecUniqueRandomMovingRectangles
    {
        public static void RunAssertion<L>(
            SupportQuadTreeToolUnique<L> tools,
            double rectangleWidth,
            double rectangleHeight)
        {
            Assert.IsNull(tools.generator);

            SupportQuadTreeConfig[] configs = {
                new SupportQuadTreeConfig(0, 0, 100, 100, 4, 20),
                new SupportQuadTreeConfig(0, 0, 100, 100, 100, 20),
                new SupportQuadTreeConfig(0, 0, 100, 100, 4, 100),
                new SupportQuadTreeConfig(10, 8000, 90, 2000, 4, 100)
            };

            foreach (var config in configs)
            {
                RunAssertion(1000, 1000, 5, config, tools, rectangleWidth, rectangleHeight);
                RunAssertion(1000, 1000, 1, config, tools, rectangleWidth, rectangleHeight);
            }
        }

        private static void RunAssertion<L>(
            int numPoints,
            int numMoves,
            int queryFrameSize,
            SupportQuadTreeConfig config,
            SupportQuadTreeToolUnique<L> tools,
            double rectangleWidth,
            double rectangleHeight)
        {
            var random = new Random();
            var quadTree = tools.factory.Invoke(config);

            // generate
            var points = GenerateIntegerCoordinates(random, numPoints, config, rectangleWidth, rectangleHeight);

            // add
            foreach (var point in points.Values)
            {
                tools.adderUnique.Invoke(quadTree, point);
            }

            // move points
            for (var i = 0; i < numMoves; i++)
            {
                var p = MovePoint(points, quadTree, random, config, tools.adderUnique, tools.remover);

                var qx = p.X - queryFrameSize;
                var qy = p.Y - queryFrameSize;
                double qwidth = queryFrameSize * 2;
                double qheight = queryFrameSize * 2;
                var values = tools.querier.Invoke(quadTree, qx, qy, qwidth, qheight);
                AssertIds(points.Values, values, qx, qy, qwidth, qheight, tools.pointInsideChecking);
            }
        }

        private static XYPoint MovePoint<L>(
            IDictionary<XYPoint, SupportRectangleWithId> points,
            L quadTree,
            Random random,
            SupportQuadTreeConfig config,
            AdderUnique<L> adder,
            Remover<L> remover)
        {
            var coordinates = points.Keys.ToArray();
            XYPoint oldCoordinate;
            XYPoint newCoordinate;
            while (true)
            {
                oldCoordinate = coordinates[random.Next(coordinates.Length)];
                var direction = random.Next(4);
                var newX = oldCoordinate.X;
                var newY = oldCoordinate.Y;
                if (direction == 0 && newX > config.X)
                {
                    newX--;
                }

                if (direction == 1 && newY > config.Y)
                {
                    newY--;
                }

                if (direction == 2 && newX < config.X + config.Width - 1)
                {
                    newX++;
                }

                if (direction == 3 && newY < config.Y + config.Height - 1)
                {
                    newY++;
                }

                newCoordinate = new XYPoint(newX, newY);
                if (!points.ContainsKey(newCoordinate))
                {
                    break;
                }
            }

            var moved = points.Delete(oldCoordinate);
            remover.Invoke(quadTree, moved);
            moved.X = newCoordinate.X;
            moved.Y = newCoordinate.Y;
            adder.Invoke(quadTree, moved);
            points.Put(newCoordinate, moved);

            // Comment-me-in:
            // log.info("Moving " + moved.getId() + " from " + printPoint(oldCoordinate.getX(), oldCoordinate.getY()) + " to " + printPoint(newCoordinate.getX(), newCoordinate.getY()));

            return newCoordinate;
        }

        private static IDictionary<XYPoint, SupportRectangleWithId> GenerateIntegerCoordinates(
            Random random,
            int numPoints,
            SupportQuadTreeConfig config,
            double rectangleWidth,
            double rectangleHeight)
        {
            IDictionary<XYPoint, SupportRectangleWithId> result = new Dictionary<XYPoint, SupportRectangleWithId>();
            var pointNum = 0;
            while (result.Count < numPoints)
            {
                var x = (int) config.X + random.Next((int) config.Width);
                var y = (int) config.Y + random.Next((int) config.Height);
                var p = new XYPoint(x, y);
                if (result.ContainsKey(p))
                {
                    continue;
                }

                result.Put(p, new SupportRectangleWithId("P" + pointNum, x, y, rectangleWidth, rectangleHeight));
                pointNum++;
            }

            return result;
        }
    }
} // end of namespace
