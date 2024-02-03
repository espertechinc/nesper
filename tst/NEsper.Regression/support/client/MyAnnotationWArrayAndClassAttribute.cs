///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.regressionlib.support.client
{
    public class MyAnnotationWArrayAndClassAttribute : Attribute
    {
        public PriorityAttribute[] Priorities { get; set; }

        public Type ClassOne { get; set; }

        public Type ClassTwo { get; set; }
    }
} // end of namespace