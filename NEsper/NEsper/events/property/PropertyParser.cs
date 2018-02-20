///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.parse;
using com.espertech.esper.type;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Parser for property names that can be simple, nested, mapped or a combination of these.
    /// Uses ANTLR parser to parse.
    /// </summary>
    public class PropertyParser
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ILockable StaticLock = new MonitorSlimLock(60000);
        private static ISet<string> _keywordCache;

        public static Property ParseAndWalk(string property, bool isRootedDynamic)
        {
            return Walk(Parse(property), isRootedDynamic);
        }

        /// <summary>
        /// Parses property.
        /// For cases when the property is not following the property syntax assume we act lax and assume its a simple property.
        /// </summary>
        /// <param name="property">property to parse</param>
        /// <returns>property or SimpleProperty if the property cannot be parsed</returns>
        public static Property ParseAndWalkLaxToSimple(String property)
        {
            try
            {
                return Walk(Parse(property), false);
            }
            catch (PropertyAccessException)
            {
                return new SimpleProperty(property);
            }
        }

        /// <summary>
        /// Parse the given property name returning a Property instance for the property.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="isRootedDynamic">is true to indicate that the property is already rooted in a dynamicproperty and therefore all child properties should be dynamic properties as well</param>
        /// <returns>
        /// Property instance for property
        /// </returns>
        public static Property Walk(EsperEPL2GrammarParser.StartEventPropertyRuleContext tree, bool isRootedDynamic)
        {
            if (tree.eventProperty().eventPropertyAtomic().Length == 1)
            {
                return MakeProperty(tree.eventProperty().eventPropertyAtomic(0), isRootedDynamic);
            }

            var propertyRoot = tree.eventProperty();

            IList<Property> properties = new List<Property>();
            var isRootedInDynamic = isRootedDynamic;
            foreach (var atomic in propertyRoot.eventPropertyAtomic())
            {
                var property = MakeProperty(atomic, isRootedInDynamic);
                if (property is DynamicSimpleProperty)
                {
                    isRootedInDynamic = true;
                }
                properties.Add(property);
            }
            return new NestedProperty(properties);
        }

        /// <summary>
        /// Parses a given property name returning an AST.
        /// </summary>
        /// <param name="propertyName">to parse</param>
        /// <returns>AST syntax tree</returns>
        public static EsperEPL2GrammarParser.StartEventPropertyRuleContext Parse(string propertyName)
        {
            ICharStream input;
            try
            {
                input = new NoCaseSensitiveStream(propertyName);
            }
            catch (IOException ex)
            {
                throw new PropertyAccessException("IOException parsing property name '" + propertyName + '\'', ex);
            }

            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);
            try
            {
                tokens.Fill();
            }
            catch (Exception e)
            {
                if (ParseHelper.HasControlCharacters(propertyName))
                {
                    throw new PropertyAccessException("Unrecognized control characters found in text");
                }
                throw new PropertyAccessException("Failed to parse text: " + e.Message);
            }

            var g = ParseHelper.NewParser(tokens);
            EsperEPL2GrammarParser.StartEventPropertyRuleContext r;

            try
            {
                r = g.startEventPropertyRule();
            }
            catch (RecognitionException e)
            {
                return HandleRecognitionEx(e, tokens, propertyName, g);
            }
            catch (Exception e)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Error parsing property expression [" + propertyName + "]", e);
                }
                if (e.InnerException is RecognitionException)
                {
                    return HandleRecognitionEx((RecognitionException)e.InnerException, tokens, propertyName, g);
                }
                else
                {
                    throw;
                }
            }

            return r;
        }

        private static EsperEPL2GrammarParser.StartEventPropertyRuleContext HandleRecognitionEx(RecognitionException e, CommonTokenStream tokens, string propertyName, EsperEPL2GrammarParser g)
        {
            // Check for keywords and escape each, parse again
            var escapedPropertyName = EscapeKeywords(tokens);

            ICharStream inputEscaped;
            try
            {
                inputEscaped = new NoCaseSensitiveStream(escapedPropertyName);
            }
            catch (IOException ex)
            {
                throw new PropertyAccessException("IOException parsing property name '" + propertyName + '\'', ex);
            }

            var lexEscaped = ParseHelper.NewLexer(inputEscaped);
            var tokensEscaped = new CommonTokenStream(lexEscaped);
            var gEscaped = ParseHelper.NewParser(tokensEscaped);

            try
            {
                return gEscaped.startEventPropertyRule();
            }
            catch
            {
            }

            throw ExceptionConvertor.ConvertProperty(e, propertyName, true, g);
        }

        private static string EscapeKeywords(CommonTokenStream tokens)
        {
            using (StaticLock.Acquire())
            {
                if (_keywordCache == null)
                {
                    _keywordCache = new HashSet<string>();
                    var keywords = ParseHelper.NewParser(tokens).GetKeywords();
                    foreach (var keyword in keywords)
                    {
                        if (keyword[0] == '\'' && keyword[keyword.Length - 1] == '\'')
                        {
                            _keywordCache.Add(keyword.Substring(1, keyword.Length - 2));
                        }
                    }
                }

                var writer = new StringWriter();
                foreach (var token in tokens.GetTokens()) // Call getTokens first before invoking tokens.size! ANTLR problem
                {
                    if (token.Type == EsperEPL2GrammarLexer.Eof)
                    {
                        break;
                    }
                    var isKeyword = _keywordCache.Contains(token.Text.ToLowerInvariant());
                    if (isKeyword)
                    {
                        writer.Write('`');
                        writer.Write(token.Text);
                        writer.Write('`');
                    }
                    else
                    {
                        writer.Write(token.Text);
                    }
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Returns true if the property is a dynamic property.
        /// </summary>
        /// <param name="ast">property ast</param>
        /// <returns>dynamic or not</returns>
        public static bool IsPropertyDynamic(EsperEPL2GrammarParser.StartEventPropertyRuleContext ast)
        {
            IList<EsperEPL2GrammarParser.EventPropertyAtomicContext> ctxs = ast.eventProperty().eventPropertyAtomic();
            return ctxs.Any(ctx => ctx.q != null || ctx.q1 != null);
        }

        private static Property MakeProperty(EsperEPL2GrammarParser.EventPropertyAtomicContext atomic, bool isRootedInDynamic)
        {
            var prop = ASTUtil.UnescapeDot(atomic.eventPropertyIdent().GetText());
            if (prop.Length == 0)
            {
                throw new PropertyAccessException("Invalid zero-length string provided as an event property name");
            }
            if (atomic.lb != null)
            {
                var index = IntValue.ParseString(atomic.ni.GetText());
                if (!isRootedInDynamic && atomic.q == null)
                {
                    return new IndexedProperty(prop, index);
                }
                else
                {
                    return new DynamicIndexedProperty(prop, index);
                }
            }
            else if (atomic.lp != null)
            {
                var key = StringValue.ParseString(atomic.s.Text);
                if (!isRootedInDynamic && atomic.q == null)
                {
                    return new MappedProperty(prop, key);
                }
                else
                {
                    return new DynamicMappedProperty(prop, key);
                }
            }
            else
            {
                if (!isRootedInDynamic && atomic.q1 == null)
                {
                    return new SimpleProperty(prop);
                }
                else
                {
                    return new DynamicSimpleProperty(prop);
                }
            }
        }

        public static string UnescapeBacktick(string unescapedPropertyName)
        {
            if (unescapedPropertyName.StartsWith("`") && unescapedPropertyName.EndsWith("`"))
            {
                return unescapedPropertyName.Substring(1, unescapedPropertyName.Length - 2);
            }

            if (!unescapedPropertyName.Contains("`"))
            {
                return unescapedPropertyName;
            }

            // parse and render
            var property = PropertyParser.ParseAndWalkLaxToSimple(unescapedPropertyName);
            if (property is NestedProperty)
            {
                var writer = new StringWriter();
                property.ToPropertyEPL(writer);
                return writer.ToString();
            }

            return unescapedPropertyName;
        }

        public static bool IsNestedPropertyWithNonSimpleLead(EsperEPL2GrammarParser.EventPropertyContext ctx)
        {
            if (ctx.eventPropertyAtomic().Length == 1)
            {
                return false;
            }
            var atomic = ctx.eventPropertyAtomic()[0];
            return atomic.lb != null || atomic.lp != null || atomic.q1 != null;
        }
    }
}
