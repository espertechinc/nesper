///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
	/// <summary>
	/// Parser similar in structure to:
	/// http://cogitolearning.co.uk/docs/cogpar/files.html
	/// </summary>
	public class ClassDescriptorParser
	{
		private static readonly ClassDescriptorTokenizer Tokenizer;

		static ClassDescriptorParser()
		{
			Tokenizer = new ClassDescriptorTokenizer();
			Tokenizer.Add("([a-zA-Z_$][a-zA-Z\\d_$]*\\.)*[a-zA-Z_$][a-zA-Z\\d_$]*", ClassDescriptorTokenType.IDENTIFIER);
			Tokenizer.Add("\\[", ClassDescriptorTokenType.LEFT_BRACKET);
			Tokenizer.Add("\\]", ClassDescriptorTokenType.RIGHT_BRACKET);
			Tokenizer.Add("<", ClassDescriptorTokenType.LESSER_THAN);
			Tokenizer.Add(",", ClassDescriptorTokenType.COMMA);
			Tokenizer.Add(">", ClassDescriptorTokenType.GREATER_THAN);
		}

		internal static ClassDescriptor Parse(string classIdent)
		{
			try {
				var tokens = Tokenizer.Tokenize(classIdent);
				var parser = new ClassDescriptorParserWalker(tokens);
				return parser.Walk(false);
			}
			catch (ClassDescriptorParseException ex) {
				var trueType = TypeHelper.ResolveType(classIdent, false);
				if (trueType != null) {
					return new ClassDescriptor(trueType);
				}

				throw new EPException("Failed to parse class identifier '" + classIdent + "': " + ex.Message, ex);
			}
		}
	}
} // end of namespace
