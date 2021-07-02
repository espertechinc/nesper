///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
	public class ClassDescriptorTokenizer
	{

		private readonly LinkedList<ClassDescriptorTokenInfo> tokens = new LinkedList<ClassDescriptorTokenInfo>();

		public void Add(
			string pattern,
			ClassDescriptorTokenType token)
		{
			var patternText = "^(" + pattern + ")";
			var patternExpr = new Regex(patternText, RegexOptions.None); 
			tokens.AddLast(new ClassDescriptorTokenInfo(patternExpr, token));
		}

		public ArrayDeque<ClassDescriptorToken> Tokenize(string str)
		{
			var tokens = new ArrayDeque<ClassDescriptorToken>();
			while (str != string.Empty) {
				var match = false;
				foreach (var info in this.tokens) {
					var m = info.Regex.Match(str);
					if (m != Match.Empty) {
						match = true;

						var tok = m.Value.Trim();
						tokens.Add(new ClassDescriptorToken(info.Token, tok));

						str = str.Remove(m.Index, m.Length).Trim();
						//str = m.ReplaceFirst("").Trim();
						break;
					}
				}

				if (!match) {
					throw new ClassDescriptorParseException("Unexpected token '" + str + "'");
				}
			}

			return tokens;
		}
	}
} // end of namespace
