///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

namespace com.espertech.esper.grammar.@internal.util
{
    public class Antlr4ErrorStrategy : BailErrorStrategy
    {
        public override void ReportError(
            Parser recognizer,
            RecognitionException e)
        {
            // Antlr has an issue handling LexerNoViableAltException as then offending token can be null
            // Try: "select a.b('aa\") from A"
            if (e is LexerNoViableAltException && e.OffendingToken == null)
            {
                return;
            }

            base.ReportError(recognizer, e);
        }
    }
} // end of namespace