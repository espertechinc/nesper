///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
	/// <summary>
	/// Parser similar in structure to:
	/// http://cogitolearning.co.uk/docs/cogpar/files.html
	/// </summary>
	public class PropertyParserNoDep {
	    private static Tokenizer tokenizer;

	    static PropertyParserNoDep()
	    {
	        tokenizer = new Tokenizer();
	        tokenizer.Add("[a-zA-Z]([a-zA-Z0-9_]|\\\\.)*", TokenType.IDENT);
	        tokenizer.Add("`[^`]*`", TokenType.IDENTESCAPED);
	        tokenizer.Add("[0-9]+", TokenType.NUMBER);
	        tokenizer.Add("\\[", TokenType.LBRACK);
	        tokenizer.Add("\\]", TokenType.RBRACK);
	        tokenizer.Add("\\(", TokenType.LPAREN);
	        tokenizer.Add("\\)", TokenType.RPAREN);
	        tokenizer.Add("\"([^\\\\\"]|\\\\\\\\|\\\\\")*\"", TokenType.DOUBLEQUOTEDLITERAL);
	        tokenizer.Add("\'([^\\']|\\\\\\\\|\\')*\'", TokenType.SINGLEQUOTEDLITERAL);
	        tokenizer.Add("\\.", TokenType.DOT);
	        tokenizer.Add("\\?", TokenType.QUESTION);
	    }

	    public static Property ParseAndWalkLaxToSimple(string expression, bool rootedDynamic) {
	        try {
	            ArrayDeque<Token> tokens = tokenizer.Tokenize(expression);
	            PropertyTokenParser parser = new PropertyTokenParser(tokens, rootedDynamic);
	            return parser.Property();
	        } catch (PropertyParseNodepException ex) {
	            throw new PropertyAccessException("Failed to parse property '" + expression + "': " + ex.Message, ex);
	        }
	    }

	    /// <summary>
	    /// Parse the mapped property into classname, method and string argument.
	    /// Mind this has been parsed already and is a valid mapped property.
	    /// </summary>
	    /// <param name="property">is the string property to be passed as a static method invocation</param>
	    /// <returns>descriptor object</returns>
	    public static MappedPropertyParseResult ParseMappedProperty(string property) {
	        // get argument
	        int indexFirstDoubleQuote = property.IndexOf("\"");
	        int indexFirstSingleQuote = property.IndexOf("'");
	        int startArg;
	        if ((indexFirstSingleQuote == -1) && (indexFirstDoubleQuote == -1)) {
	            return null;
	        }
	        if ((indexFirstSingleQuote != -1) && (indexFirstDoubleQuote != -1)) {
	            if (indexFirstSingleQuote < indexFirstDoubleQuote) {
	                startArg = indexFirstSingleQuote;
	            } else {
	                startArg = indexFirstDoubleQuote;
	            }
	        } else if (indexFirstSingleQuote != -1) {
	            startArg = indexFirstSingleQuote;
	        } else {
	            startArg = indexFirstDoubleQuote;
	        }

	        int indexLastDoubleQuote = property.LastIndexOf("\"");
	        int indexLastSingleQuote = property.LastIndexOf("'");
	        int endArg;
	        if ((indexLastSingleQuote == -1) && (indexLastDoubleQuote == -1)) {
	            return null;
	        }
	        if ((indexLastSingleQuote != -1) && (indexLastDoubleQuote != -1)) {
	            if (indexLastSingleQuote > indexLastDoubleQuote) {
	                endArg = indexLastSingleQuote;
	            } else {
	                endArg = indexLastDoubleQuote;
	            }
	        } else if (indexLastSingleQuote != -1) {
	            if (indexLastSingleQuote == indexFirstSingleQuote) {
	                return null;
	            }
	            endArg = indexLastSingleQuote;
	        } else {
	            if (indexLastDoubleQuote == indexFirstDoubleQuote) {
	                return null;
	            }
	            endArg = indexLastDoubleQuote;
	        }
	        string argument = property.Substring(startArg + 1, endArg);

	        // get method
	        string[] splitDots = property.Split("[\\.]");
	        if (splitDots.Length == 0) {
	            return null;
	        }

	        // find which element represents the method, its the element with the parenthesis
	        int indexMethod = -1;
	        for (int i = 0; i < splitDots.Length; i++) {
	            if (splitDots[i].Contains("(")) {
	                indexMethod = i;
	                break;
	            }
	        }
	        if (indexMethod == -1) {
	            return null;
	        }

	        string method = splitDots[indexMethod];
	        int indexParan = method.IndexOf("(");
	        method = method.Substring(0, indexParan);
	        if (method.Length == 0) {
	            return null;
	        }

	        if (splitDots.Length == 1) {
	            // no class name
	            return new MappedPropertyParseResult(null, method, argument);
	        }

	        // get class
	        StringBuilder clazz = new StringBuilder();
	        for (int i = 0; i < indexMethod; i++) {
	            if (i > 0) {
	                clazz.Append('.');
	            }
	            clazz.Append(splitDots[i]);
	        }

	        return new MappedPropertyParseResult(clazz.ToString(), method, argument);
	    }
	}
} // end of namespace