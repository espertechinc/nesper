///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportLegacyBeanInt
    {
        public int fieldIntPrimitive;

        public SupportLegacyBeanInt(int fieldIntPrimitive)
        {
            this.fieldIntPrimitive = fieldIntPrimitive;
        }

        public int IntPrimitive => fieldIntPrimitive;

        public int GetIntPrimitive()
        {
            return fieldIntPrimitive;
        }

        public int ReadIntPrimitive()
        {
            return fieldIntPrimitive;
        }
    }
} // end of namespace