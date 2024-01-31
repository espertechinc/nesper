///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;

namespace com.espertech.esper.common.@internal.bytecodemodel.name
{
    public class CodegenFieldNameMatchRecognizePrevious : CodegenFieldName
    {
        public static readonly CodegenFieldNameMatchRecognizePrevious INSTANCE =
            new CodegenFieldNameMatchRecognizePrevious();

        private CodegenFieldNameMatchRecognizePrevious()
        {
        }

        public string Name => CodegenNamespaceScopeNames.PreviousMatchRecognize();
    }
} // end of namespace