///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;

namespace com.espertech.esper.regression.client
{
    public class MyAnnotationNestedAttribute : Attribute
    {
        [Required] public MyAnnotationNestableSimpleAttribute NestableSimple { get; set; }
        [Required] public MyAnnotationNestableValuesAttribute NestableValues { get; set; }
        [Required] public MyAnnotationNestableNestableAttribute NestableNestable { get; set; }
    }
}
