///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public partial class StmtClassForgeableStmtFields : StmtClassForgeable
    {
        private readonly string _className;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly bool _dataflowOperatorFields;

        public StmtClassForgeableStmtFields(
            string className,
            CodegenNamespaceScope namespaceScope)
            : this(className, namespaceScope, false)
        {
        }

        public StmtClassForgeableStmtFields(
            string className,
            CodegenNamespaceScope namespaceScope,
            bool dataflowOperatorFields)
        {
            _className = className;
            _namespaceScope = namespaceScope;
            _dataflowOperatorFields = dataflowOperatorFields;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            // This code can cause the statementFields to not be generated and this
            // causes problems elsewhere.  Instead, let the class get generated because
            // we must pass the instance to other classes. - AJ
#if DISABLED
            if (!_dataflowOperatorFields && !_namespaceScope.HasAnyFields) {
                return null;
            }
#endif

            var memberFields = Members;
            var maxMembersPerClass = Math.Max(1, _namespaceScope.Config.InternalUseOnlyMaxMembersPerClass);

            IList<CodegenInnerClass> innerClasses = EmptyList<CodegenInnerClass>.Instance;
            IList<CodegenTypedParam> members;
            if (memberFields.Count <= maxMembersPerClass) {
                members = ToMembers(memberFields);
            }
            else {
                var assignments = CollectionUtil.Subdivide(memberFields, maxMembersPerClass);
                innerClasses = new List<CodegenInnerClass>(assignments.Count);
                members = MakeInnerClasses(assignments, innerClasses);
            }

            // Add a reference to "self"
            members.Add(new CodegenTypedParam(_className, null, "statementFields", false, false));
            
            // ctor
            var ctor = new CodegenCtor(GetType(), includeDebugSymbols, EmptyList<CodegenTypedParam>.Instance);
            ctor.Block.AssignRef(Ref("statementFields"), Ref("this"));
            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);

            // init method
            var initMethod = _namespaceScope.InitMethod;
            new CodegenRepetitiveValueBuilder<KeyValuePair<CodegenField, CodegenExpression>>(
                    _namespaceScope.FieldsUnshared,
                    initMethod,
                    classScope,
                    GetType())
                .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref)
                .SetConsumer(
                    (
                        entry,
                        index,
                        leaf) => leaf.Block.AssignRef(entry.Key.NameWithMember, entry.Value))
                .Build();

            // build methods
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(initMethod, "Init", methods, properties);

            // assignment methods
            if (_namespaceScope.HasAssignableStatementFields) {
                var assignMethod =
                    CodegenMethod
                        .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                        .AddParam<StatementAIFactoryAssignments>("assignments")
                        .WithStatic(false);
                var unassignMethod = CodegenMethod
                    .MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                    .WithStatic(false);
                GenerateAssignAndUnassign(assignMethod, unassignMethod, _namespaceScope.FieldsNamed);
                CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", methods, properties);
                CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "Unassign", methods, properties);
            }

            return new CodegenClass(
                CodegenClassType.STATEMENTFIELDS,
                typeof(StatementFields),
                _className,
                classScope,
                members,
                ctor,
                methods,
                properties,
                innerClasses);
        }

        private IList<CodegenTypedParam> MakeInnerClasses(
            IList<IList<MemberFieldPair>> assignments,
            IList<CodegenInnerClass> innerClasses)
        {
            var indexAssignment = 0;
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>(assignments.Count);

            foreach (var assignment in assignments) {
                var classNameAssignment = "A" + indexAssignment;
                var memberNameAssignment = "a" + indexAssignment;

                // set assigned member name
                IList<CodegenTypedParam> assignmentMembers = new List<CodegenTypedParam>(assignment.Count);
                foreach (var memberField in assignment) {
                    assignmentMembers.Add(memberField.Member);
                    memberField.Field.AssignmentMemberName = memberNameAssignment;
                }

                // add inner class
                var innerClass = new CodegenInnerClass(
                    classNameAssignment,
                    null,
                    assignmentMembers,
                    new CodegenClassMethods(),
                    new CodegenClassProperties());
                innerClasses.Add(innerClass);

                // initialize member
                var member = new CodegenTypedParam(innerClass.ClassName, memberNameAssignment)
                    .WithStatic(false)
                    .WithInitializer(NewInstanceInner(innerClass.ClassName));
                members.Add(member);

                indexAssignment++;
            }

            return members;
        }

        private IList<CodegenTypedParam> ToMembers(IList<MemberFieldPair> memberFields)
        {
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>(memberFields.Count);
            foreach (var memberField in memberFields) {
                members.Add(memberField.Member);
            }

            return members;
        }

        private IList<MemberFieldPair> Members {
            get {
                // members
                IList<MemberFieldPair> members = new List<MemberFieldPair>();

                GenerateNamedMembers(members);

                // numbered members
                foreach (var entry in _namespaceScope.FieldsUnshared) {
                    var field = entry.Key;
                    var member = new CodegenTypedParam(field.Type, field.Name)
                        .WithStatic(false)
                        .WithFinal(false);
                    members.Add(new MemberFieldPair(member, field));
                }

                // substitution-parameter members
                GenerateSubstitutionParamMembers(members);

                return members;
            }
        }

        private void GenerateSubstitutionParamMembers(IList<MemberFieldPair> members)
        {
            var numbered = _namespaceScope.SubstitutionParamsByNumber;
            var named = _namespaceScope.SubstitutionParamsByName;

            if (numbered.IsEmpty() && named.IsEmpty()) {
                return;
            }

            if (!numbered.IsEmpty() && !named.IsEmpty()) {
                throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
            }

            IList<CodegenSubstitutionParamEntry> fields;
            if (!numbered.IsEmpty()) {
                fields = numbered;
            }
            else {
                fields = new List<CodegenSubstitutionParamEntry>(named.Values);
            }

            for (var i = 0; i < fields.Count; i++) {
                var field = fields[i].Field;
                var name = field.Name;
                var member = new CodegenTypedParam(fields[i].EntryType, name)
                    .WithStatic(false)
                    .WithFinal(false);
                members.Add(new MemberFieldPair(member, field));
            }
        }

        private void GenerateNamedMembers(IList<MemberFieldPair> fields)
        {
            foreach (var entry in _namespaceScope.FieldsNamed) {
                var member = new CodegenTypedParam(entry.Value.Type, entry.Key.Name)
                    .WithFinal(false)
                    .WithStatic(false);
                fields.Add(new MemberFieldPair(member, entry.Value));
            }
        }

        private static void GenerateAssignAndUnassign(
            CodegenMethod assign,
            CodegenMethod unassign,
            IDictionary<CodegenFieldName, CodegenField> names)
        {
            foreach (var entry in names) {
                var name = entry.Key;
                var field = entry.Value;
                if (name is CodegenFieldNameAgg) {
                    Generate(
                        ExprDotName(Ref("assignments"), "AggregationResultFuture"),
                        field,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNamePrevious previous) {
                    Generate(
                        ArrayAtIndex(
                            ExprDotName(Ref("assignments"), "PreviousStrategies"),
                            Constant(previous.StreamNumber)),
                        field,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNamePrior namePrior) {
                    Generate(
                        ArrayAtIndex(
                            ExprDotName(Ref("assignments"), "PriorStrategies"),
                            Constant(namePrior.StreamNumber)),
                        field,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNameViewAgg) {
                    Generate(
                        ConstantNull(),
                        field,
                        assign,
                        unassign,
                        true); // we assign null as the view can assign a value
                    continue;
                }

                if (name is CodegenFieldNameSubqueryResult result) {
                    var subqueryLookupStrategy = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryLookup",
                        Constant(result.SubqueryNumber));
                    Generate(subqueryLookupStrategy, field, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrior subqueryPrior) {
                    var prior = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryPrior",
                        Constant(subqueryPrior.SubqueryNumber));
                    Generate(prior, field, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrevious subqueryPrevious) {
                    var prev = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryPrevious",
                        Constant(subqueryPrevious.SubqueryNumber));
                    Generate(prev, field, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryAgg subq) {
                    var agg = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryAggregation",
                        Constant(subq.SubqueryNumber));
                    Generate(agg, field, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameTableAccess tableAccess) {
                    var tableAccessLookupStrategy = ExprDotMethod(
                        Ref("assignments"),
                        "GetTableAccess",
                        Constant(tableAccess.TableAccessNumber));
                    // Table strategies don't get unassigned as they don't hold on to table instance
                    Generate(tableAccessLookupStrategy, field, assign, unassign, false);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizePrevious) {
                    Generate(
                        ExprDotName(Ref("assignments"), "RowRecogPreviousStrategy"),
                        field,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizeAgg) {
                    Generate(
                        ConstantNull(),
                        field,
                        assign,
                        unassign,
                        true); // we assign null as the view can assign a value
                    continue;
                }

                throw new IllegalStateException("Unrecognized field " + entry.Key);
            }
        }

        public string ClassName => _className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.FIELDS;

        public static void MakeSubstitutionSetter(
            CodegenNamespaceScope namespaceScope,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            MakeSubstitutionSetter(
                namespaceScope,
                method,
                method.Block,
                classScope);
        }

        public static void MakeSubstitutionSetter(
            CodegenNamespaceScope namespaceScope,
            CodegenMethodScope methodScope,
            CodegenBlock enclosingBlock,
            CodegenClassScope classScope)
        {
            // var assignMethod = CodegenMethod
            //     .MakeParentNode(typeof(void), typeof(StmtClassForgeableStmtFields), classScope)
            //     .AddParam<StatementAIFactoryAssignments>("assignments");
            // assignerSetterClass.AddMethod("assign", assignMethod);
            var assignLambda = new CodegenExpressionLambda(enclosingBlock)
                .WithParam<StatementAIFactoryAssignments>("assignments");
            if (!namespaceScope.FieldsNamed.IsEmpty()) {
                assignLambda.Block.ExprDotMethod(Ref("statementFields"), "Assign", Ref("assignments"));
            }

            // var setValueMethod = CodegenMethod
            //     .MakeParentNode(typeof(void), typeof(StmtClassForgeableStmtFields), classScope)
            //     .AddParam<int>("index")
            //     .AddParam<object>("value");
            // assignerSetterClass.AddMethod("setValue", setValueMethod);
            var setValueLambda = new CodegenExpressionLambda(enclosingBlock)
                .WithParam(typeof(int), "index")
                .WithParam(typeof(object), "value");
            
            // var assignerSetterClass = NewAnonymousClass(method.Block, typeof(FAFQueryMethodAssignerSetter));
            // method.Block.MethodReturn(assignerSetterClass);
            var assignerSetterClass = NewInstance<ProxyFAFQueryMethodAssignerSetter>(
                assignLambda, setValueLambda);
            enclosingBlock.ReturnMethodOrBlock(assignerSetterClass);
            
            // CodegenSubstitutionParamEntry.CodegenSetterMethod(classScope, setValueLambda);
            CodegenSubstitutionParamEntry.CodegenSetterBody(
                classScope, methodScope, setValueLambda.Block, Ref("statementFields"));
        }

        private static void Generate(
            CodegenExpression init,
            CodegenField field,
            CodegenMethod assign,
            CodegenMethod unassign,
            bool generateUnassign)
        {
            assign.Block.AssignRef(field.NameWithMember, init);

            // Table strategies are not unassigned since they do not hold on to the table instance
            if (generateUnassign) {
                unassign.Block.AssignRef(field.NameWithMember, ConstantNull());
            }
        }
    }
} // end of namespace