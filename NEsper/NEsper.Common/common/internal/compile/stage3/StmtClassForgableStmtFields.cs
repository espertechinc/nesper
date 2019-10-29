///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtClassForgableStmtFields : StmtClassForgable
    {
        private static int _gid;
        
        private readonly int _id = ++_gid;
        private readonly string _className;
        private readonly int _numStreams;
        private readonly CodegenNamespaceScope _namespaceScope;

        public StmtClassForgableStmtFields(
            string className,
            CodegenNamespaceScope namespaceScope,
            int numStreams)
        {
            _className = className;
            _namespaceScope = namespaceScope;
            _numStreams = numStreams;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            // members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();

            // Add a reference to "self"
            members.Add(new CodegenTypedParam(_className, null, "statementFields", false, false));

            GenerateNamedMembers(members);

            // numbered members
            foreach (var entry in _namespaceScope.FieldsUnshared) {
                var field = entry.Key;
                members.Add(
                    new CodegenTypedParam(field.Type, field.Name)
                        .WithStatic(false)
                        .WithFinal(false));
            }

            // substitution-parameter members
            GenerateSubstitutionParamMembers(members);

            var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, ClassName);

            // ctor
            var ctor = new CodegenCtor(GetType(), ClassName, includeDebugSymbols, Collections.GetEmptyList<CodegenTypedParam>());
            ctor.Block.AssignRef(Ref("statementFields"), Ref("this"));

            //_namespaceScope
            var initMethod = _namespaceScope.InitMethod;
            foreach (var entry in _namespaceScope.FieldsUnshared) {
                initMethod.Block.AssignRef(entry.Key.Name, entry.Value);
            }

            // assignment methods
            CodegenMethod assignMethod = CodegenMethod
                .MakeMethod(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(StatementAIFactoryAssignments), "assignments")
                .WithStatic(false);
            CodegenMethod unassignMethod = CodegenMethod
                .MakeMethod(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .WithStatic(false);
            GenerateAssignAndUnassign(_numStreams, assignMethod, unassignMethod, _namespaceScope.FieldsNamed);

            // build methods
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();

            CodegenStackGenerator.RecursiveBuildStack(initMethod, "Init", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "Assign", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "Unassign", methods, properties);

            return new CodegenClass(
                typeof(StatementFields),
                _namespaceScope.Namespace,
                ClassName,
                classScope,
                members,
                ctor,
                methods,
                properties,
                Collections.GetEmptyList<CodegenInnerClass>());
        }

        public string ClassName => _className;

        public StmtClassForgableType ForgableType => StmtClassForgableType.FIELDS;

        private void GenerateSubstitutionParamMembers(IList<CodegenTypedParam> members)
        {
            IList<CodegenSubstitutionParamEntry> numbered = _namespaceScope.SubstitutionParamsByNumber;
            IDictionary<string, CodegenSubstitutionParamEntry> named = _namespaceScope.SubstitutionParamsByName;

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
                string name = fields[i].Field.Name;
                members.Add(new CodegenTypedParam(fields[i].Type, name).WithStatic(false).WithFinal(true));
            }
        }

        private void GenerateNamedMembers(IList<CodegenTypedParam> fields)
        {
            foreach (KeyValuePair<CodegenFieldName, CodegenField> entry in _namespaceScope.FieldsNamed) {
                fields.Add(new CodegenTypedParam(entry.Value.Type, entry.Key.Name)
                    .WithFinal(false)
                    .WithStatic(false));
            }
        }

        private static void GenerateAssignAndUnassign(
            int numStreams,
            CodegenMethod assign,
            CodegenMethod unassign,
            IDictionary<CodegenFieldName, CodegenField> names)
        {
            foreach (KeyValuePair<CodegenFieldName, CodegenField> entry in names) {
                var name = entry.Key;
                if (name is CodegenFieldNameAgg) {
                    Generate(
                        ExprDotName(Ref("assignments"), "AggregationResultFuture"),
                        name,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNamePrevious codegenFieldNamePrevious) {
                    Generate(
                        ArrayAtIndex(
                            ExprDotName(Ref("assignments"), "PreviousStrategies"),
                            Constant(codegenFieldNamePrevious.StreamNumber)),
                        codegenFieldNamePrevious,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNamePrior codegenFieldNamePrior) {
                    Generate(
                        ArrayAtIndex(
                            ExprDotName(Ref("assignments"), "PriorStrategies"),
                            Constant(codegenFieldNamePrior.StreamNumber)),
                        codegenFieldNamePrior,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNameViewAgg) {
                    Generate(
                        ConstantNull(),
                        name,
                        assign,
                        unassign,
                        true); // we assign null as the view can assign a value
                    continue;
                }

                if (name is CodegenFieldNameSubqueryResult codegenFieldNameSubqueryResult) {
                    var subqueryLookupStrategy = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryLookup",
                        Constant(codegenFieldNameSubqueryResult.SubqueryNumber));
                    Generate(subqueryLookupStrategy, codegenFieldNameSubqueryResult, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrior codegenFieldNameSubqueryPrior) {
                    var prior = ExprDotMethod(Ref("assignments"), "GetSubqueryPrior", Constant(codegenFieldNameSubqueryPrior.SubqueryNumber));
                    Generate(prior, codegenFieldNameSubqueryPrior, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrevious codegenFieldNameSubqueryPrevious) {
                    var prev = ExprDotMethod(Ref("assignments"), "GetSubqueryPrevious", Constant(codegenFieldNameSubqueryPrevious.SubqueryNumber));
                    Generate(prev, codegenFieldNameSubqueryPrevious, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryAgg codegenFieldNameSubqueryAgg) {
                    var agg = ExprDotMethod(
                        Ref("assignments"),
                        "GetSubqueryAggregation",
                        Constant(codegenFieldNameSubqueryAgg.SubqueryNumber));
                    Generate(agg, codegenFieldNameSubqueryAgg, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameTableAccess tableAccess) {
                    var tableAccessLookupStrategy = ExprDotMethod(
                        Ref("assignments"),
                        "GetTableAccess",
                        Constant(tableAccess.TableAccessNumber));
                    // Table strategies don't get unassigned as they don't hold on to table instance
                    Generate(tableAccessLookupStrategy, tableAccess, assign, unassign, false);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizePrevious) {
                    Generate(
                        ExprDotName(Ref("assignments"), "RowRecogPreviousStrategy"),
                        name,
                        assign,
                        unassign,
                        true);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizeAgg) {
                    Generate(
                        ConstantNull(),
                        name,
                        assign,
                        unassign,
                        true); // we assign null as the view can assign a value
                    continue;
                }

                throw new IllegalStateException("Unrecognized field " + entry.Key);
            }
        }

        public static void MakeSubstitutionSetter(
            CodegenNamespaceScope packageScope,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            MakeSubstitutionSetter(
                packageScope,
                method.Block,
                classScope);
        }

        public static void MakeSubstitutionSetter(
            CodegenNamespaceScope packageScope,
            CodegenBlock enclosingBlock,
            CodegenClassScope classScope)
        {
            var assignLambda = new CodegenExpressionLambda(enclosingBlock)
                .WithParam<StatementAIFactoryAssignments>("assignments");

            //var assignMethod = CodegenMethod
            //    .MakeParentNode(typeof(void), typeof(StmtClassForgableStmtFields), classScope)
            //    .AddParam(typeof(StatementAIFactoryAssignments), "assignments");
            //assignerSetterClass.AddMethod("Assign", assignMethod);

            assignLambda.Block.ExprDotMethod(Ref("statementFields"), "Assign", @Ref("assignments"));
            //assignMethod.Block.StaticMethod(packageScope.FieldsClassName, "Assign", Ref("assignments"));

            var setValueMethod = new CodegenExpressionLambda(enclosingBlock)
                .WithParam(typeof(int), "index")
                .WithParam(typeof(object), "value");
            //assignerSetterClass.AddMethod("SetValue", setValueMethod);

            var assignerSetterClass = NewInstance<ProxyFAFQueryMethodAssignerSetter>(
                assignLambda, setValueMethod);

            //var assignerSetterClass = NewAnonymousClass(enclosingBlock, typeof(FAFQueryMethodAssignerSetter));
            enclosingBlock.ReturnMethodOrBlock(assignerSetterClass);

            CodegenSubstitutionParamEntry.CodegenSetterBody(
                classScope, setValueMethod.Block, Ref("statementFields"));
        }

        private static void Generate(
            CodegenExpression init,
            CodegenFieldName name,
            CodegenMethod assign,
            CodegenMethod unassign,
            bool generateUnassign)
        {
            assign.Block.AssignRef(name.Name, init);

            // Table strategies are not unassigned since they do not hold on to the table instance
            if (generateUnassign) {
                unassign.Block.AssignRef(name.Name, ConstantNull());
            }
        }
    }
} // end of namespace