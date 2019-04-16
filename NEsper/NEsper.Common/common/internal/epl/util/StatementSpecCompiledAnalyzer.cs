///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class StatementSpecCompiledAnalyzer
    {
        public static StatementSpecCompiledAnalyzerResult AnalyzeFilters(StatementSpecCompiled spec)
        {
            IList<FilterSpecCompiled> filters = new List<FilterSpecCompiled>();
            IList<NamedWindowConsumerStreamSpec> namedWindows = new List<NamedWindowConsumerStreamSpec>();

            AddFilters(spec.StreamSpecs, filters, namedWindows);

            foreach (var subselect in spec.SubselectNodes) {
                AddFilters(subselect.StatementSpecCompiled.StreamSpecs, filters, namedWindows);
            }

            return new StatementSpecCompiledAnalyzerResult(filters, namedWindows);
        }

        private static void AddFilters(
            StreamSpecCompiled[] streams,
            IList<FilterSpecCompiled> filters,
            IList<NamedWindowConsumerStreamSpec> namedWindows)
        {
            foreach (var compiled in streams) {
                if (compiled is FilterStreamSpecCompiled) {
                    var c = (FilterStreamSpecCompiled) compiled;
                    filters.Add(c.FilterSpecCompiled);
                }

                if (compiled is PatternStreamSpecCompiled) {
                    var r = (PatternStreamSpecCompiled) compiled;
                    var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(r.Root);
                    var filterNodes = evalNodeAnalysisResult.FilterNodes;
                    foreach (var filterNode in filterNodes) {
                        filters.Add(filterNode.FilterSpecCompiled);
                    }
                }

                if (compiled is NamedWindowConsumerStreamSpec) {
                    namedWindows.Add((NamedWindowConsumerStreamSpec) compiled);
                }
            }
        }
    }
} // end of namespace