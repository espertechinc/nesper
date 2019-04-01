///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    public sealed class SupportBeanTypeChange
    {
        private readonly int? _intBoxed;
        private readonly String _intPrimitive;

        public SupportBeanTypeChange(int? intBoxed, String intPrimitive)
        {
            _intBoxed = intBoxed;
            _intPrimitive = intPrimitive;
        }

        public int? IntBoxed
        {
            get { return _intBoxed; }
        }

        public string IntPrimitive
        {
            get { return IntPrimitive; }
        }
    }
}
