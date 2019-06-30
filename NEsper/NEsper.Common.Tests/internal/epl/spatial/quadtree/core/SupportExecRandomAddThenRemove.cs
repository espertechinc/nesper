///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportExecRandomAddThenRemove
    {
        public static void RunAssertion<L>(SupportQuadTreeToolUnique<L> tools)
        {
            SupportQuadTreeConfig[] configs = {
                new SupportQuadTreeConfig(0, 0, 100, 100, 4, 20),
                new SupportQuadTreeConfig(50, 80, 20, 900, 4, 20),
                new SupportQuadTreeConfig(50, 80, 20, 900, 2, 80),
                new SupportQuadTreeConfig(50, 800000, 2000, 900, 1000, 80)
            };

            foreach (var config in configs)
            {
                RunAssertion(100, 100, config, tools);
            }
        }

        private static void RunAssertion<L>(
            int numPoints,
            int numQueries,
            SupportQuadTreeConfig config,
            SupportQuadTreeToolUnique<L> tools)
        {
            L quadTree = tools.factory.Invoke(config);

            // generate
            var random = new Random();
            var rectangles = tools.generator.Generate(random, numPoints, config.X, config.Y, config.Width, config.Height);

            // add
            foreach (var rectangle in rectangles)
            {
                tools.adderUnique.Invoke(quadTree, rectangle);
            }

            // query
            for (var i = 0; i < numQueries; i++)
            {
                SupportQuadTreeUtil.RandomQuery(
                    quadTree,
                    rectangles,
                    random,
                    config.X,
                    config.Y,
                    config.Width,
                    config.Height,
                    tools.querier,
                    tools.pointInsideChecking);
            }

            // remove point-by-point
            while (!rectangles.IsEmpty())
            {
                int removeIndex = random.Next(rectangles.Count);
                SupportRectangleWithId removed = rectangles.DeleteAt(removeIndex);
                tools.remover.Invoke(quadTree, removed);

                for (var i = 0; i < numQueries; i++)
                {
                    SupportQuadTreeUtil.RandomQuery(
                        quadTree,
                        rectangles,
                        random,
                        config.X,
                        config.Y,
                        config.Width,
                        config.Height,
                        tools.querier,
                        tools.pointInsideChecking);
                }
            }
        }
    }
} // end of namespace