///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.collection
{
    public class SuperEnumerator
    {
        public static IEnumerator<T> For<T>(
            IEnumerator<T> first,
            IEnumerator<T> second)
        {
            if (first != null) {
                while (first.MoveNext()) {
                    yield return first.Current;
                }
            }

            if (second != null) {
                while (second.MoveNext()) {
                    yield return second.Current;
                }
            }
        }
    }
}