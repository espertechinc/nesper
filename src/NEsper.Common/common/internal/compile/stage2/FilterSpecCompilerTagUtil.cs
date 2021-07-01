///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerTagUtil
    {
        public static ISet<string> AssignEventAsTagNumber(
            ISet<string> priorAllTags,
            EvalForgeNode evalForgeNode)
        {
            var allTagNamesOrdered = new LinkedHashSet<string>();
            var filterFactoryNodes = EvalNodeUtil.RecursiveGetChildNodes(
                evalForgeNode,
                StreamSpecCompiler.FilterForFilterFactoryNodes.INSTANCE);
            if (priorAllTags != null) {
                allTagNamesOrdered.AddAll(priorAllTags);
            }

            foreach (var filterNode in filterFactoryNodes) {
                var forge = (EvalFilterForgeNode) filterNode;
                int tagNumber;
                if (forge.EventAsName != null) {
                    if (!allTagNamesOrdered.Contains(forge.EventAsName)) {
                        allTagNamesOrdered.Add(forge.EventAsName);
                        tagNumber = allTagNamesOrdered.Count - 1;
                    }
                    else {
                        tagNumber = FindTagNumber(forge.EventAsName, allTagNamesOrdered);
                    }

                    forge.EventAsTagNumber = tagNumber;
                }
            }

            return allTagNamesOrdered;
        }

        public static ISet<string> GetTagNumbers(EvalForgeNode evalForgeNode)
        {
            var tags = new HashSet<string>();
            var filterFactoryNodes = EvalNodeUtil.RecursiveGetChildNodes(
                evalForgeNode,
                StreamSpecCompiler.FilterForFilterFactoryNodes.INSTANCE);
            foreach (var filterNode in filterFactoryNodes) {
                var forge = (EvalFilterForgeNode) filterNode;
                if (forge.EventAsName != null) {
                    tags.Add(forge.EventAsName);
                }
            }

            return tags;
        }

        public static int FindTagNumber(
            string findTag,
            ISet<string> allTagNamesOrdered)
        {
            var index = 0;
            foreach (var tag in allTagNamesOrdered) {
                if (findTag.Equals(tag)) {
                    return index;
                }

                index++;
            }

            throw new EPException("Failed to find tag '" + findTag + "' among known tags");
        }
    }
} // end of namespace