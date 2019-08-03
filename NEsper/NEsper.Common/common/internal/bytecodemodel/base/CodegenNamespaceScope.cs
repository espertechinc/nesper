///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenNamespaceScope
    {
        // Well-named fields
        private readonly IDictionary<CodegenFieldName, CodegenField> _fieldsNamed =
            new LinkedHashMap<CodegenFieldName, CodegenField>();

        // Shared fields
        private readonly IDictionary<CodegenFieldSharable, CodegenField> _fieldsShared =
            new LinkedHashMap<CodegenFieldSharable, CodegenField>();

        // Unshared fields
        private readonly IDictionary<CodegenField, CodegenExpression> _fieldsUnshared =
            new LinkedHashMap<CodegenField, CodegenExpression>();

        private int _currentMemberNumber;
        private int _currentSubstitutionParamNumber;

        private readonly IDictionary<string, CodegenSubstitutionParamEntry> _substitutionParamsByName =
            new LinkedHashMap<string, CodegenSubstitutionParamEntry>();

        // Substitution parameters

        public CodegenNamespaceScope(
            string @namespace,
            string fieldsClassNameOptional,
            bool instrumented)
        {
            Namespace = @namespace;
            FieldsClassNameOptional = fieldsClassNameOptional;
            IsInstrumented = instrumented;
            InitMethod = CodegenMethod
                .MakeParentNode(
                    typeof(void),
                    typeof(CodegenNamespaceScope),
                    new CodegenClassScope(true, this, null))
                .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref)
                .WithStatic(true);
        }

        public string Namespace { get; }

        public CodegenMethod InitMethod { get; }

        public IDictionary<CodegenFieldName, CodegenField> FieldsNamed => _fieldsNamed;

        public IDictionary<CodegenField, CodegenExpression> FieldsUnshared => _fieldsUnshared;

        public string FieldsClassNameOptional { get; }

        public IList<CodegenSubstitutionParamEntry> SubstitutionParamsByNumber { get; } =
            new List<CodegenSubstitutionParamEntry>();

        public IDictionary<string, CodegenSubstitutionParamEntry> SubstitutionParamsByName => _substitutionParamsByName;

        public bool IsInstrumented { get; }

        public CodegenExpressionField AddFieldUnshared<T>(
            bool isFinal,
            CodegenExpression initCtorScoped)
        {
            return AddFieldUnshared(isFinal, typeof(T), initCtorScoped);
        }

        public CodegenExpressionField AddFieldUnshared(
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            if (FieldsClassNameOptional == null) {
                throw new IllegalStateException("No fields class name");
            }

            return Field(AddFieldUnsharedInternal(isFinal, type, initCtorScoped));
        }

        public CodegenExpressionField AddOrGetFieldSharable(CodegenFieldSharable sharable)
        {
            var member = _fieldsShared.Get(sharable);
            if (member != null) {
                return Field(member);
            }

            member = AddFieldUnsharedInternal(true, sharable.Type(), sharable.InitCtorScoped());
            _fieldsShared.Put(sharable, member);
            return Field(member);
        }

        public CodegenExpressionField AddOrGetFieldWellKnown(
            CodegenFieldName fieldName,
            Type type)
        {
            var existing = _fieldsNamed.Get(fieldName);
            if (existing != null) {
                if (existing.Type != type) {
                    throw new IllegalStateException(
                        "Field '" +
                        fieldName +
                        "' already registered with a different type, registered with " +
                        existing.Type.GetSimpleName() +
                        " but provided " +
                        type.GetSimpleName());
                }

                return Field(existing);
            }

            var field = new CodegenField(FieldsClassNameOptional, fieldName.Name, type, null, false);
            _fieldsNamed.Put(fieldName, field);
            return Field(field);
        }

        private CodegenField AddFieldUnsharedInternal(
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            var memberNumber = _currentMemberNumber++;
            var name = CodegenPackageScopeNames.AnyField(memberNumber);
            var member = new CodegenField(FieldsClassNameOptional, name, type, null, isFinal);
            _fieldsUnshared.Put(member, initCtorScoped);
            return member;
        }

        public CodegenField AddSubstitutionParameter(
            string name,
            Type type)
        {
            var mixed = false;
            if (name == null) {
                if (!_substitutionParamsByName.IsEmpty()) {
                    mixed = true;
                }
            }
            else if (!SubstitutionParamsByNumber.IsEmpty()) {
                mixed = true;
            }

            if (mixed) {
                throw new ArgumentException("Mixing named and unnamed substitution parameters is not allowed");
            }

            if (name != null) {
                var entry = _substitutionParamsByName.Get(name);
                if (entry != null && !TypeHelper.IsSubclassOrImplementsInterface(type, entry.Type)) {
                    throw new ArgumentException(
                        "Substitution parameter '" +
                        name +
                        "' of type '" +
                        entry.Type +
                        "' cannot be assigned type '" +
                        type +
                        "'");
                }
            }

            CodegenField member;
            if (name == null) {
                var assigned = ++_currentSubstitutionParamNumber;
                var fieldName = CodegenPackageScopeNames.AnySubstitutionParam(assigned);
                member = new CodegenField(FieldsClassNameOptional, fieldName, type, null, false);
                SubstitutionParamsByNumber.Add(new CodegenSubstitutionParamEntry(member, name, type));
            }
            else {
                var existing = _substitutionParamsByName.Get(name);
                if (existing == null) {
                    var assigned = ++_currentSubstitutionParamNumber;
                    var fieldName = CodegenPackageScopeNames.AnySubstitutionParam(assigned);
                    member = new CodegenField(FieldsClassNameOptional, fieldName, type, null, false);
                    _substitutionParamsByName.Put(name, new CodegenSubstitutionParamEntry(member, name, type));
                }
                else {
                    member = existing.Field;
                }
            }

            return member;
        }
    }
} // end of namespace