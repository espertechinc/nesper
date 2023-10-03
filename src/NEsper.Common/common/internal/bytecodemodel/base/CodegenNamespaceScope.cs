///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
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
            bool instrumented,
            ConfigurationCompilerByteCode config)
        {
            Namespace = @namespace;
            FieldsClassNameOptional = fieldsClassNameOptional;
            IsInstrumented = instrumented;
            InitMethod = CodegenMethod
                .MakeMethod(
                    typeof(void),
                    typeof(CodegenNamespaceScope),
                    new CodegenClassScope(true, this, null))
                .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref)
                .WithStatic(false);
            Config = config;
        }

        public string Namespace { get; }

        public CodegenMethod InitMethod { get; }

        public IDictionary<CodegenFieldName, CodegenField> FieldsNamed => _fieldsNamed;

        public IDictionary<CodegenField, CodegenExpression> FieldsUnshared => _fieldsUnshared;

        public string FieldsClassNameOptional { get; }
        
        public bool HasAssignableStatementFields => !_fieldsNamed.IsEmpty();

        public bool HasAnyFields =>
            !FieldsNamed.IsEmpty() ||
            !FieldsUnshared.IsEmpty() ||
            !SubstitutionParamsByNumber.IsEmpty() ||
            !SubstitutionParamsByName.IsEmpty();

        public IList<CodegenSubstitutionParamEntry> SubstitutionParamsByNumber { get; } =
            new List<CodegenSubstitutionParamEntry>();

        public IDictionary<string, CodegenSubstitutionParamEntry> SubstitutionParamsByName => _substitutionParamsByName;

        public bool IsInstrumented { get; }

        public ConfigurationCompilerByteCode Config { get; }
        
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
            if (FieldsClassNameOptional == null) {
                throw new IllegalStateException("No fields class name");
            }

            var unshared = AddFieldUnsharedInternal(isFinal, type, initCtorScoped);
            return InstanceField(instance, unshared);
        }

#if DEPRECATED
        public CodegenExpressionField AddFieldUnshared<T>(
            bool isFinal,
            CodegenExpression initCtorScoped)
        {
            return AddFieldUnshared(isFinal, typeof(T), initCtorScoped);
        }
#endif

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

#if DEPRECATED
        public CodegenExpressionField AddFieldUnshared(
            bool isFinal,
            Type type,
            CodegenExpression initCtorScoped)
        {
            if (FieldsClassName == null) {
                throw new IllegalStateException("No fields class name");
            }

            return Field(AddFieldUnsharedInternal(isFinal, type, initCtorScoped));
        }
#endif

        public CodegenExpressionInstanceField AddOrGetInstanceFieldSharable(
            CodegenExpression instance,
            CodegenFieldSharable sharable)
        {
            var fieldExpression = AddOrGetFieldSharable(sharable);
            return InstanceField(instance, fieldExpression.Field);
        }

        public CodegenExpressionInstanceField AddOrGetDefaultFieldSharable(
            CodegenFieldSharable sharable)
        {
            CodegenExpression instance = Ref("statementFields");
            var fieldExpression = AddOrGetFieldSharable(sharable);
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
            var fieldExpression = AddOrGetFieldWellKnown(fieldName, type);
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

            var field = new CodegenField(FieldsClassNameOptional, fieldName.Name, type, false);
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
            var member = new CodegenField(FieldsClassNameOptional, name, type, isFinal);
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
                if (entry != null && !TypeHelper.IsSubclassOrImplementsInterface(type, entry.EntryType)) {
                    throw new ArgumentException(
                        "Substitution parameter '" +
                        name +
                        "' of type '" +
                        entry.EntryType +
                        "' cannot be assigned type '" +
                        type +
                        "'");
                }
            }

            CodegenField member;
            if (name == null) {
                var assigned = ++_currentSubstitutionParamNumber;
                var fieldName = CodegenNamespaceScopeNames.AnySubstitutionParam(assigned);
                member = new CodegenField(FieldsClassNameOptional, fieldName, type, false);
                SubstitutionParamsByNumber.Add(new CodegenSubstitutionParamEntry(member, name, type));
            }
            else {
                var existing = _substitutionParamsByName.Get(name);
                if (existing == null) {
                    var assigned = ++_currentSubstitutionParamNumber;
                    var fieldName = CodegenNamespaceScopeNames.AnySubstitutionParam(assigned);
                    member = new CodegenField(FieldsClassNameOptional, fieldName, type, false);
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

        public void RewriteStatementFieldUse(IList<CodegenClass> classes)
        {
            if (FieldsClassNameOptional != null && !HasAnyFields) {
                RewriteProviderNoFieldInit(classes, FieldsClassNameOptional);
            }
        }

        private static void RewriteProviderNoFieldInit(
            IList<CodegenClass> classes,
            string fieldClassName)
        {
            // Rewrite the constructor of providers to remove calls to field initialization, for when there is no fields-class.
            // Field initialization cannot be predicted as forging adds fields.
            // The forge order puts the forging of the fields-class last so that fields can be added during forging.
            // Since the fields-class is forged last the provider classes cannot predict whether fields are required or not.
            foreach (CodegenClass clazz in classes) {
                if (clazz.ClassType == CodegenClassType.FAFQUERYMETHODPROVIDER ||
                    clazz.ClassType == CodegenClassType.STATEMENTAIFACTORYPROVIDER) {

                    var statements = clazz.OptionalCtor.Block.Statements;
                    for (var ii = 0; ii < statements.Count; ii++) {
                        var statement = statements[ii];
                        if (statement is CodegenStatementExpression expression) {
                            if (expression.Expression is CodegenExpressionStaticMethod staticMethod) {
                                if (staticMethod.TargetClassName != null &&
                                    staticMethod.TargetClassName.Equals(fieldClassName)) {
                                    statements.RemoveAt(ii--);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
} // end of namespace