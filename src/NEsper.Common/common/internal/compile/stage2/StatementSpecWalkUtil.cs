///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class StatementSpecWalkUtil
    {
        public static bool IsPotentialSelfJoin(StatementSpecCompiled spec)
        {
            // Include create-context as nested contexts that have pattern-initiated sub-contexts may change filters during execution
            if (spec.Raw.CreateContextDesc != null && spec.Raw.CreateContextDesc.ContextDetail is ContextNested) {
                return true;
            }

            // if order-by is specified, ans since multiple output rows may produce, ensure dispatch
            if (spec.Raw.OrderByList.Count > 0) {
                return true;
            }

            foreach (var streamSpec in spec.StreamSpecs) {
                if (streamSpec is PatternStreamSpecCompiled) {
                    return true;
                }
            }

            // not a self join
            if (spec.StreamSpecs.Length <= 1 && spec.SubselectNodes.Count == 0) {
                return false;
            }

            // join - determine types joined
            IList<EventType> filteredTypes = new List<EventType>();

            // consider subqueryes
            var optSubselectTypes = PopulateSubqueryTypes(spec.SubselectNodes);

            var hasFilterStream = false;
            foreach (var streamSpec in spec.StreamSpecs) {
                if (streamSpec is FilterStreamSpecCompiled compiled) {
                    var type = compiled.FilterSpecCompiled.FilterForEventType;
                    filteredTypes.Add(type);
                    hasFilterStream = true;
                }
            }

            if (filteredTypes.Count == 1 && optSubselectTypes.IsEmpty()) {
                return false;
            }

            // pattern-only streams are not self-joins
            if (!hasFilterStream) {
                return false;
            }

            // is type overlap in filters
            for (var i = 0; i < filteredTypes.Count; i++) {
                for (var j = i + 1; j < filteredTypes.Count; j++) {
                    var typeOne = filteredTypes[i];
                    var typeTwo = filteredTypes[j];
                    if (typeOne == typeTwo) {
                        return true;
                    }

                    if (typeOne.SuperTypes != null) {
                        foreach (var typeOneSuper in typeOne.SuperTypes) {
                            if (typeOneSuper == typeTwo) {
                                return true;
                            }
                        }
                    }

                    if (typeTwo.SuperTypes != null) {
                        foreach (var typeTwoSuper in typeTwo.SuperTypes) {
                            if (typeOne == typeTwoSuper) {
                                return true;
                            }
                        }
                    }
                }
            }

            // analyze subselect types
            if (!optSubselectTypes.IsEmpty()) {
                foreach (var typeOne in filteredTypes) {
                    if (optSubselectTypes.Contains(typeOne)) {
                        return true;
                    }

                    if (typeOne.SuperTypes != null) {
                        foreach (var typeOneSuper in typeOne.SuperTypes) {
                            if (optSubselectTypes.Contains(typeOneSuper)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static ISet<EventType> PopulateSubqueryTypes(IList<ExprSubselectNode> subSelectExpressions)
        {
            ISet<EventType> set = null;
            foreach (var subselect in subSelectExpressions) {
                foreach (var streamSpec in subselect.StatementSpecCompiled.StreamSpecs) {
                    if (streamSpec is FilterStreamSpecCompiled compiled) {
                        var type = compiled.FilterSpecCompiled.FilterForEventType;
                        if (set == null) {
                            set = new HashSet<EventType>();
                        }

                        set.Add(type);
                    }
                    else if (streamSpec is PatternStreamSpecCompiled specCompiled) {
                        var evalNodeAnalysisResult =
                            EvalNodeUtil.RecursiveAnalyzeChildNodes(specCompiled.Root);
                        var filterNodes = evalNodeAnalysisResult.FilterNodes;
                        foreach (var filterNode in filterNodes) {
                            if (set == null) {
                                set = new HashSet<EventType>();
                            }

                            set.Add(filterNode.FilterSpecCompiled.FilterForEventType);
                        }
                    }
                }
            }

            if (set == null) {
                return new EmptySet<EventType>();
            }

            return set;
        }
    }
} // end of namespace