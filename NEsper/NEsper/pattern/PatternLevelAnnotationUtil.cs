///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.pattern
{
    public class PatternLevelAnnotationUtil {
    
        private static readonly string DISCARDPARTIALSONMATCH = "DiscardPartialsOnMatch";
        private static readonly string SUPPRESSOVERLAPPINGMATCHES = "SuppressOverlappingMatches";
    
        public static AnnotationPart[] AnnotationsFromSpec(PatternStreamSpecRaw pattern) {
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
    
            if (parts == null) {
                return null;
            }
            return Parts.ToArray(new AnnotationPart[parts.Count]);
        }
    
        public static PatternLevelAnnotationFlags AnnotationsToSpec(AnnotationPart[] parts) {
            var flags = new PatternLevelAnnotationFlags();
            if (parts == null) {
                return flags;
            }
            foreach (AnnotationPart part in parts) {
                ValidateSetFlags(flags, part.Name);
            }
            return flags;
        }
    
        public static void ValidateSetFlags(PatternLevelAnnotationFlags flags, string annotation) {
            if (annotation.ToLowerInvariant().Equals(DISCARDPARTIALSONMATCH.ToLowerInvariant())) {
                flags.DiscardPartialsOnMatch = true;
            } else if (annotation.ToLowerInvariant().Equals(SUPPRESSOVERLAPPINGMATCHES.ToLowerInvariant())) {
                flags.SuppressSameEventMatches = true;
            } else {
                throw new ArgumentException("Unrecognized pattern-level annotation '" + annotation + "'");
            }
        }
    }
} // end of namespace
