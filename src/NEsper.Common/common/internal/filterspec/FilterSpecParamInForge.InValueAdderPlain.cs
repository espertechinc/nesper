﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed partial class FilterSpecParamInForge
    {
        public class InValueAdderPlain : FilterSpecParamInAdder
        {
            public static readonly InValueAdderPlain INSTANCE = new InValueAdderPlain();

            private InValueAdderPlain()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                constants.Add(value);
            }

            public void ValueToString(StringBuilder @out)
            {
                @out.Append("collection");
            }
        }
    }
}