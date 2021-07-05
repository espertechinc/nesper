///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.createclass;
using com.espertech.esper.common.@internal.context.aifactory.createcontext;
using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.common.@internal.context.aifactory.createexpression;
using com.espertech.esper.common.@internal.context.aifactory.createindex;
using com.espertech.esper.common.@internal.context.aifactory.createschema;
using com.espertech.esper.common.@internal.context.aifactory.createtable;
using com.espertech.esper.common.@internal.context.aifactory.createvariable;
using com.espertech.esper.common.@internal.context.aifactory.createwindow;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;

using static com.espertech.esper.compiler.@internal.util.CompilerHelperSingleEPL;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperValidator;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperStatementProvider
    {
        public static CompilableItem CompileItem(
            Compilable compilable,
            string optionalModuleName,
            string moduleIdentPostfix,
            int statementNumber,
            ISet<string> statementNames,
            ModuleCompileTimeServices moduleCompileTimeServices,
            CompilerOptions compilerOptions,
            out Pair<Assembly, byte[]> assemblyWithImage)
        {
            var compileTimeServices = new StatementCompileTimeServices(statementNumber, moduleCompileTimeServices);

            // Stage 1 - parse and compile-inline-classes and walk statement
            var walked = ParseCompileInlinedClassesWalk(compilable, compilerOptions.InlinedClassInspection, compileTimeServices);
            var raw = walked.StatementSpecRaw;
            string classNameCreateClass = null;
            if (raw.CreateClassProvided != null) {
                classNameCreateClass = DetermineClassNameCreateClass(walked.ClassesInlined);
            }
            
            try {
                // Stage 2(a) - precompile: compile annotations
                var annotations = AnnotationUtil.CompileAnnotations(
                    raw.Annotations,
                    compileTimeServices.ImportServiceCompileTime,
                    compilable);

                // Stage 2(b) - walk subselects, alias expressions, declared expressions, dot-expressions
                ExprNodeSubselectDeclaredDotVisitor visitor;
                try {
                    visitor = StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(raw);
                }
                catch (ExprValidationException ex) {
                    throw new StatementSpecCompileException(ex.Message, compilable.ToEPL());
                }

                var subselectNodes = visitor.Subselects;

                // Determine a statement name
                var statementNameProvided = GetNameFromAnnotation(annotations);
                if (compilerOptions.StatementName != null) {
                    var assignedName = compilerOptions.StatementName.Invoke(
                        new StatementNameContext(
                            () => compilable.ToEPL(),
                            statementNameProvided,
                            optionalModuleName,
                            annotations,
                            statementNumber));
                    if (assignedName != null) {
                        statementNameProvided = assignedName;
                    }
                }

                var statementName = statementNameProvided ?? Convert.ToString(statementNumber);
                if (statementNames.Contains(statementName)) {
                    var count = 1;
                    var newStatementName = statementName + "-" + count;
                    while (statementNames.Contains(newStatementName)) {
                        count++;
                        newStatementName = statementName + "-" + count;
                    }

                    statementName = newStatementName;
                }

                statementName = statementName.Trim();
                statementNames.Add(statementName);

                // Determine table access nodes
                var tableAccessNodes = DetermineTableAccessNodes(raw.TableExpressions, visitor);

                // compile scripts once in this central place, may also compile later in expression
                ScriptValidationPrecompileUtil.ValidateScripts(
                    raw.ScriptExpressions,
                    raw.ExpressionDeclDesc,
                    compileTimeServices);

                // Determine subselects for compilation, and lambda-expression shortcut syntax for named windows
                if (!visitor.ChainedExpressionsDot.IsEmpty()) {
                    RewriteNamedWindowSubselect(
                        visitor.ChainedExpressionsDot,
                        subselectNodes,
                        compileTimeServices.NamedWindowCompileTimeResolver);
                }

                // Stage 2(c) compile context descriptor
                ContextCompileTimeDescriptor contextDescriptor = null;
                var optionalContextName = raw.OptionalContextName;
                if (optionalContextName != null) {
                    var detail = compileTimeServices.ContextCompileTimeResolver.GetContextInfo(optionalContextName);
                    if (detail == null) {
                        throw new StatementSpecCompileException(
                            "Context by name '" + optionalContextName + "' could not be found",
                            compilable.ToEPL());
                    }

                    contextDescriptor = new ContextCompileTimeDescriptor(
                        optionalContextName,
                        detail.ContextModuleName,
                        detail.ContextVisibility,
                        new ContextPropertyRegistry(detail),
                        detail.ValidationInfos);
                }

                // Stage 2(d) compile raw statement spec
                var statementType = StatementTypeUtil.GetStatementType(raw).Value;
                var statementRawInfo = new StatementRawInfo(
                    statementNumber,
                    statementName,
                    annotations,
                    statementType,
                    contextDescriptor,
                    raw.IntoTableSpec?.Name,
                    compilable,
                    optionalModuleName);
                var compiledDesc = StatementRawCompiler.Compile(
                    raw,
                    compilable,
                    false,
                    false,
                    annotations,
                    subselectNodes,
                    tableAccessNodes,
                    statementRawInfo,
                    compileTimeServices);
                var specCompiled = compiledDesc.Compiled;
                var statementIdentPostfix = IdentifierUtil.GetIdentifierMayStartNumeric(statementName);

                // get compile-time user object
                object userObjectCompileTime = null;
                if (compilerOptions.StatementUserObject != null) {
                    userObjectCompileTime = compilerOptions.StatementUserObject.Invoke(
                        new StatementUserObjectContext(
                            () => compilable.ToEPL(),
                            statementName,
                            optionalModuleName,
                            annotations,
                            statementNumber));
                }

                // handle hooks
                HandleStatementCompileHook(annotations, compileTimeServices, specCompiled);

                // Stage 3(a) - statement-type-specific forge building
                var @base = new StatementBaseInfo(
                    compilable,
                    specCompiled,
                    userObjectCompileTime,
                    statementRawInfo,
                    optionalModuleName);
                StmtForgeMethod forgeMethod;
                if (raw.UpdateDesc != null) {
                    forgeMethod = new StmtForgeMethodUpdate(@base);
                }
                else if (raw.OnTriggerDesc != null) {
                    forgeMethod = new StmtForgeMethodOnTrigger(@base);
                }
                else if (raw.CreateIndexDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateIndex(@base);
                }
                else if (raw.CreateVariableDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateVariable(@base);
                }
                else if (raw.CreateDataFlowDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateDataflow(@base);
                }
                else if (raw.CreateTableDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateTable(@base);
                }
                else if (raw.CreateExpressionDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateExpression(@base);
                }
                else if (raw.CreateClassProvided != null) {
                    forgeMethod = new StmtForgeMethodCreateClass(@base, walked.ClassesInlined, classNameCreateClass);
                }
                else if (raw.CreateWindowDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateWindow(@base);
                }
                else if (raw.CreateContextDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateContext(@base);
                }
                else if (raw.CreateSchemaDesc != null) {
                    forgeMethod = new StmtForgeMethodCreateSchema(@base);
                }
                else {
                    forgeMethod = new StmtForgeMethodSelect(@base);
                }

                // check context-validity conditions for this statement
                if (contextDescriptor != null) {
                    try {
                        foreach (var validator in contextDescriptor.ValidationInfos) {
                            validator.ValidateStatement(
                                contextDescriptor.ContextName,
                                specCompiled,
                                compileTimeServices);
                        }
                    }
                    catch (ExprValidationException ex) {
                        throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
                    }
                }

                // Stage 3(b) - forge-factory-to-forge
                var classPostfix = moduleIdentPostfix + "_" + statementIdentPostfix;
                var forgeables = new List<StmtClassForgeable>();
                
                // add forgeables from filter-related processing i.e. multikeys
                foreach (var additional in compiledDesc.AdditionalForgeables) {
                    var namespaceScope = new CodegenNamespaceScope(compileTimeServices.Namespace, null, false);
                    forgeables.Add(additional.Make(namespaceScope, classPostfix));
                }
                
                var filterSpecCompileds = new List<FilterSpecCompiled>();
                var scheduleHandleCallbackProviders = new List<ScheduleHandleCallbackProvider>();
                var namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
                var filterBooleanExpressions = new List<FilterSpecParamExprNodeForge>();
                
                var result = forgeMethod.Make(compileTimeServices.Namespace, classPostfix, compileTimeServices);
                forgeables.AddAll(result.Forgeables);
                VerifyForgeables(forgeables);
                
                filterSpecCompileds.AddAll(result.Filtereds);
                scheduleHandleCallbackProviders.AddAll(result.Scheduleds);
                namedWindowConsumers.AddAll(result.NamedWindowConsumers);
                filterBooleanExpressions.AddAll(result.FilterBooleanExpressions);

                // Stage 3(c) - filter assignments: assign filter callback ids and filter-path-num for boolean expressions
                var filterId = -1;
                foreach (var provider in filterSpecCompileds) {
                    var assigned = ++filterId;
                    provider.FilterCallbackId = assigned;
                }

                // Stage 3(d) - schedule assignments: assign schedule callback ids
                var scheduleId = 0;
                foreach (var provider in scheduleHandleCallbackProviders) {
                    provider.ScheduleCallbackId = scheduleId++;
                }

                // Stage 3(e) - named window consumers: assign consumer id
                var namedWindowConsumerId = 0;
                foreach (var provider in namedWindowConsumers) {
                    provider.NamedWindowConsumerId = namedWindowConsumerId++;
                }

                // Stage 3(f) - filter boolean expression id assignment
                var filterBooleanExprNum = 0;
                foreach (var expr in filterBooleanExpressions) {
                    expr.FilterBoolExprId = filterBooleanExprNum++;
                }

                // Stage 3(f) - verify substitution parameters
                VerifySubstitutionParams(raw.SubstitutionParameters);

                // Stage 4 - forge-to-class (forge with statement-fields last)
                var classes = forgeables
                    .Select(forgeable => forgeable.Forge(true, false))
                    .ToList();

                // Stage 5 - refactor methods to make sure the constant pool does not grow too large for any given class
                CompilerHelperRefactorToStaticMethods.RefactorMethods(
                    classes, compileTimeServices.Configuration.Compiler.ByteCode.MaxMethodsPerClass);

                // Stage 6 - sort to make the "fields" class first and all the rest later
                var sorted = classes
                    .OrderBy(c => c.ClassType.GetSortCode())
                    .ToList();

                // We are making sure JsonEventType receives the underlying class itself
                CompilableItemPostCompileLatch postCompile = CompilableItemPostCompileLatchDefault.INSTANCE;
                foreach (var eventType in compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded) {
                    if (eventType is JsonEventType) {
                        postCompile = new CompilableItemPostCompileLatchJson(
                            compileTimeServices.EventTypeCompileTimeRegistry.NewTypesAdded,
                            compileTimeServices.ParentClassLoader);
                        break;
                    }
                }

                var container = compileTimeServices.Container;
                var compiler = container
                    .RoslynCompiler()
                    .WithCodeLogging(compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
                    .WithCodeAuditDirectory(compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
                    .WithCodegenClasses(sorted);

                assemblyWithImage = compiler.Compile();

                string statementProviderClassName = CodeGenerationIDGenerator.GenerateClassNameWithNamespace(
                    compileTimeServices.Namespace,
                    typeof(StatementProvider),
                    classPostfix);

                var additionalClasses = new HashSet<Type>();
                additionalClasses.AddAll(walked.ClassesInlined.Classes);
                compileTimeServices.ClassProvidedCompileTimeResolver.AddTo(additionalClasses);
                compileTimeServices.ClassProvidedCompileTimeRegistry.AddTo(additionalClasses);

                return new CompilableItem(
                    statementProviderClassName,
                    classes,
                    postCompile,
                    additionalClasses);
            }
            catch (StatementSpecCompileException) {
                throw;
            }
            catch (ExprValidationException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }
            catch (EPException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }
            catch (Exception ex) {
                var text = ex.Message ?? ex.GetType().FullName;
                throw new StatementSpecCompileException(text, ex, compilable.ToEPL());
            }
        }

        private static string DetermineClassNameCreateClass(ClassProvidedPrecompileResult classesInlined)
        {
            string className = null;
            for (int i = classesInlined.Classes.Count - 1; i >= 0; i--) {
                var clazz = classesInlined.Classes[i];
                if (clazz.FullName.Contains("+")) { // TBD: <<-- Evaluation, converted from JVM notation to CLR
                    continue;
                }

                return clazz.FullName;
            }

            var exportedTypes = classesInlined.Assembly.GetExportedTypes()
                .Select(t => t.Name)
                .ToList();
            
            throw new IllegalStateException("Could not determine class name, entries are: " + exportedTypes.RenderAny());
        }

        private static void VerifyForgeables(IList<StmtClassForgeable> forgables)
        {
            // there can only be one class of the same name
            ISet<string> names = new HashSet<string>();
            foreach (var forgable in forgables) {
                if (names.Contains(forgable.ClassName)) {
                    throw new IllegalStateException("Class name '" + forgable.ClassName + "' appears twice");
                }

                names.Add(forgable.ClassName);
            }

            // there can be only one fields and statement provider
            StmtClassForgeable stmtProvider = null;
            foreach (var forgable in forgables) {
                if (forgable.ForgeableType == StmtClassForgeableType.STMTPROVIDER) {
                    if (stmtProvider != null) {
                        throw new IllegalStateException("Multiple stmt-provider classes");
                    }

                    stmtProvider = forgable;
                }
            }
        }

        private static void HandleStatementCompileHook(
            Attribute[] annotations,
            StatementCompileTimeServices compileTimeServices,
            StatementSpecCompiled specCompiled)
        {
            StatementCompileHook compileHook = null;
            try {
                compileHook = (StatementCompileHook) ImportUtil.GetAnnotationHook(
                    annotations,
                    HookType.INTERNAL_COMPILE,
                    typeof(StatementCompileHook),
                    compileTimeServices.ImportServiceCompileTime);
            }
            catch (ExprValidationException e) {
                throw new EPException("Failed to obtain hook for " + HookType.INTERNAL_QUERY_PLAN, e);
            }

            compileHook?.Compiled(specCompiled);
        }

        protected internal static string GetNameFromAnnotation(Attribute[] annotations)
        {
            if (annotations != null && annotations.Length != 0) {
                foreach (var annotation in annotations) {
                    if (annotation is NameAttribute name && name.Value != null) {
                        return name.Value;
                    }
                }
            }

            return null;
        }

        private static void RewriteNamedWindowSubselect(
            IList<ExprDotNode> chainedExpressionsDot,
            IList<ExprSubselectNode> subselects,
            NamedWindowCompileTimeResolver service)
        {
            foreach (var dotNode in chainedExpressionsDot) {
                if (dotNode.ChainSpec.IsEmpty()) {
                    continue;
                }
                
                var proposedWindow = dotNode.ChainSpec[0].GetRootNameOrEmptyString();
                var namedWindowDetail = service.Resolve(proposedWindow);
                if (namedWindowDetail == null) {
                    continue;
                }

                // build spec for subselect
                var raw = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
                var filter = new FilterSpecRaw(proposedWindow, Collections.GetEmptyList<ExprNode>(), null);
                raw.StreamSpecs.Add(
                    new FilterStreamSpecRaw(
                        filter,
                        ViewSpec.EMPTY_VIEWSPEC_ARRAY,
                        proposedWindow,
                        StreamSpecOptions.DEFAULT));

                
                var modified = new List<Chainable>(dotNode.ChainSpec);
                var firstChain = modified.DeleteAt(0);
                var firstChainParams = firstChain.GetParametersOrEmpty();
                if (!firstChainParams.IsEmpty()) {
                    if (firstChainParams.Count == 1) {
                        raw.WhereClause = firstChainParams[0];
                    } else {
                        ExprAndNode andNode = new ExprAndNodeImpl();
                        foreach (ExprNode node in firstChainParams) {
                            andNode.AddChildNode(node);
                        }
                        raw.WhereClause = andNode;
                    }
                }

                // activate subselect
                ExprSubselectNode subselect = new ExprSubselectRowNode(raw);
                subselects.Add(subselect);
                dotNode.ChildNodes = new ExprNode[] {subselect};
                dotNode.ChainSpec = modified;
            }
        }

        private static IList<ExprTableAccessNode> DetermineTableAccessNodes(
            ISet<ExprTableAccessNode> statementDirectTableAccess,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            ISet<ExprTableAccessNode> tableAccessNodes = new HashSet<ExprTableAccessNode>();
            if (statementDirectTableAccess != null) {
                tableAccessNodes.AddAll(statementDirectTableAccess);
            }

            // include all declared expression usages
            var tableAccessVisitor = new ExprNodeTableAccessVisitor(tableAccessNodes);
            foreach (var declared in visitor.DeclaredExpressions) {
                declared.Body.Accept(tableAccessVisitor);
            }

            // include all subqueries (and their declared expressions)
            // This is nested as declared expressions can have more subqueries, however all subqueries are in this list.
            foreach (var subselectNode in visitor.Subselects) {
                tableAccessNodes.AddAll(subselectNode.StatementSpecRaw.TableExpressions);
            }

            return new List<ExprTableAccessNode>(tableAccessNodes);
        }
    }
} // end of namespace