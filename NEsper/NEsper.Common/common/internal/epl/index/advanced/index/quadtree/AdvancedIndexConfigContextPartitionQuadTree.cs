///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.lookup;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class AdvancedIndexConfigContextPartitionQuadTree : AdvancedIndexConfigContextPartition
    {
        private readonly double x;
        private readonly double y;
        private readonly double width;
        private readonly double height;
        private readonly int leafCapacity;
        private readonly int maxTreeHeight;

        public AdvancedIndexConfigContextPartitionQuadTree(double x, double y, double width, double height, int leafCapacity, int maxTreeHeight)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.leafCapacity = leafCapacity;
            this.maxTreeHeight = maxTreeHeight;
        }

        public CodegenExpression Make()
        {
            return NewInstance(typeof(AdvancedIndexConfigContextPartitionQuadTree),
                    Constant(x), Constant(y), Constant(width), Constant(height), Constant(leafCapacity), Constant(maxTreeHeight));
        }

        public double X
        {
            get => x;
        }

        public double Y
        {
            get => y;
        }

        public double Width
        {
            get => width;
        }

        public double Height
        {
            get => height;
        }

        public int LeafCapacity
        {
            get => leafCapacity;
        }

        public int MaxTreeHeight
        {
            get => maxTreeHeight;
        }

        public void ToConfiguration(StringWriter builder)
        {
            builder.Write(Convert.ToString(x, CultureInfo.InvariantCulture));
            builder.Write(",");
            builder.Write(Convert.ToString(y, CultureInfo.InvariantCulture));
            builder.Write(",");
            builder.Write(Convert.ToString(width, CultureInfo.InvariantCulture));
            builder.Write(",");
            builder.Write(Convert.ToString(height, CultureInfo.InvariantCulture));
            builder.Write(",");
            builder.Write(Convert.ToString(leafCapacity));
            builder.Write(",");
            builder.Write(Convert.ToString(maxTreeHeight));
        }
    }
} // end of namespace