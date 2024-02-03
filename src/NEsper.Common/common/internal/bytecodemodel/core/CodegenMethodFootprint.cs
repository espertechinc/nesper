///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.util;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenMethodFootprint
    {
        public CodegenMethodFootprint(
            Type returnType,
            string returnTypeName,
            IList<CodegenNamedParam> @params,
            string optionalComment)
        {
            if (returnType == null && returnTypeName == null) {
                throw new ArgumentException("Invalid null return type");
            }

            ReturnType = returnType;
            ReturnTypeName = returnTypeName;
            Params = @params;
            OptionalComment = optionalComment;
        }

        public Type ReturnType { get; }

        public string ReturnTypeName { get; }

        public IList<CodegenNamedParam> Params { get; }

        public string OptionalComment { get; }

        public void MergeClasses(ISet<Type> classes)
        {
            if (ReturnType != null) {
                classes.AddToSet(ReturnType);
            }

            foreach (var param in Params) {
                param.MergeClasses(classes);
            }
        }
    }
} // end of namespace