///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Reflection;

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compiler.@internal.parse
{
	public class Antlr4ErrorListener : BaseErrorListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Antlr4ErrorListener INSTANCE = new Antlr4ErrorListener();

		private Antlr4ErrorListener()
		{
		}

		public void SyntaxError(
			TextWriter output,
			IRecognizer recognizer,
			object offendingSymbol,
			int line,
			int charPositionInLine,
			string msg,
			RecognitionException e)
		{
			throw e;
		}

		public override void ReportAmbiguity(
			Parser recognizer,
			DFA dfa,
			int startIndex,
			int stopIndex,
			bool exact,
			BitSet ambigAlts,
			ATNConfigSet configs)
		{
			Log.Debug("ReportAmbiguity");
		}

		public override void ReportAttemptingFullContext(
			Parser recognizer,
			DFA dfa,
			int startIndex,
			int stopIndex,
			BitSet conflictingAlts,
			ATNConfigSet configs)
		{
			Log.Debug("ReportAttemptingFullContext");
		}


		public override void ReportContextSensitivity(
			Parser recognizer,
			DFA dfa,
			int startIndex,
			int stopIndex,
			int prediction,
			ATNConfigSet configs)
		{
			Log.Debug("ReportContextSensitivity");
		}
	}
} // end of namespace
