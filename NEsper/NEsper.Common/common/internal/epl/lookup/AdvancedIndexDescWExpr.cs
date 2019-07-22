///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Linq;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public class AdvancedIndexDescWExpr
    {
        public AdvancedIndexDescWExpr(
            string indexTypeName,
            ExprNode[] indexedExpressions)
        {
            IndexTypeName = indexTypeName;
            IndexedExpressions = indexedExpressions;
        }

        public string IndexTypeName { get; }

        public ExprNode[] IndexedExpressions { get; }

        public string ToQueryPlan()
        {
            if (IndexedExpressions.Length == 0) {
                return IndexTypeName;
            }

            var writer = new StringWriter();
            writer.Write(IndexTypeName);
            writer.Write("(");
            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceAsList(IndexedExpressions, writer);
            writer.Write(")");
            return writer.ToString();
        }

        public AdvancedIndexIndexMultiKeyPart AdvancedIndexDescRuntime {
            get {
                var indexExpressionTexts = new string[IndexedExpressions.Length];
                var indexedProperties = new string[indexExpressionTexts.Length];
                for (var i = 0; i < IndexedExpressions.Length; i++) {
                    indexExpressionTexts[i] =
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(IndexedExpressions[i]);
                    var visitor = new ExprNodeIdentifierVisitor(true);
                    IndexedExpressions[i].Accept(visitor);
                    if (visitor.ExprProperties.Count != 1) {
                        throw new IllegalStateException("Failed to find indexed property");
                    }

                    indexedProperties[i] = visitor.ExprProperties.First().Second;
                }

                return new AdvancedIndexIndexMultiKeyPart(IndexTypeName, indexExpressionTexts, indexedProperties);
            }
        }
    }
} // end of namespace