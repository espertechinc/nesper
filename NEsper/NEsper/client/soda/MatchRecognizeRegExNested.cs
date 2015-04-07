///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Atom representing an expression for use in match-recognize.
    /// <para/>
    /// Event row regular expressions are organized into a tree-like structure
    /// with nodes representing sub-expressions.
    /// </summary>
    [Serializable]
    public class MatchRecognizeRegExNested
        : MatchRecognizeRegEx
    {
        /// <summary>Ctor. </summary>
        public MatchRecognizeRegExNested()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="type">multiplicity</param>
        public MatchRecognizeRegExNested(MatchRecognizePatternElementType type)
        {
            ElementType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRecognizeRegExNested"/> class.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="optionalRepeat">The optional repeat.</param>
        public MatchRecognizeRegExNested(MatchRecognizePatternElementType elementType, MatchRecognizeRegExRepeat optionalRepeat)
        {
            ElementType = elementType;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>Returns multiplicity. </summary>
        /// <returns>multiplicity</returns>
        public MatchRecognizePatternElementType ElementType { get; set; }

        /// <summary>Returns the optional repeat.</summary>
        /// <value>The optional repeat.</value>
        public MatchRecognizeRegExRepeat OptionalRepeat { get; set; }

        public override void WriteEPL(TextWriter writer)
        {
            writer.Write("(");
            String delimiter = "";
            foreach (MatchRecognizeRegEx node in Children)
            {
                writer.Write(delimiter);
                node.WriteEPL(writer);
                delimiter = " ";
            }
            writer.Write(")");
            writer.Write(ElementType.GetText());
        
            if (OptionalRepeat != null)
            {
                OptionalRepeat.WriteEPL(writer);
            }
        }
    }
}
