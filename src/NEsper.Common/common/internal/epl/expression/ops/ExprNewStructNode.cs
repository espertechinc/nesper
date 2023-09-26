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

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents the "new {...}" operator in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNewStructNode : ExprNodeBase
    {
        private ExprNewStructNodeForge _forge;

        public ExprNewStructNode(string[] columnNames)
        {
            ColumnNames = columnNames;
        }

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

        public Type EvaluationType => typeof(IDictionary<object, object>);

        public string[] ColumnNames { get; }

        public bool IsConstantResult {
            get {
                CheckValidated(_forge);
                return _forge.IsAllConstants;
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var eventType = new LinkedHashMap<string, object>();

            var isAllConstants = false;
            for (var i = 0; i < ColumnNames.Length; i++) {
                isAllConstants = isAllConstants && ChildNodes[i].Forge.ForgeConstantType.IsCompileTimeConstant;
                if (eventType.ContainsKey(ColumnNames[i])) {
                    throw new ExprValidationException(
                        "Failed to validate new-keyword property names, property '" +
                        ColumnNames[i] +
                        "' has already been declared");
                }

                IDictionary<string, object> eventTypeResult = null;
                if (ChildNodes[i].Forge is ExprTypableReturnForge) {
                    eventTypeResult = ((ExprTypableReturnForge)ChildNodes[i].Forge).RowProperties;
                }

                if (eventTypeResult != null) {
                    eventType.Put(ColumnNames[i], eventTypeResult);
                }
                else {
                    var classResult = ChildNodes[i].Forge.EvaluationType.GetBoxedType();
                    eventType.Put(ColumnNames[i], classResult);
                }
            }

            _forge = new ExprNewStructNodeForge(this, isAllConstants, eventType);
            return null;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprNewStructNode other)) {
                return false;
            }

            var columnNamesSet = new HashSet<string>(ColumnNames);
            return columnNamesSet.SetEquals(other.ColumnNames);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("new{");
            var delimiter = "";
            for (var i = 0; i < ChildNodes.Length; i++) {
                writer.Write(delimiter);
                writer.Write(ColumnNames[i]);
                var expr = ChildNodes[i];

                var outputexpr = true;
                if (expr is ExprIdentNode prop) {
                    if (prop.ResolvedPropertyName.Equals(ColumnNames[i])) {
                        outputexpr = false;
                    }
                }

                if (outputexpr) {
                    writer.Write("=");
                    expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
                }

                delimiter = ",";
            }

            writer.Write("}");
        }
    }
} // end of namespace