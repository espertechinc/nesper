///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;

namespace com.espertech.esper.rowregex
{
    public class EventRowRegexNFAViewFactoryHelper
    {
        public static ObjectArrayBackedEventBean GetDefineMultimatchBean(
            StatementContext statementContext,
            LinkedHashMap<String, Pair<int, bool>> variableStreams,
            EventType parentViewType)
        {
            IDictionary<String, Object> multievent = new LinkedHashMap<String, Object>();
            foreach (var entry in variableStreams)
            {
                if (entry.Value.Second)
                {
                    multievent.Put(entry.Key, new EventType[] {parentViewType});
                }
            }
            var multimatch = statementContext.EventAdapterService.CreateAnonymousObjectArrayType(
                    "esper_matchrecog_internal", multievent);
            return (ObjectArrayBackedEventBean) statementContext.EventAdapterService.AdapterForTypedObjectArray(new Object[multievent.Count], multimatch);
        }

        public static StreamTypeService BuildDefineStreamTypeServiceDefine(
            StatementContext statementContext,
            LinkedHashMap<String, Pair<int, bool>> variableStreams,
            MatchRecognizeDefineItem defineItem,
            IDictionary<String, ISet<String>> visibilityByIdentifier,
            EventType parentViewType)
        {
            if (!variableStreams.ContainsKey(defineItem.Identifier))
            {
                throw new ExprValidationException("Variable '" + defineItem.Identifier + "' does not occur in pattern");
            }

            var streamNamesDefine = new String[variableStreams.Count + 1];
            var typesDefine = new EventType[variableStreams.Count + 1];
            var isIStreamOnly = new bool[variableStreams.Count + 1];
            CompatExtensions.Fill(isIStreamOnly, true);

            var streamNumDefine = variableStreams.Get(defineItem.Identifier).First;
            streamNamesDefine[streamNumDefine] = defineItem.Identifier;
            typesDefine[streamNumDefine] = parentViewType;

            // add visible single-value
            var visibles = visibilityByIdentifier.Get(defineItem.Identifier);
            var hasVisibleMultimatch = false;
            if (visibles != null)
            {
                foreach (var visible in visibles)
                {
                    var def = variableStreams.Get(visible);
                    if (!def.Second)
                    {
                        streamNamesDefine[def.First] = visible;
                        typesDefine[def.First] = parentViewType;
                    }
                    else
                    {
                        hasVisibleMultimatch = true;
                    }
                }
            }

            // compile multi-matching event type (in last position), if any are used
            if (hasVisibleMultimatch)
            {
                IDictionary<String, Object> multievent = new LinkedHashMap<String, Object>();
                foreach (var entry in variableStreams)
                {
                    var identifier = entry.Key;
                    if (entry.Value.Second)
                    {
                        if (visibles.Contains(identifier))
                        {
                            multievent.Put(
                                identifier, new EventType[]
                                {
                                    parentViewType
                                });
                        }
                        else
                        {
                            multievent.Put("esper_matchrecog_internal", null);
                        }
                    }
                }
                var multimatch = statementContext.EventAdapterService.CreateAnonymousObjectArrayType(
                    "esper_matchrecog_internal", multievent);
                typesDefine[typesDefine.Length - 1] = multimatch;
                streamNamesDefine[streamNamesDefine.Length - 1] = multimatch.Name;
            }

            return new StreamTypeServiceImpl(
                typesDefine, streamNamesDefine, isIStreamOnly, statementContext.EngineURI, false);
        }
    }
}
