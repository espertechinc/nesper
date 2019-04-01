///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionEnumOrPublicConstantValue : CodegenExpression
    {
        private readonly Type enumType;
        private readonly string enumTypeString;
        private readonly string enumValue;

        public CodegenExpressionEnumOrPublicConstantValue(Type enumType, string enumValue)
        {
            this.enumType = enumType;
            enumTypeString = null;
            this.enumValue = enumValue;
        }

        public CodegenExpressionEnumOrPublicConstantValue(string enumTypeString, string enumValue)
        {
            this.enumTypeString = enumTypeString;
            this.enumValue = enumValue;
            enumType = null;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            if (enumType != null) {
                AppendClassName(builder, enumType, null, imports);
            }
            else {
                builder.Append(enumTypeString);
            }

            builder.Append(".");
            builder.Append(enumValue);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(enumType);
        }
    }
} // end of namespace