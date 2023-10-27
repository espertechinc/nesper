///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents a string concatenation.
    /// </summary>
    [Serializable]
    public class ExprConcatNode : ExprNodeBase
    {
        [JsonIgnore]
        [NonSerialized]
        private ExprConcatNodeForge _forge;
        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.CONCAT;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length < 2) {
                throw new ExprValidationException("Concat node must have at least 2 parameters");
            }

            for (var i = 0; i < ChildNodes.Length; i++) {
                var childType = ChildNodes[i].Forge.EvaluationType;
                var childTypeName = childType == null ? "null" : childType.CleanName();
                if (childType != typeof(string)) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        childTypeName +
                        "' to System.String is not allowed");
                }
            }

            var threadLocalManager = validationContext.Container.ThreadLocalManager();
            var threadingProfile = validationContext.StatementCompileTimeService.Configuration.Common.Execution
                .ThreadingProfile;
            _forge = new ExprConcatNodeForge(threadLocalManager, this, threadingProfile);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var delimiter = "";
            foreach (var child in ChildNodes) {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence, flags);
                delimiter = "||";
            }
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprConcatNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace