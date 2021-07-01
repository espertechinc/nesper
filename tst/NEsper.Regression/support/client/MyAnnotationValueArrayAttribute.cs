///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

using com.espertech.esper.compat.attributes;

namespace com.espertech.esper.regressionlib.support.client
{
    public class MyAnnotationValueArrayAttribute : Attribute
    {
        [Required]
        public long[] Value { get; set; }

        [Required]
        public int[] IntArray { get; set; }

        [Required]
        public double[] DoubleArray { get; set; }

        [Required]
        public string[] StringArray { get; set; }

        [DefaultValue(new string[] { "XYZ" })]
        public string[] StringArrayDef { get; set; }
    }
} // end of namespace