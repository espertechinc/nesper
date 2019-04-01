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
        private readonly int numStreams;
        private readonly CodegenPackageScope packageScope;

        public StmtClassForgableStmtFields(
            string className,
            CodegenPackageScope packageScope,
            int numStreams)
        {
            ClassName = className;
            this.packageScope = packageScope;
            this.numStreams = numStreams;
        }

        public CodegenClass Forge(bool includeDebugSymbols)
        {
            // members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();

            GenerateNamedMembers(members);

            // numbered members
            foreach (KeyValuePair<CodegenField, CodegenExpression> entry in packageScope.FieldsUnshared) {
                var field = entry.Key;
                members.Add(
                    new CodegenTypedParam(field.Type, field.Name)
                        .WithStatic(true)
                        .WithFinal(false));
            }

            // substitution-parameter members
            GenerateSubstitutionParamMembers(members);

            // ctor
            var ctor = new CodegenCtor(GetType(), includeDebugSymbols, Collections.GetEmptyList<CodegenTypedParam>());
            var classScope = new CodegenClassScope(includeDebugSymbols, packageScope, ClassName);

            // init method
            var initMethod = packageScope.InitMethod;
            foreach (KeyValuePair<CodegenField, CodegenExpression> entry in packageScope.FieldsUnshared) {
                initMethod.Block.AssignRef(entry.Key.Name, entry.Value);
            }

            // assignment methods
            CodegenMethod assignMethod =
                CodegenMethod.MakeParentNode(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                    .AddParam(typeof(StatementAIFactoryAssignments), "assignments").WithStatic(true);
            CodegenMethod unassignMethod = CodegenMethod.MakeParentNode(
                typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope).WithStatic(true);
            GenerateAssignAndUnassign(numStreams, assignMethod, unassignMethod, packageScope.FieldsNamed);

            // build methods
            var methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(initMethod, "init", methods);
            CodegenStackGenerator.RecursiveBuildStack(assignMethod, "assign", methods);
            CodegenStackGenerator.RecursiveBuildStack(unassignMethod, "unassign", methods);

            return new CodegenClass(
                typeof(StatementFields), packageScope.PackageName, ClassName, classScope, members, ctor, methods,
                Collections.GetEmptyList<CodegenInnerClass>());
        }

        public string ClassName { get; }

        public StmtClassForgableType ForgableType => StmtClassForgableType.FIELDS;

        private void GenerateSubstitutionParamMembers(IList<CodegenTypedParam> members)
        {
            IList<CodegenSubstitutionParamEntry> numbered = packageScope.SubstitutionParamsByNumber;
            IDictionary<string, CodegenSubstitutionParamEntry> named = packageScope.SubstitutionParamsByName;

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
                members.Add(new CodegenTypedParam(fields[i].Type, name).WithStatic(true).WithFinal(false));
            }
        }

        private void GenerateNamedMembers(IList<CodegenTypedParam> fields)
        {
            foreach (KeyValuePair<CodegenFieldName, CodegenField> entry in packageScope.FieldsNamed) {
                fields.Add(new CodegenTypedParam(entry.Value.Type, entry.Key.Name).WithFinal(false).WithStatic(true));
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
                        ExprDotMethod(Ref("assignments"), "getAggregationResultFuture"), name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNamePrevious) {
                    var previous = (CodegenFieldNamePrevious) name;
                    Generate(
                        ArrayAtIndex(
                            ExprDotMethod(Ref("assignments"), "getPreviousStrategies"),
                            Constant(previous.StreamNumber)), name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNamePrior) {
                    var prior = (CodegenFieldNamePrior) name;
                    Generate(
                        ArrayAtIndex(
                            ExprDotMethod(Ref("assignments"), "getPriorStrategies"), Constant(prior.StreamNumber)),
                        name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameViewAgg) {
                    Generate(
                        ConstantNull(), name, assign, unassign, true); // we assign null as the view can assign a value
                    continue;
                }

                if (name is CodegenFieldNameSubqueryResult) {
                    var subq = (CodegenFieldNameSubqueryResult) name;
                    var subqueryLookupStrategy = ExprDotMethod(
                        Ref("assignments"), "getSubqueryLookup", Constant(subq.SubqueryNumber));
                    Generate(subqueryLookupStrategy, name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrior) {
                    var subq = (CodegenFieldNameSubqueryPrior) name;
                    var prior = ExprDotMethod(Ref("assignments"), "getSubqueryPrior", Constant(subq.SubqueryNumber));
                    Generate(prior, name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryPrevious) {
                    var subq = (CodegenFieldNameSubqueryPrevious) name;
                    var prev = ExprDotMethod(Ref("assignments"), "getSubqueryPrevious", Constant(subq.SubqueryNumber));
                    Generate(prev, name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameSubqueryAgg) {
                    var subq = (CodegenFieldNameSubqueryAgg) name;
                    var agg = ExprDotMethod(
                        Ref("assignments"), "getSubqueryAggregation", Constant(subq.SubqueryNumber));
                    Generate(agg, name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameTableAccess) {
                    var tableAccess = (CodegenFieldNameTableAccess) name;
                    var tableAccessLookupStrategy = ExprDotMethod(
                        Ref("assignments"), "getTableAccess", Constant(tableAccess.TableAccessNumber));
                    // Table strategies don't get unassigned as they don't hold on to table instance
                    Generate(tableAccessLookupStrategy, name, assign, unassign, false);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizePrevious) {
                    Generate(
                        ExprDotMethod(Ref("assignments"), "getRowRecogPreviousStrategy"), name, assign, unassign, true);
                    continue;
                }

                if (name is CodegenFieldNameMatchRecognizeAgg) {
                    Generate(
                        ConstantNull(), name, assign, unassign, true); // we assign null as the view can assign a value
                    continue;
                }

                throw new IllegalStateException("Unrecognized field " + entry.Key);
            }
        }

        public static void MakeSubstitutionSetter(
            CodegenPackageScope packageScope,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var assignerSetterClass = NewAnonymousClass(method.Block, typeof(FAFQueryMethodAssignerSetter));
            method.Block.MethodReturn(assignerSetterClass);

            var assignMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(StmtClassForgableStmtFields), classScope).AddParam(
                    typeof(StatementAIFactoryAssignments), "assignments");
            assignerSetterClass.AddMethod("assign", assignMethod);
            assignMethod.Block.StaticMethod(packageScope.FieldsClassNameOptional, "assign", Ref("assignments"));

            var setValueMethod = CodegenMethod
                .MakeParentNode(typeof(void), typeof(StmtClassForgableStmtFields), classScope)
                .AddParam(typeof(int), "index").AddParam(typeof(object), "value");
            assignerSetterClass.AddMethod("setValue", setValueMethod);
            CodegenSubstitutionParamEntry.CodegenSetterMethod(classScope, setValueMethod);
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