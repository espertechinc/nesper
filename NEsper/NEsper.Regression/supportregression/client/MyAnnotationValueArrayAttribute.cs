///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;

namespace com.espertech.esper.supportregression.client
{
    public class MyAnnotationValueArrayAttribute : Attribute
    {
        public MyAnnotationValueArrayAttribute()
        {
            StringArrayDef = new[] {"XYZ"};
        }

        public long[] Value { get; set; }
        public int[] IntArray { get; set; }
        [Required] public double[] DoubleArray { get; set; }
        public string[] StringArray { get; set; }
        public string[] StringArrayDef { get; set; }
    }
}
