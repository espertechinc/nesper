///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        [Required]
        public string StringVal { get; set; }
        [Required]
        public byte ByteVal { get; set; }
        [Required]
        public short ShortVal { get; set; }
        [Required]
        public int IntVal { get; set; }
        [Required]
        public long LongVal { get; set; }
        [Required]
        public bool BooleanVal { get; set; }
        [Required]
        public char CharVal { get; set; }
        [Required]
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