///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.util;

namespace com.espertech.esper.codegen.core
{
    public class CodeGenerationIDGenerator
    {
        public static string GenerateMethod()
        {
            return "m" + UuidGenerator.GenerateNoDash();
        }

        public static string GenerateMember()
        {
            return "_" + UuidGenerator.GenerateNoDash();
        }

        public static string GenerateClass()
        {
            return "c" + UuidGenerator.GenerateNoDash();
        }
    }
} // end of namespace