///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed partial class FilterSpecParamInForge
    {
        public class InValueAdderArray : FilterSpecParamInAdder
        {
            public static readonly InValueAdderArray INSTANCE = new InValueAdderArray();

            private InValueAdderArray()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                var array = (Array)value;
                var len = array.Length;
                for (var i = 0; i < len; i++) {
                    constants.Add(array.GetValue(i));
                }
            }

            public void ValueToString(StringBuilder @out)
            {
                @out.Append("collection");
            }
        }
    }
}