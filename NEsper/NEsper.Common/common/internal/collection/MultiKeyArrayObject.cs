///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.collection
{
    public sealed class MultiKeyArrayObject : MultiKeyArrayBase<object>
    {
        public MultiKeyArrayObject(Array keys) : base(ArrayToObjectArray(keys))
        {
        }

        public static object[] ArrayToObjectArray(Array input)
        {
            if (input == null) {
                return null;
            }
            if (input is object[] inputAsTypedArray) {
                return inputAsTypedArray;
            }

            var target = new object[input.Length];
            input.CopyTo(target, 0);
            return target;
        }
    }
} // end of namespace