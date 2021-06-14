///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    [Serializable]
    public class ExprNodeScript : ExprNodeBase,
        ExprForge,
        ExprEnumerationForge,
        ExprNodeInnerNodeProvider
    {
        public const string CONTEXT_BINDING_NAME = "epl";

        private readonly string _defaultDialect;
        private EventType _eventTypeCollection;
        private ScriptDescriptorCompileTime _scriptDescriptor;

        public ExprNodeScript(
            string defaultDialect,
            ExpressionScriptProvided script,
            IList<ExprNode> parameters)
        {
            _defaultDialect = defaultDialect;
            Script = script ?? throw new ArgumentException("script cannot be null", nameof(script));
            Parameters = parameters;
        }

        public override ExprForge Forge => this;

        public IList<ExprNode> Parameters { get; private set; }

        public string EventTypeNameAnnotation => Script.OptionalEventTypeName;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExpressionScriptProvided Script { get; }

        public bool IsConstantResult => false;

        public Type ComponentTypeCollection {
            get {
                var returnType = _scriptDescriptor.ReturnType;
                if (returnType.IsArray) {
                    return returnType.GetElementType();
                }

                return null;
            }
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _eventTypeCollection;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
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

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime()
                };
            }
        }

        public Type EvaluationType => _scriptDescriptor.ReturnType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprNodeRenderable EnumForgeRenderable => this;

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

        public IList<ExprNode> AdditionalNodes => Parameters;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(Script.Name);
            ExprNodeUtilityPrint.ToExpressionStringIncludeParen(Parameters, writer);
        }

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

            var that = (ExprNodeScript) node;

            if (!Script?.Equals(that.Script) ?? that.Script != null) {
                return false;
            }

            return ExprNodeUtilityCompare.DeepEquals(Parameters, that.Parameters);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (Script.ParameterNames.Length != Parameters.Count) {
                throw new ExprValidationException(
                    string.Format(
                        "Invalid number of parameters for script '{0}', expected {1} parameters but received {2} parameters",
                        Script.Name,
                        Script.ParameterNames.Length,
                        Parameters.Count));
            }

            if (!validationContext.StatementCompileTimeService.Configuration.Compiler.Scripts.IsEnabled) {
                throw new ExprValidationException("Script compilation has been disabled by configuration");
            }

            // validate all expression parameters
            var validatedParameters = Parameters
                .Select(
                    expr => ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SCRIPTPARAMS,
                        expr,
                        validationContext))
                .ToList();

            // set up map of input parameter names and evaluators
            var forges = new ExprForge[Script.ParameterNames.Length];
            for (var i = 0; i < Script.ParameterNames.Length; i++) {
                forges[i] = validatedParameters[i].Forge;
            }

            Parameters = validatedParameters;

            // Compile script
            var parameterTypes = ExprNodeUtilityQuery.GetExprResultTypes(forges);
            var dialect = Script.OptionalDialect ?? _defaultDialect;
            var compiled = CompileScript(
                dialect,
                Script.Name,
                Script.Expression,
                Script.ParameterNames,
                parameterTypes,
                Script.CompiledBuf,
                validationContext.ImportService,
                validationContext.ScriptCompiler);

            // Determine declared return type
            var declaredReturnType = GetDeclaredReturnType(Script.OptionalReturnTypeName, validationContext);
            if (Script.IsOptionalReturnTypeIsArray && declaredReturnType != null) {
                declaredReturnType = TypeHelper.GetArrayType(declaredReturnType);
            }

            Type returnType;
            if (compiled.KnownReturnType == null && Script.OptionalReturnTypeName == null) {
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
                            Script.Name +
                            "', known return type is " +
                            knownReturnType.Name +
                            " versus declared return type " +
                            declaredReturnType.Name);
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

            _eventTypeCollection = null;
            if (Script.OptionalEventTypeName != null) {
                if (returnType.IsArray && returnType.GetElementType() == typeof(EventBean)) {
                    _eventTypeCollection = EventTypeUtility.RequireEventType(
                        "Script",
                        Script.Name,
                        Script.OptionalEventTypeName,
                        validationContext.StatementCompileTimeService.EventTypeCompileTimeResolver);
                }
                else {
                    throw new ExprValidationException(EventTypeUtility.DisallowedAtTypeMessage());
                }
            }

            _scriptDescriptor = new ScriptDescriptorCompileTime(
                Script.OptionalDialect,
                Script.Name,
                Script.Expression,
                Script.ParameterNames,
                Parameters.ToArray(),
                returnType,
                _defaultDialect);
            return null;
        }

        private ExpressionScriptCompiled CompileScript(
            string dialect,
            string scriptName,
            string scriptExpression,
            string[] scriptParameterNames,
            Type[] parameterTypes,
            ExpressionScriptCompiled scriptCompiledBuf,
            ImportServiceCompileTime importService,
            ScriptCompiler scriptingCompiler)
        {
            return new ExpressionScriptCompiledImpl(
                scriptingCompiler.Compile(
                    Script.OptionalDialect ?? _defaultDialect,
                    Script));
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptParams(visitor, Parameters);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            base.Accept(visitor);
            ExprNodeUtilityQuery.AcceptParams(visitor, Parameters);
        }

        public override void AcceptChildnodes(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            base.AcceptChildnodes(visitor, parent);
            ExprNodeUtilityQuery.AcceptParams(visitor, Parameters, this);
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
                symbols.GetAddEPS(codegenMethodScope),
                symbols.GetAddIsNewData(codegenMethodScope),
                symbols.GetAddExprEvalCtx(codegenMethodScope));
        }

        public CodegenExpressionInstanceField GetField(CodegenClassScope codegenClassScope)
        {
            return codegenClassScope.NamespaceScope.AddOrGetDefaultFieldSharable(
                new ScriptCodegenFieldSharable(_scriptDescriptor, codegenClassScope));
        }

        private Type GetDeclaredReturnType(
            string returnTypeName,
            ExprValidationContext validationContext)
        {
            if (returnTypeName == null) {
                return null;
            }

            if (returnTypeName == "void") {
                return null;
            }

            var returnType = TypeHelper.GetTypeForSimpleName(
                returnTypeName,
                validationContext.ImportService.ClassForNameProvider);
            if (returnType != null) {
                return returnType;
            }

            if (returnTypeName.Equals("EventBean")) {
                return typeof(EventBean);
            }

            try {
                return validationContext.ImportService.ResolveClass(
                    returnTypeName,
                    false,
                    ExtensionClassEmpty.INSTANCE);
            }
            catch (ImportException) {
                throw new ExprValidationException(
                    "Failed to resolve return type '" +
                    returnTypeName +
                    "' specified for script '" +
                    Script.Name +
                    "'");
            }
        }
    }
} // end of namespace