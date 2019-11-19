///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

namespace com.espertech.esper.regressionlib.support.client
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

        public string StringVal { get; set; }
        public byte ByteVal { get; set; }
        public short ShortVal { get; set; }
        public int IntVal { get; set; }
        public long LongVal { get; set; }
        public bool BooleanVal { get; set; }
        public char CharVal { get; set; }
        public double DoubleVal { get; set; }

        [DefaultValue("def")]
        public string StringValDef { get; set; }
        [DefaultValue(100)]
        public int IntValDef { get; set; }
        [DefaultValue(200L)]
        public long LongValDef { get; set; }
        [DefaultValue(true)]
        public bool BooleanValDef { get; set; }
        [DefaultValue('D')]
        public char CharValDef { get; set; }
        [DefaultValue(1.1d)]
        public double DoubleValDef { get; set; }
    }
} // end of namespace