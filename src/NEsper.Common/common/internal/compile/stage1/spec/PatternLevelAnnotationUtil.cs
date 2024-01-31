///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class PatternLevelAnnotationUtil
    {
        private const string DISCARDPARTIALSONMATCH = "DiscardPartialsOnMatch";
        private const string SUPPRESSOVERLAPPINGMATCHES = "SuppressOverlappingMatches";

        public static AnnotationPart[] AnnotationsFromSpec(PatternStreamSpecRaw pattern)
        {
            Deque<AnnotationPart> parts = null;

            if (pattern.IsDiscardPartialsOnMatch) {
                parts = new ArrayDeque<AnnotationPart>();
                parts.Add(new AnnotationPart(DISCARDPARTIALSONMATCH));
            }

            if (pattern.IsSuppressSameEventMatches) {
                if (parts == null) {
                    parts = new ArrayDeque<AnnotationPart>();
                }

                parts.Add(new AnnotationPart(SUPPRESSOVERLAPPINGMATCHES));
            }

            return parts?.ToArray();
        }

        public static PatternLevelAnnotationFlags AnnotationsToSpec(AnnotationPart[] parts)
        {
            var flags = new PatternLevelAnnotationFlags();
            if (parts == null) {
                return flags;
            }

            foreach (var part in parts) {
                ValidateSetFlags(flags, part.Name);
            }

            return flags;
        }

        public static void ValidateSetFlags(
            PatternLevelAnnotationFlags flags,
            string annotation)
        {
            if (string.Equals(annotation, DISCARDPARTIALSONMATCH, StringComparison.InvariantCultureIgnoreCase)) {
                flags.IsDiscardPartialsOnMatch = true;
            }
            else if (string.Equals(
                         annotation,
                         SUPPRESSOVERLAPPINGMATCHES,
                         StringComparison.InvariantCultureIgnoreCase)) {
                flags.IsSuppressSameEventMatches = true;
            }
            else {
                throw new ArgumentException("Unrecognized pattern-level annotation '" + annotation + "'");
            }
        }
    }
} // end of namespace