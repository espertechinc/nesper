///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ExprNodeScript : ExprNodeBase,
        ExprForge,
        ExprEnumerationForge,
        ExprNodeInnerNodeProvider
    {
        public const string CONTEXT_BINDING_NAME = "epl";
        
        private readonly string defaultDialect;
        private readonly ExpressionScriptProvided script;
        private IList<ExprNode> parameters;
        private ScriptDescriptorCompileTime scriptDescriptor;
        private EventType eventTypeCollection;

        public ExprNodeScript(
            string defaultDialect,
            ExpressionScriptProvided script,
            IList<ExprNode> parameters)
        {
            this.defaultDialect = defaultDialect;
            this.script = script;
            this.parameters = parameters;
        }

        public override ExprForge Forge => this;
        public IList<ExprNode> AdditionalNodes => parameters;
        public IList<ExprNode> Parameters => parameters;
        public string EventTypeNameAnnotation => script.OptionalEventTypeName;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(script.Name);
            ExprNodeUtilityPrint.ToExpressionStringIncludeParen(parameters, writer);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;
        public ExpressionScriptProvided Script => script;
        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (this == node) {
                return true;
            }

            if (node == null || GetType() != node.GetType()) {
                return false;
            }

            var that = (ExprNodeScript)node;
            if (!script?.Equals(that.script) ?? that.script != null) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(parameters, that.parameters);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (script.ParameterNames.Length != parameters.Count) {
                throw new ExprValidationException(
                    "Invalid number of parameters for script '" +
                    script.Name +
                    "', expected " +
                    script.ParameterNames.Length +
                    " parameters but received " +
                    parameters.Count +
                    " parameters");
            }

            if (!validationContext.StatementCompileTimeService.Configuration.Compiler.Scripts.IsEnabled) {
                throw new ExprValidationException("Script compilation has been disabled by configuration");
            }

            // validate all expression parameters
            IList<ExprNode> validatedParameters = new List<ExprNode>();
            foreach (var expr in parameters) {
                validatedParameters.Add(
                    ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SCRIPTPARAMS, expr, validationContext));
            }

            // set up map of input parameter names and evaluators
            var forges = new ExprForge[script.ParameterNames.Length];
            for (var i = 0; i < script.ParameterNames.Length; i++) {
                forges[i] = validatedParameters[i].Forge;
            }

            parameters = validatedParameters;
            // Compile script
            var parameterTypes = ExprNodeUtilityQuery.GetExprResultTypes(forges);
            var dialect = script.OptionalDialect ?? defaultDialect;
            var compiled = ExpressionNodeScriptCompiler.CompileScript(
                dialect,
                script.Name,
                script.Expression,
                script.ParameterNames,
                parameterTypes,
                script.CompiledBuf,
                validationContext.ImportService,
                validationContext.ScriptCompiler);
            
            // Determine declared return type
            var declaredReturnType = GetDeclaredReturnType(script.OptionalReturnTypeName, validationContext);
            Type returnType;
            if (compiled.KnownReturnType == null && script.OptionalReturnTypeName == null) {
                returnType = typeof(object);
            }
            else if (compiled.KnownReturnType != null) {
                if (declaredReturnType == null) {
                    returnType = compiled.KnownReturnType;
                }
                else {
                    var knownReturnType = compiled.KnownReturnType;
                    if (declaredReturnType.IsArray && knownReturnType.IsArray) {
                        // we are fine
                    }
                    else if (!knownReturnType.IsAssignmentCompatible(declaredReturnType)) {
                        throw new ExprValidationException(
                            "Return type and declared type not compatible for script '" +
                            script.Name +
                            "', known return type is " +
                            knownReturnType.CleanName() +
                            " versus declared return type " +
                            declaredReturnType.CleanName());
                    }

                    returnType = declaredReturnType;
                }
            }
            else {
                returnType = declaredReturnType;
            }

            if (returnType == null) {
                returnType = typeof(object);
            }

            eventTypeCollection = null;
            if (script.OptionalEventTypeName != null) {
                if (returnType.IsArray && returnType.GetComponentType() == typeof(EventBean)) {
                    eventTypeCollection = EventTypeUtility.RequireEventType(
                        "Script",
                        script.Name,
                        script.OptionalEventTypeName,
                        validationContext.StatementCompileTimeService.EventTypeCompileTimeResolver);
                }
                else {
                    throw new ExprValidationException(EventTypeUtility.DisallowedAtTypeMessage());
                }
            }

            scriptDescriptor = new ScriptDescriptorCompileTime(
                script.OptionalDialect,
                script.Name,
                script.Expression,
                script.ParameterNames,
                parameters.ToArray(),
                returnType,
                defaultDialect);
            return null;
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptParams(visitor, parameters);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            Accept(visitor);
            ExprNodeUtilityQuery.AcceptParams(visitor, parameters);
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtilityQuery.AcceptParams(visitor, parameters, this);
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return eventTypeCollection;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return CodegenLegoCast.CastSafeFromObjectType(
                requiredType,
                MakeEval("Evaluate", codegenMethodScope, symbols, codegenClassScope));
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return MakeEval("EvaluateGetROCollectionEvents", codegenMethodScope, symbols, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return MakeEval("EvaluateGetROCollectionScalar", codegenMethodScope, symbols, codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return MakeEval("EvaluateGetEventBean", codegenMethodScope, symbols, codegenClassScope);
        }

        private CodegenExpression MakeEval(
            string method,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var eval = GetField(codegenClassScope);
            return ExprDotMethod(
                eval,
                method,
                symbols.GetAddEps(codegenMethodScope),
                symbols.GetAddIsNewData(codegenMethodScope),
                symbols.GetAddExprEvalCtx(codegenMethodScope));
        }

        public CodegenExpressionInstanceField GetField(CodegenClassScope codegenClassScope)
        {
            return codegenClassScope.NamespaceScope.AddOrGetDefaultFieldSharable(
                new ScriptCodegenFieldSharable(scriptDescriptor, codegenClassScope));
        }

        private Type GetDeclaredReturnType(
            string returnTypeName,
            ExprValidationContext validationContext)
        {
            if (returnTypeName == null) {
                return null;
            }

            if (returnTypeName.Equals("void")) {
                return null;
            }

            var simpleNameType = TypeHelper.GetTypeForSimpleName(returnTypeName, validationContext.ImportService.TypeResolver);
            if (simpleNameType != null) {
                return simpleNameType;
            }

            var returnTypeLower = returnTypeName.ToLowerInvariant();
            if (returnTypeLower.Equals("eventbean")) {
                return typeof(EventBean);
            }

            if (returnTypeLower.Equals("eventbean[]")) {
                return typeof(EventBean[]);
            }

            var classDescriptor = ClassDescriptor.ParseTypeText(returnTypeName);
            Type returnType = ImportTypeUtil.ResolveClassIdentifierToType(
                classDescriptor,
                false,
                validationContext.ImportService,
                validationContext.ClassProvidedExtension);
            if (returnType == null) {
                throw new ExprValidationException(
                    "Failed to resolve return type '" +
                    returnTypeName +
                    "' specified for script '" +
                    script.Name +
                    "'");
            }

            return returnType;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime()
                };
            }
        }

        public Type EvaluationType => scriptDescriptor.ReturnType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable => this;
        public ExprNodeRenderable EnumForgeRenderable => this;

        public Type ComponentTypeCollection {
            get {
                var returnType = scriptDescriptor.ReturnType;
                if (returnType.IsArray) {
                    return returnType.GetComponentType();
                }

                return null;
            }
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
    }
} // end of namespace