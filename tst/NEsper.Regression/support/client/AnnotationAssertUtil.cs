///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.client
{
    public class AnnotationAssertUtil
    {
        public static Attribute[] SortAlpha(Attribute[] annotations)
        {
            if (annotations == null) {
                return null;
            }

            var sorted = new List<Attribute>();
            sorted.AddAll(Arrays.AsList(annotations));
            sorted.SortInPlace(
                (
                    o1,
                    o2) => o1.GetType().Name.CompareTo(o2.GetType().Name));
            return sorted.ToArray();
        }
    }
} // end of namespace