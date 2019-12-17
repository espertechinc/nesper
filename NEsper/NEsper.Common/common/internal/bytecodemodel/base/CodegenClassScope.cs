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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenClassScope : CodegenScope
    {
        public CodegenClassScope(
            bool debug,
            CodegenNamespaceScope namespaceScope,
            string outermostClassName)
            : base(debug)
        {
            Id = Guid.NewGuid();

            NamespaceScope = namespaceScope;
            OutermostClassName = outermostClassName;

            // Work in progress: trying to find a good way to indicate that the default name for state
            // fields being managed in the statement fields object are exposed using this instance ref.
            // However, sometimes, the instance ref is something else (e.g. this) and needs to be
            // assignable.

            InstanceRef = CodegenExpressionBuilder.Ref("statementFields");
        }

        public Guid Id { get; }

        public string ClassName { get; }

        public CodegenNamespaceScope NamespaceScope { get; }

        public string OutermostClassName { get; }

        public IList<CodegenInnerClass> AdditionalInnerClasses { get; } = new List<CodegenInnerClass>();

        public bool IsInstrumented => NamespaceScope.IsInstrumented;

        public CodegenExpressionRef InstanceRef { get; set; }

        public CodegenExpressionInstanceField AddInstanceFieldUnshared<T>(
            CodegenExpression instance,
            bool isFinal,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return AddInstanceFieldUnshared(instance, isFinal, typeof(T), assignScopedPackageInitMethod);
        }

        public CodegenExpressionInstanceField AddDefaultFieldUnshared<T>(
            bool isFinal,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return AddInstanceFieldUnshared<T>(
                InstanceRef,
                isFinal,
                assignScopedPackageInitMethod);
        }

        public CodegenExpressionInstanceField AddInstanceFieldUnshared(
            CodegenExpression instance,
            bool isFinal,
            Type type,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return NamespaceScope.AddInstanceFieldUnshared(
                instance, isFinal, type, assignScopedPackageInitMethod);
        }

        public CodegenExpressionInstanceField AddDefaultFieldUnshared(
            bool isFinal,
            Type type,
            CodegenExpression assignScopedPackageInitMethod)
        {
            return AddInstanceFieldUnshared(
                InstanceRef,
                isFinal,
                type,
                assignScopedPackageInitMethod);
        }

#if DEPRECATED
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
            return NamespaceScope.AddFieldUnshared(isFinal, type, assignScopedPackageInitMethod);
        }
#endif

        public CodegenExpressionInstanceField AddOrGetInstanceFieldSharable(
            CodegenExpression instance,
            CodegenFieldSharable sharable)
        {
            return NamespaceScope.AddOrGetInstanceFieldSharable(instance, sharable);
        }

        public CodegenExpressionInstanceField AddOrGetDefaultFieldSharable(
            CodegenFieldSharable sharable)
        {
            return NamespaceScope.AddOrGetInstanceFieldSharable(
                InstanceRef, sharable);
        }

#if DEPRECATED
        public CodegenExpressionField AddOrGetFieldSharable(CodegenFieldSharable sharable)
        {
            return NamespaceScope.AddOrGetFieldSharable(sharable);
        }
#endif

#if DEPRECATED
        public CodegenExpressionField AddOrGetFieldWellKnown(
            CodegenFieldName fieldName,
            Type type)
        {
            return NamespaceScope.AddOrGetFieldWellKnown(fieldName, type);
        }
#endif

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
            return NamespaceScope.AddSubstitutionParameter(name, type);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(ClassName)}: {ClassName}, {nameof(NamespaceScope)}: {NamespaceScope}, {nameof(OutermostClassName)}: {OutermostClassName}";
        }
    }
} // end of namespace