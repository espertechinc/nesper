///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.spec.util
{
    public class StatementSpecCompiledAnalyzer {
    
        public static StatementSpecCompiledAnalyzerResult AnalyzeFilters(StatementSpecCompiled spec) {
            IList<FilterSpecCompiled> filters = new List<FilterSpecCompiled>();
            IList<NamedWindowConsumerStreamSpec> namedWindows = new List<NamedWindowConsumerStreamSpec>();
    
            AddFilters(spec.StreamSpecs, filters, namedWindows);
    
            foreach (ExprSubselectNode subselect in spec.SubSelectExpressions) {
                AddFilters(subselect.StatementSpecCompiled.StreamSpecs, filters, namedWindows);
            }
    
            return new StatementSpecCompiledAnalyzerResult(filters, namedWindows);
        }
    
        private static void AddFilters(StreamSpecCompiled[] streams, IList<FilterSpecCompiled> filters, IList<NamedWindowConsumerStreamSpec> namedWindows) {
            foreach (StreamSpecCompiled compiled in streams) {
                if (compiled is FilterStreamSpecCompiled) {
                    FilterStreamSpecCompiled c = (FilterStreamSpecCompiled) compiled;
                    filters.Add(c.FilterSpec);
                }
                if (compiled is PatternStreamSpecCompiled) {
                    PatternStreamSpecCompiled r = (PatternStreamSpecCompiled) compiled;
                    EvalNodeAnalysisResult evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes((r.EvalFactoryNode));
                    IList<EvalFilterFactoryNode> filterNodes = evalNodeAnalysisResult.FilterNodes;
                    foreach (EvalFilterFactoryNode filterNode in filterNodes)
                    {
                        filters.Add(filterNode.FilterSpec);
                    }
                }
                if (compiled is NamedWindowConsumerStreamSpec) {
                    namedWindows.Add((NamedWindowConsumerStreamSpec) compiled);
                }
            }
        }
    }
}
