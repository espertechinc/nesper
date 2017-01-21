///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.pattern
{
    public class PatternLevelAnnotationUtil
    {
        private readonly static String DISCARDPARTIALSONMATCH = "DiscardPartialsOnMatch";
        private readonly static String SUPPRESSOVERLAPPINGMATCHES = "SuppressOverlappingMatches";
    
        public static AnnotationPart[] AnnotationsFromSpec(PatternStreamSpecRaw pattern) {
            ArrayDeque<AnnotationPart> parts = null;
    
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
            return parts.ToArray();
        }

        public static PatternLevelAnnotationFlags AnnotationsToSpec(AnnotationPart[] parts)
        {
            PatternLevelAnnotationFlags flags = new PatternLevelAnnotationFlags();
            if (parts == null) {
                return flags;
            }
            foreach (AnnotationPart part in parts)
            {
                ValidateSetFlags(flags, part.Name);
            }
            return flags;
        }
    
        public static void ValidateSetFlags(PatternLevelAnnotationFlags flags, String annotation) {
            if (annotation.ToLower().Equals(DISCARDPARTIALSONMATCH.ToLower())) {
                flags.IsDiscardPartialsOnMatch = true;
            }
            else if (annotation.ToLower().Equals(SUPPRESSOVERLAPPINGMATCHES.ToLower())) {
                flags.IsSuppressSameEventMatches = true;
            }
            else {
                throw new ArgumentException("Unrecognized pattern-level annotation '" + annotation + "'");
            }
        }
    }
}
