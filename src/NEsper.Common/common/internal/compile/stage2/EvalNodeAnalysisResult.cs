///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    /// Result of analysis of pattern expression node tree.
    /// </summary>
    public class EvalNodeAnalysisResult
    {
        private IList<EvalForgeNode> activeNodes = new List<EvalForgeNode>();

        /// <summary>
        /// Add a node found.
        /// </summary>
        /// <param name="node">found</param>
        public void AddNode(EvalForgeNode node)
        {
            activeNodes.Add(node);
        }

        /// <summary>
        /// Returns all nodes found.
        /// </summary>
        /// <returns>pattern nodes</returns>
        public IList<EvalForgeNode> ActiveNodes => activeNodes;

        public IList<EvalFilterForgeNode> FilterNodes {
            get {
                IList<EvalFilterForgeNode> filterNodes = new List<EvalFilterForgeNode>();
                foreach (var node in activeNodes) {
                    if (node is EvalFilterForgeNode forgeNode) {
                        filterNodes.Add(forgeNode);
                    }
                }

                return filterNodes;
            }
        }

        public IList<EvalMatchUntilForgeNode> RepeatNodes {
            get {
                IList<EvalMatchUntilForgeNode> filterNodes = new List<EvalMatchUntilForgeNode>();
                foreach (var node in activeNodes) {
                    if (node is EvalMatchUntilForgeNode forgeNode) {
                        filterNodes.Add(forgeNode);
                    }
                }

                return filterNodes;
            }
        }
    }
} // end of namespace