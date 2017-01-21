///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Event row regular expressions are organized into a tree-like structure with nodes
    /// representing sub-expressions.
    /// </summary>

    [Serializable]
    public class MatchRecognizeRegExAtom : MatchRecognizeRegEx
    {
        /// <summary>Ctor. </summary>
        public MatchRecognizeRegExAtom() {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="name">of variable</param>
        /// <param name="type">multiplicity</param>
        public MatchRecognizeRegExAtom(String name, MatchRecognizePatternElementType type) {
            Name = name;
            ElementType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRecognizeRegExAtom"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="optionalRepeat">The optional repeat.</param>
        public MatchRecognizeRegExAtom(String name, MatchRecognizePatternElementType type, MatchRecognizeRegExRepeat optionalRepeat)
        {
            Name = name;
            ElementType = type;
            OptionalRepeat = optionalRepeat;
        }

        /// <summary>Returns variable name. </summary>
        /// <returns>name</returns>
        public string Name { get; set; }

        /// <summary>Returns multiplicity. </summary>
        /// <returns>multiplicity</returns>
        public MatchRecognizePatternElementType ElementType { get; set; }

        /// <summary>Returns the optional repeat.</summary>
        /// <value>The optional repeat.</value>
        public MatchRecognizeRegExRepeat OptionalRepeat { get; set; }

        public override void WriteEPL(TextWriter writer) {
            writer.Write(Name);
            writer.Write(ElementType.GetText());
            if (OptionalRepeat != null)
            {
                OptionalRepeat.WriteEPL(writer);
            }
        }
    }
}
