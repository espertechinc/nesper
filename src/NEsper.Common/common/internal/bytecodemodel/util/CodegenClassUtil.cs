///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenClassUtil
    {
        public static Type GetComponentTypeOutermost(Type clazz)
        {
            if (!clazz.IsArray) {
                return clazz;
            }

            return GetComponentTypeOutermost(clazz.GetElementType());
        }

        public static int GetNumberOfDimensions(Type clazz)
        {
            if (clazz.GetElementType() == null) {
                return 0;
            }
            else {
                return GetNumberOfDimensions(clazz.GetElementType()) + 1;
            }
        }
    }
} // end of namespace