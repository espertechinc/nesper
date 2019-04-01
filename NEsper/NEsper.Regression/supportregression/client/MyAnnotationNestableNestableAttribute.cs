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
    public sealed class MyAnnotationNestableNestableAttribute : Attribute
    {
        [Required]
        public string Value { get; set; }

        public MyAnnotationNestableNestableAttribute()
        {
        }

        public MyAnnotationNestableNestableAttribute(string value)
        {
            Value = value;
        }
    }
}
