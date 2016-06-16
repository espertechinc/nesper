///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.script;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.script
{
    [Serializable]
    public class ExprNodeScript 
        : ExprNodeBase
        , ExprNodeInnerNodeProvider
    {
        public const string CONTEXT_BINDING_NAME = "epl";

        private readonly String _defaultDialect;

        [NonSerialized]
        private ExprEvaluator _evaluator;
    
        public ExprNodeScript(String defaultDialect, ExpressionScriptProvided script, IList<ExprNode> parameters)
        {
            _defaultDialect = defaultDialect;
            Script = script;
            Parameters = parameters;
        }

        public IList<ExprNode> AdditionalNodes
        {
            get { return Parameters; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _evaluator; }
        }

        public IList<ExprNode> Parameters { get; private set; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(Script.Name);
            ExprNodeUtility.ToExpressionStringIncludeParen(Parameters, writer);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public ExpressionScriptProvided Script { get; private set; }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node)
        {
            if (this == node) return true;
            if (node == null || GetType() != node.GetType()) return false;
    
            var that = (ExprNodeScript) node;
    
            if (Script != null ? !Script.Equals(that.Script) : that.Script != null) return false;
            return ExprNodeUtility.DeepEquals(Parameters, that.Parameters);
        }
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (_evaluator != null)
            {
                return null;
            }

            var service = validationContext.ScriptingService;

            if (Script.ParameterNames.Count != Parameters.Count) {
                throw new ExprValidationException("Invalid number of parameters for script '" + Script.Name + "', expected " + Script.ParameterNames.Count + " parameters but received " + Parameters.Count + " parameters");
            }
    
            // validate all expression parameters
            var validatedParameters = Parameters
                .Select(expr => ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SCRIPTPARAMS, expr, validationContext))
                .ToList();

            // set up map of input parameter names and evaluators
            var inputParamNames = new String[Script.ParameterNames.Count];
            var evaluators = new ExprEvaluator[Script.ParameterNames.Count];
    
            for (int i = 0; i < Script.ParameterNames.Count; i++) {
                inputParamNames[i] = Script.ParameterNames[i];
                evaluators[i] = validatedParameters[i].ExprEvaluator;
            }
    
            // Compile script
            if (Script.Compiled == null) {
                CompileScript(validationContext.ScriptingService, evaluators);
            }
    
            // Determine declared return type
            Type declaredReturnType = GetDeclaredReturnType(Script.OptionalReturnTypeName, validationContext);
            if (Script.IsOptionalReturnTypeIsArray && declaredReturnType != null) {
                declaredReturnType = TypeHelper.GetArrayType(declaredReturnType);
            }
            Type returnType;
            if (Script.Compiled.KnownReturnType == null && Script.OptionalReturnTypeName == null) {
                returnType = typeof(Object);
            }
            else if (Script.Compiled.KnownReturnType != null) {
                if (declaredReturnType == null) {
                    returnType = Script.Compiled.KnownReturnType;
                }
                else {
                    var knownReturnType = Script.Compiled.KnownReturnType;
                    if (declaredReturnType.IsArray && knownReturnType.IsArray) {
                        // we are fine
                    }
                    else if (!knownReturnType.IsAssignmentCompatible(declaredReturnType)) {
                        throw new ExprValidationException("Return type and declared type not compatible for script '" + Script.Name + "', known return type is " + knownReturnType.Name + " versus declared return type " + declaredReturnType.Name);
                    }
                    returnType = declaredReturnType;
                }
            }
            else {
                returnType = declaredReturnType;
            }
            if (returnType == null) {
                returnType = typeof(Object);
            }
    
            // Prepare evaluator - this sets the evaluator
            PrepareEvaluator(validationContext.StatementName, inputParamNames, evaluators, returnType);

            return null;
        }

        private void CompileScript(ScriptingService scriptingService, ExprEvaluator[] evaluators)
        {
            Script.Compiled = new ExpressionScriptCompiledImpl(
                scriptingService.Compile(
                    Script.OptionalDialect ?? _defaultDialect,
                    Script));
        }
    
        private void PrepareEvaluator(String statementName, String[] inputParamNames, ExprEvaluator[] evaluators, Type returnType)
        {
            var scriptExpression = (ExpressionScriptCompiledImpl) Script.Compiled;
            _evaluator = new ExprNodeScriptEvalImpl(
                Script.Name, statementName, inputParamNames, evaluators, returnType, scriptExpression.ScriptAction);
        }

        private Type GetDeclaredReturnType(String returnTypeName, ExprValidationContext validationContext)
        {
            if (returnTypeName == null)
            {
                return null;
            }

            if (returnTypeName.Equals("void"))
            {
                return null;
            }

            var returnType = TypeHelper.GetTypeForSimpleName(returnTypeName, false);
            if (returnType != null)
            {
                return returnType;
            }

            try
            {
                return validationContext.MethodResolutionService.ResolveType(returnTypeName, false);
            }
            catch (EngineImportException e1)
            {
                throw new ExprValidationException(
                    "Failed to resolve return type '" + returnTypeName + "' specified for script '" + Script.Name + "'");
            }
        }
    }
}
