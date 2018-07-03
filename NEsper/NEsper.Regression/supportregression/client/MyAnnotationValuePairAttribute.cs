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
    public class MyAnnotationValuePairAttribute : Attribute
    {
        public MyAnnotationValuePairAttribute()
        {
            StringValDef = "def";
            IntValDef = 100;
            LongValDef = 200;
            BooleanValDef = true;
            CharValDef = 'D';
            DoubleValDef = 1.1;
        }

        [Required] public string StringVal { get; set; }
        public byte ByteVal { get; set; }
        public short ShortVal { get; set; }
        public int IntVal { get; set; }
        public long LongVal { get; set; }
        [Required] public bool BooleanVal { get; set; }
        public char CharVal { get; set; }
        public double DoubleVal { get; set; }

        public string StringValDef { get; set; }
        public byte ByteValDef { get; set; }
        public short ShortValDef { get; set; }
        public int IntValDef { get; set; }
        public long LongValDef { get; set; }
        public bool BooleanValDef { get; set; }
        public char CharValDef { get; set; }
        public double DoubleValDef { get; set; }
    }
}
