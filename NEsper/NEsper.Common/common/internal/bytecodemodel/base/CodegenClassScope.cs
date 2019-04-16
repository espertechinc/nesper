///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenClassScope : CodegenScope
    {
        public CodegenClassScope(
            bool debug,
            CodegenPackageScope packageScope,
            string outermostClassName)
            : base(debug)
        {
            PackageScope = packageScope;
            OutermostClassName = outermostClassName;
        }

        public CodegenPackageScope PackageScope { get; }

        public string OutermostClassName { get; }

        public IList<CodegenInnerClass> AdditionalInnerClasses { get; } = new List<CodegenInnerClass>();

        public bool IsInstrumented => PackageScope.IsInstrumented;

        public CodegenExpressionField AddFieldUnshared<T>(
            bool isFinal,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return AddFieldUnshared(isFinal, typeof(T), assignScopedPackageInitMethod);
        }

        public CodegenExpressionField AddFieldUnshared(
            bool isFinal,
            Type type,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return PackageScope.AddFieldUnshared(isFinal, type, assignScopedPackageInitMethod);
        }

        public CodegenExpressionField AddOrGetFieldSharable(CodegenFieldSharable sharable)
        {
            return PackageScope.AddOrGetFieldSharable(sharable);
        }

        public CodegenExpressionField AddOrGetFieldWellKnown(
            CodegenFieldName fieldName,
            Type type)
        {
            return PackageScope.AddOrGetFieldWellKnown(fieldName, type);
        }

        public void AddInnerClass(CodegenInnerClass innerClass)
        {
            AdditionalInnerClasses.Add(innerClass);
        }

        public void AddInnerClasses(IList<CodegenInnerClass> innerClasses)
        {
            AdditionalInnerClasses.AddAll(innerClasses);
        }

        public CodegenField AddSubstitutionParameter(
            string name,
            Type type)
        {
            return PackageScope.AddSubstitutionParameter(name, type);
        }
    }
} // end of namespace