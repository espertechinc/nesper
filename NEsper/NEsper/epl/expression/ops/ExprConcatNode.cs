///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents a string concatenation.
    /// </summary>
    [Serializable]
    public class ExprConcatNode : ExprNodeBase
    {
        private ExprEvaluator _evaluator;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count < 2)
            {
                throw new ExprValidationException("Concat node must have at least 2 parameters");
            }

            ExprEvaluator[] evaluators = ExprNodeUtility.GetEvaluators(this.ChildNodes);
    
            for (var i = 0; i < evaluators.Length; i++)
            {
                var childType = evaluators[i].ReturnType;
                var childTypeName = childType == null ? "null" : Name.Clean(childType);
                if (childType != typeof(String))
                {
                    throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to string is not allowed", childTypeName));
                }
            }

            ConfigurationEngineDefaults.ThreadingProfile threadingProfile = validationContext.EngineImportService.ThreadingProfile;
            if (threadingProfile == ConfigurationEngineDefaults.ThreadingProfile.LARGE)
            {
                _evaluator = new ExprConcatNodeEvalWNew(this, evaluators);
            }
            else
            {
                _evaluator = new ExprConcatNodeEvalThreadLocal(this, evaluators, validationContext.ThreadLocalManager);
            }

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _evaluator; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            foreach (ExprNode child in ChildNodes)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence);
                delimiter = "||";
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.CONCAT; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprConcatNode;
        }
    }
}
