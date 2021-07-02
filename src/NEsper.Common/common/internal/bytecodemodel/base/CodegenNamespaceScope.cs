///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
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
            string fieldsClassName,
            bool instrumented)
        {
            Namespace = @namespace;
            FieldsClassName = fieldsClassName;
            IsInstrumented = instrumented;
            InitMethod = CodegenMethod
                .MakeMethod(
                    typeof(void),
                    typeof(CodegenNamespaceScope),
                    new CodegenClassScope(true, this, null))
                .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref)
                .WithStatic(false);
        }

        public string Namespace { get; }

        public CodegenMethod InitMethod { get; }

        public IDictionary<CodegenFieldName, CodegenField> FieldsNamed => _fieldsNamed;

        public IDictionary<CodegenField, CodegenExpression> FieldsUnshared => _fieldsUnshared;

        public string FieldsClassName { get; }

        public IList<CodegenSubstitutionParamEntry> SubstitutionParamsByNumber { get; } =
            new List<CodegenSubstitutionParamEntry>();

        public IDictionary<string, CodegenSubstitutionParamEntry> SubstitutionParamsByName => _substitutionParamsByName;

        public bool IsInstrumented { get; }

        public bool HasStatementFields => !_fieldsNamed.IsEmpty();

        public bool HasSubstitution => !SubstitutionParamsByNumber.IsEmpty() || !SubstitutionParamsByName.IsEmpty();

        public CodegenExpressionInstanceField AddInstanceFieldUnshared<T>(
            CodegenExpression instance,
            bool isFinal,
            CodegenExpression initCtorScoped)
        {
            return AddInstanceFieldUnshared(instance, isFinal, typeof(T), initCtorScoped);
        }

        public CodegenExpressionInstanceField AddInstanceFieldUnshared(
            CodegenExpression instance,
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            if (FieldsClassName == null)
            {
                throw new IllegalStateException("No fields class name");
            }

            CodegenField unshared = AddFieldUnsharedInternal(isFinal, type, initCtorScoped);
            return InstanceField(instance, unshared);
        }

        public CodegenExpressionInstanceField AddDefaultFieldUnshared(
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            return AddInstanceFieldUnshared(
                Ref("statementFields"),
                isFinal,
                type,
                initCtorScoped);
        }

        public CodegenExpressionInstanceField AddOrGetInstanceFieldSharable(
            CodegenExpression instance,
            CodegenFieldSharable sharable)
        {
            CodegenExpressionField fieldExpression = AddOrGetFieldSharable(sharable);
            return InstanceField(instance, fieldExpression.Field);
        }

        public CodegenExpressionInstanceField AddOrGetDefaultFieldSharable(
            CodegenFieldSharable sharable)
        {
            CodegenExpression instance = Ref("statementFields");
            CodegenExpressionField fieldExpression = AddOrGetFieldSharable(sharable);
            return InstanceField(instance, fieldExpression.Field);
        }

        public CodegenExpressionField AddOrGetFieldSharable(CodegenFieldSharable sharable)
        {
            return AddOrGetFieldSharableInternal(sharable);
        }

        private CodegenExpressionField AddOrGetFieldSharableInternal(CodegenFieldSharable sharable)
        {
            var member = _fieldsShared.Get(sharable);
            if (member != null) {
                return Field(member);
            }

            member = AddFieldUnsharedInternal(true, sharable.Type(), sharable.InitCtorScoped());
            _fieldsShared.Put(sharable, member);
            return Field(member);
        }

        // --------------------------------------------------------------------------------

        public CodegenExpressionInstanceField AddOrGetDefaultFieldWellKnown(
            CodegenFieldName fieldName,
            Type type)
        {
            CodegenExpression instance = Ref("statementFields");
            CodegenExpressionField fieldExpression = AddOrGetFieldWellKnown(fieldName, type);
            return InstanceField(instance, fieldExpression.Field);
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

            var field = new CodegenField(FieldsClassName, fieldName.Name, type, false);
            _fieldsNamed.Put(fieldName, field);
            return Field(field);
        }

        // --------------------------------------------------------------------------------

        private CodegenField AddFieldUnsharedInternal(
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            var memberNumber = _currentMemberNumber++;
            var name = CodegenNamespaceScopeNames.AnyField(memberNumber);
            var member = new CodegenField(FieldsClassName, name, type, isFinal);
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
                var fieldName = CodegenNamespaceScopeNames.AnySubstitutionParam(assigned);
                member = new CodegenField(FieldsClassName, fieldName, type, false);
                SubstitutionParamsByNumber.Add(new CodegenSubstitutionParamEntry(member, name, type));
            }
            else {
                var existing = _substitutionParamsByName.Get(name);
                if (existing == null) {
                    var assigned = ++_currentSubstitutionParamNumber;
                    var fieldName = CodegenNamespaceScopeNames.AnySubstitutionParam(assigned);
                    member = new CodegenField(FieldsClassName, fieldName, type, false);
                    _substitutionParamsByName.Put(name, new CodegenSubstitutionParamEntry(member, name, type));
                }
                else {
                    member = existing.Field;
                }
            }

            return member;
        }

        public override string ToString()
        {
            return $"{nameof(Namespace)}: {Namespace}";
        }
    }
} // end of namespace