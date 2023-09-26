///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityAggregation
    {
        /// <summary>
        /// Returns true if all properties within the expression are witin data window'd streams.
        /// </summary>
        /// <param name="child">expression to interrogate</param>
        /// <param name="streamTypeService">streams</param>
        /// <param name="unidirectionalJoin">indicator unidirection join</param>
        /// <returns>indicator</returns>
        public static bool HasRemoveStreamForAggregations(
            ExprNode child,
            StreamTypeService streamTypeService,
            bool unidirectionalJoin)
        {
            // Determine whether all streams are istream-only or irstream
            var isIStreamOnly = streamTypeService.IStreamOnly;
            var isAllIStream = true; // all true?
            var isAllIRStream = true; // all false?
            foreach (var anIsIStreamOnly in isIStreamOnly) {
                if (!anIsIStreamOnly) {
                    isAllIStream = false;
                }
                else {
                    isAllIRStream = false;
                }
            }

            // determine if a data-window applies to this max function
            var hasDataWindows = true;
            if (isAllIStream) {
                hasDataWindows = false;
            }
            else if (!isAllIRStream) {
                if (streamTypeService.EventTypes.Length > 1) {
                    if (unidirectionalJoin) {
                        return false;
                    }

                    // In a join we assume that a data window is present or implicit via unidirectional
                }
                else {
                    hasDataWindows = false;
                    // get all aggregated properties to determine if any is from a windowed stream
                    var visitor = new ExprNodeIdentifierCollectVisitor();
                    child.Accept(visitor);
                    foreach (var node in visitor.ExprProperties) {
                        if (!isIStreamOnly[node.StreamId]) {
                            hasDataWindows = true;
                            break;
                        }
                    }
                }
            }

            return hasDataWindows;
        }

        public static ExprNodePropOrStreamSet GetAggregatedProperties(IList<ExprAggregateNode> aggregateNodes)
        {
            // Get a list of properties being aggregated in the clause.
            var propertiesAggregated = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);
            foreach (ExprNode selectAggExprNode in aggregateNodes) {
                visitor.Reset();
                selectAggExprNode.Accept(visitor);
                var properties = visitor.Refs;
                propertiesAggregated.AddAll(properties);
            }

            return propertiesAggregated;
        }

        public static void AddNonAggregatedProps(
            ExprNode exprNode,
            ExprNodePropOrStreamSet set,
            EventType[] types,
            ContextPropertyRegistry contextPropertyRegistry)
        {
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            exprNode.Accept(visitor);
            AddNonAggregatedProps(set, visitor.Refs, types, contextPropertyRegistry);
        }

        public static ExprNodePropOrStreamSet GetNonAggregatedProps(
            EventType[] types,
            IList<ExprNode> exprNodes,
            ContextPropertyRegistry contextPropertyRegistry)
        {
            // Determine all event properties in the clause
            var nonAggProps = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(false);
            foreach (var node in exprNodes) {
                visitor.Reset();
                node.Accept(visitor);
                AddNonAggregatedProps(nonAggProps, visitor.Refs, types, contextPropertyRegistry);
            }

            return nonAggProps;
        }

        public static ExprNodePropOrStreamSet GetGroupByPropertiesValidateHasOne(ExprNode[] groupByNodes)
        {
            // Get the set of properties refered to by all group-by expression nodes.
            var propertiesGroupBy = new ExprNodePropOrStreamSet();
            var visitor = new ExprNodeIdentifierAndStreamRefVisitor(true);

            foreach (var groupByNode in groupByNodes) {
                visitor.Reset();
                groupByNode.Accept(visitor);
                var propertiesNode = visitor.Refs;
                propertiesGroupBy.AddAll(propertiesNode);

                // For each group-by expression node, require at least one property.
                if (propertiesNode.IsEmpty()) {
                    throw new ExprValidationException("Group-by expressions must refer to property names");
                }
            }

            return propertiesGroupBy;
        }

        private static void AddNonAggregatedProps(
            ExprNodePropOrStreamSet nonAggProps,
            IList<ExprNodePropOrStreamDesc> refs,
            EventType[] types,
            ContextPropertyRegistry contextPropertyRegistry)
        {
            foreach (var pair in refs) {
                if (pair is ExprNodePropOrStreamPropDesc propDesc) {
                    var originType = types.Length > pair.StreamNum ? types[pair.StreamNum] : null;
                    if (originType == null ||
                        contextPropertyRegistry == null ||
                        !contextPropertyRegistry.IsPartitionProperty(originType, propDesc.PropertyName)) {
                        nonAggProps.Add(propDesc);
                    }
                }
                else {
                    nonAggProps.Add(pair);
                }
            }
        }
    }
} // end of namespace