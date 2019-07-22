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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StatementForgeMethodSelectUtil
    {
        protected internal static bool[] GetHasIStreamOnly(
            bool[] isNamedWindow,
            IList<ViewFactoryForge>[] views)
        {
            var result = new bool[views.Length];
            for (var i = 0; i < views.Length; i++) {
                if (isNamedWindow[i]) {
                    continue;
                }

                result[i] = !ViewFactoryForgeUtil.HasDataWindows(views[i]);
            }

            return result;
        }

        protected internal static string[] DetermineStreamNames(StreamSpecCompiled[] streams)
        {
            var streamNames = new string[streams.Length];
            for (var i = 0; i < streams.Length; i++) {
                // Assign a stream name for joins, if not supplied
                streamNames[i] = streams[i].OptionalStreamName;
                if (streamNames[i] == null) {
                    streamNames[i] = "stream_" + i;
                }
            }

            return streamNames;
        }

        protected internal static StreamJoinAnalysisResultCompileTime VerifyJoinViews(
            StatementSpecCompiled statementSpec,
            NamedWindowCompileTimeResolver namedWindowCompileTimeResolver)
        {
            var streamSpecs = statementSpec.StreamSpecs;
            var analysisResult = new StreamJoinAnalysisResultCompileTime(streamSpecs.Length);
            if (streamSpecs.Length < 2) {
                return analysisResult;
            }

            // Determine if any stream has a unidirectional keyword

            // inspect unidirectional indicator and named window flags
            for (var i = 0; i < statementSpec.StreamSpecs.Length; i++) {
                var streamSpec = statementSpec.StreamSpecs[i];
                if (streamSpec.Options.IsUnidirectional) {
                    analysisResult.SetUnidirectionalInd(i);
                }

                if (streamSpec.ViewSpecs.Length > 0) {
                    analysisResult.SetHasChildViews(i);
                }

                if (streamSpec is NamedWindowConsumerStreamSpec) {
                    var nwSpec = (NamedWindowConsumerStreamSpec) streamSpec;
                    if (nwSpec.OptPropertyEvaluator != null && !streamSpec.Options.IsUnidirectional) {
                        throw new ExprValidationException(
                            "Failed to validate named window use in join, contained-event is only allowed for named windows when marked as unidirectional");
                    }

                    var nwinfo = nwSpec.NamedWindow;
                    analysisResult.SetNamedWindowsPerStream(i, nwinfo);
                    analysisResult.UniqueKeys[i] = EventTableIndexMetadataUtil.GetUniqueness(
                        nwinfo.IndexMetadata,
                        nwinfo.Uniqueness);
                }
            }

            // non-outer-join: verify unidirectional can be on a single stream only
            if (statementSpec.StreamSpecs.Length > 1 && analysisResult.IsUnidirectional) {
                VerifyJoinUnidirectional(analysisResult, statementSpec);
            }

            // count streams that provide data, excluding streams that poll data (DB and method)
            var countProviderNonpolling = 0;
            for (var i = 0; i < statementSpec.StreamSpecs.Length; i++) {
                var streamSpec = statementSpec.StreamSpecs[i];
                if (streamSpec is MethodStreamSpec ||
                    streamSpec is DBStatementStreamSpec ||
                    streamSpec is TableQueryStreamSpec) {
                    continue;
                }

                countProviderNonpolling++;
            }

            // if there is only one stream providing data, the analysis is done
            if (countProviderNonpolling == 1) {
                return analysisResult;
            }
            // there are multiple driving streams, verify the presence of a view for insert/remove stream

            // validation of join views works differently for unidirectional as there can be self-joins that don't require a view
            // see if this is a self-join in which all streams are filters and filter specification is the same.
            FilterSpecCompiled unidirectionalFilterSpec = null;
            FilterSpecCompiled lastFilterSpec = null;
            var pureSelfJoin = true;
            foreach (var streamSpec in statementSpec.StreamSpecs) {
                if (!(streamSpec is FilterStreamSpecCompiled)) {
                    pureSelfJoin = false;
                    continue;
                }

                var filterSpec = ((FilterStreamSpecCompiled) streamSpec).FilterSpecCompiled;
                if (lastFilterSpec != null && !lastFilterSpec.EqualsTypeAndFilter(filterSpec)) {
                    pureSelfJoin = false;
                }

                if (streamSpec.ViewSpecs.Length > 0) {
                    pureSelfJoin = false;
                }

                lastFilterSpec = filterSpec;

                if (streamSpec.Options.IsUnidirectional) {
                    unidirectionalFilterSpec = filterSpec;
                }
            }

            // self-join without views and not unidirectional
            if (pureSelfJoin && unidirectionalFilterSpec == null) {
                analysisResult.IsPureSelfJoin = true;
                return analysisResult;
            }

            // weed out filter and pattern streams that don't have a view in a join
            for (var i = 0; i < statementSpec.StreamSpecs.Length; i++) {
                var streamSpec = statementSpec.StreamSpecs[i];
                if (streamSpec.ViewSpecs.Length > 0) {
                    continue;
                }

                var name = streamSpec.OptionalStreamName;
                if (name == null && streamSpec is FilterStreamSpecCompiled) {
                    name = ((FilterStreamSpecCompiled) streamSpec).FilterSpecCompiled.FilterForEventTypeName;
                }

                if (name == null && streamSpec is PatternStreamSpecCompiled) {
                    name = "pattern event stream";
                }

                if (streamSpec.Options.IsUnidirectional) {
                    continue;
                }

                // allow a self-join without a child view, in that the filter spec is the same as the unidirection's stream filter
                if (unidirectionalFilterSpec != null &&
                    streamSpec is FilterStreamSpecCompiled &&
                    ((FilterStreamSpecCompiled) streamSpec).FilterSpecCompiled.EqualsTypeAndFilter(
                        unidirectionalFilterSpec)) {
                    analysisResult.SetUnidirectionalNonDriving(i);
                    continue;
                }

                if (streamSpec is FilterStreamSpecCompiled ||
                    streamSpec is PatternStreamSpecCompiled) {
                    throw new ExprValidationException(
                        "Joins require that at least one view is specified for each stream, no view was specified for " +
                        name);
                }
            }

            return analysisResult;
        }

        private static void VerifyJoinUnidirectional(
            StreamJoinAnalysisResultCompileTime analysisResult,
            StatementSpecCompiled statementSpec)
        {
            var numUnidirectionalStreams = analysisResult.UnidirectionalCount;
            var numStreams = statementSpec.StreamSpecs.Length;

            // only a single stream is unidirectional (applies to all but all-full-outer-join)
            if (!IsFullOuterJoinAllStreams(statementSpec)) {
                if (numUnidirectionalStreams > 1) {
                    throw new ExprValidationException(
                        "The unidirectional keyword can only apply to one stream in a join");
                }
            }
            else {
                // verify full-outer-join: requires unidirectional for all streams
                if (numUnidirectionalStreams > 1 && numUnidirectionalStreams < numStreams) {
                    throw new ExprValidationException(
                        "The unidirectional keyword must either apply to a single stream or all streams in a full outer join");
                }
            }

            // verify no-child-view for unidirectional
            for (var i = 0; i < statementSpec.StreamSpecs.Length; i++) {
                if (analysisResult.UnidirectionalInd[i]) {
                    if (analysisResult.HasChildViews[i]) {
                        throw new ExprValidationException(
                            "The unidirectional keyword requires that no views are declared onto the stream (applies to stream " +
                            i +
                            ")");
                    }
                }
            }
        }

        private static bool IsFullOuterJoinAllStreams(StatementSpecCompiled statementSpec)
        {
            var outers = statementSpec.Raw.OuterJoinDescList;
            if (outers == null || outers.Count == 0) {
                return false;
            }

            for (var stream = 0; stream < statementSpec.StreamSpecs.Length - 1; stream++) {
                if (outers[stream].OuterJoinType != OuterJoinType.FULL) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace