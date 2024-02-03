///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportExecUniqueRandomIntPointsInSquare
    {
        public static void RunAssertion<L>(SupportQuadTreeToolUnique<L> tools)
        {
            ClassicAssert.IsTrue(tools.generator.Unique());

            SupportQuadTreeConfig[] configs = {
                new SupportQuadTreeConfig(0, 0, 1000, 1000, 4, 20),
                new SupportQuadTreeConfig(0, 0, 1000, 1000, 1000, 20),
                new SupportQuadTreeConfig(0, 0, 1000, 1000, 2, 50)
            };

            foreach (var config in configs)
            {
                RunAssertionPointsUnique(1000, config, tools);
            }
        }

        private static void RunAssertionPointsUnique<L>(
            int numPoints,
            SupportQuadTreeConfig config,
            SupportQuadTreeToolUnique<L> tools)
        {
            var random = new Random();
            L quadTree = tools.factory.Invoke(config);
            var points = tools.generator.Generate(random, numPoints, config.X, config.Y, config.Width, config.Height);

            // add
            foreach (var p in points)
            {
                tools.adderUnique.Invoke(quadTree, p);
            }

            // find all individually
            foreach (var p in points)
            {
                ICollection<object> values = tools.querier.Invoke(quadTree, p.X, p.Y, 0.9, 0.9);
                ClassicAssert.IsTrue(values != null && !values.IsEmpty(), "Failed to find " + p);
                ClassicAssert.AreEqual(1, values.Count);
                ClassicAssert.AreEqual(p.Id, values.First());
            }

            // get all content
            ICollection<object> all = tools.querier.Invoke(quadTree, config.X, config.Y, config.Width, config.Height);
            ClassicAssert.AreEqual(points.Count, all.Count);
            ClassicAssert.AreEqual(points.Count, new HashSet<object>(all).Count);
            foreach (var value in all)
            {
                ClassicAssert.IsInstanceOf<string>(value);
            }

            // remove all
            foreach (var p in points)
            {
                tools.remover.Invoke(quadTree, p);
            }

            ICollection<object> valuesLater = tools.querier.Invoke(quadTree, config.X, config.Y, config.Width, config.Height);
            ClassicAssert.IsNull(valuesLater);
        }
    }
} // end of namespace
