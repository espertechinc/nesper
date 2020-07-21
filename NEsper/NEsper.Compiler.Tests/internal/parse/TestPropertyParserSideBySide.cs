///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.grammar.@internal.generated;
using com.espertech.esper.grammar.@internal.util;

using NUnit.Framework;

using static com.espertech.esper.grammar.@internal.generated.EsperEPL2GrammarParser;

namespace com.espertech.esper.compiler.@internal.parse
{
    [TestFixture]
	public class TestPropertyParserSideBySide  {
		private static object keywordCacheLock = new object();
	    private static ISet<string> keywordCache;

        [Test]
	    public void TestParse() {
	        RunAssertion("prop", new SimplePropAssertion("prop"));
	        RunAssertion("a[1]", new IndexedPropAssertion("a", 1));
	        RunAssertion("a(\"key\")", new MappedPropAssertion("a", "key"));
	        RunAssertion("a('key')", new MappedPropAssertion("a", "key"));
	        RunAssertion("a.b", new NestedPropAssertion(new SimplePropAssertion("a"), new SimplePropAssertion("b")));
	        RunAssertion("prop?", new SimplePropAssertion("prop", true));
	        RunAssertion("a[1]?", new IndexedPropAssertion("a", 1, true));
	        RunAssertion("a('key')?", new MappedPropAssertion("a", "key", true));
	        RunAssertion("item?.id", new NestedPropAssertion(new SimplePropAssertion("item", true), new SimplePropAssertion("id", true)));
	        RunAssertion("item[0]?.id", new NestedPropAssertion(new IndexedPropAssertion("item", 0, true), new SimplePropAssertion("id")));
	        RunAssertion("item('a')?.id", new NestedPropAssertion(new MappedPropAssertion("item", "a", true), new SimplePropAssertion("id")));
	    }

	    private void RunAssertion(
		    string expression,
		    IPropAssertion propAssertion)
	    {
		    RunAssertion(expression, propAssertion.Accept);
	    }

	    private void RunAssertion(string expression, Consumer<Property> consumer) {
	        var antlr = AntlrParseAndWalk(expression, false);
	        consumer.Invoke(antlr);

	        var nodep = PropertyParser.ParseAndWalkLaxToSimple(expression);
	        consumer.Invoke(nodep);
	    }

	    public static Property AntlrParseAndWalk(string property, bool isRootedDynamic) {
	        return Walk(Parse(property));
	    }

	    public static StartEventPropertyRuleContext Parse(string propertyName) {
	        var input = new CaseInsensitiveInputStream(propertyName);
	        var lex = ParseHelper.NewLexer(input);
	        var tokens = new CommonTokenStream(lex);
	        try {
	            tokens.Fill();
	        } catch (Exception e) {
	            if (ParseHelper.HasControlCharacters(propertyName)) {
	                throw new PropertyAccessException("Unrecognized control characters found in text");
	            }
	            throw new PropertyAccessException("Failed to parse text: " + e.Message);
	        }

	        var g = ParseHelper.NewParser(tokens);
	        StartEventPropertyRuleContext r;

	        try {
	            r = g.startEventPropertyRule();
	        } catch (RecognitionException e) {
	            return HandleRecognitionEx(e, tokens, propertyName, g);
	        } catch (Exception e) {
	            if (e.InnerException is RecognitionException re) {
	                return HandleRecognitionEx(re, tokens, propertyName, g);
	            } else {
	                throw;
	            }
	        }

	        return r;
	    }

	    private static StartEventPropertyRuleContext HandleRecognitionEx(
		    RecognitionException e,
		    CommonTokenStream tokens,
		    string propertyName,
		    EsperEPL2GrammarParser g)
	    {
		    // Check for keywords and escape each, parse again
		    var escapedPropertyName = EscapeKeywords(tokens);

		    var inputEscaped = new CaseInsensitiveInputStream(escapedPropertyName);
		    var lexEscaped = ParseHelper.NewLexer(inputEscaped);
		    var tokensEscaped = new CommonTokenStream(lexEscaped);
		    var gEscaped = ParseHelper.NewParser(tokensEscaped);

		    try {
			    return gEscaped.startEventPropertyRule();
		    }
		    catch (Exception eEscaped) {
		    }

		    throw ExceptionConvertor.ConvertProperty(e, propertyName, true, g);
	    }

	    private static HashSet<string> CreateKeywordCache(ITokenStream tokens)
	    {
		    var myKeywordCache = new HashSet<string>();
		    var myKeywords = ParseHelper.NewParser(tokens).GetKeywords();

		    foreach (var keyword in myKeywords) {
			    if (keyword[0] == '\'' && keyword[keyword.Length - 1] == '\'') {
				    myKeywordCache.Add(keyword.Substring(1, keyword.Length - 1));
			    }
		    }

		    return myKeywordCache;
	    }

	    private static string EscapeKeywords(CommonTokenStream tokens) {
		    lock (keywordCacheLock) {
			    if (keywordCache == null) {
				    keywordCache = CreateKeywordCache(tokens);
			    }
		    }

		    var writer = new StringWriter();
		    // Call getTokens first before invoking tokens.size! ANTLR problem
		    foreach (IToken t in tokens.GetTokens()) {
			    if (t.Type == EsperEPL2GrammarLexer.Eof) {
				    break;
			    }

			    var isKeyword = keywordCache.Contains(t.Text.ToLowerInvariant());
			    if (isKeyword) {
				    writer.Write('`');
				    writer.Write(t.Text);
				    writer.Write('`');
			    }
			    else {
				    writer.Write(t.Text);
			    }
		    }

		    return writer.ToString();
	    }

	    /// <summary>
	    /// Parse the given property name returning a Property instance for the property.
	    /// </summary>
	    /// <param name="tree">tree</param>
	    /// <returns>Property instance for property</returns>
	    public static Property Walk(StartEventPropertyRuleContext tree) {
	        // handle root
	        var root = tree.chainable().chainableRootWithOpt();
	        var rootProp = root.chainableWithArgs();

	        IList<ChainableAtomicWithOptContext> chained = tree.chainable().chainableElements().chainableAtomicWithOpt();
	        IList<Property> properties = new List<Property>();
	        var optionalRoot = root.q != null;
	        var property = WalkProp(rootProp, chained.IsEmpty() ? null : chained[0], optionalRoot, false);
	        properties.Add(property);
	        var rootedDynamic = property is DynamicSimpleProperty;

	        for (var i = 0; i < chained.Count; i++) {
	            var ctx = chained[i];
	            if (ctx.chainableAtomic().chainableArray() != null) {
	                continue;
	            }
	            var optional = ctx.q != null;
	            property = WalkProp(ctx.chainableAtomic().chainableWithArgs(), chained.Count <= i+1 ? null : chained[i + 1], optional, rootedDynamic);
	            properties.Add(property);
	        }

	        if (properties.Count == 1) {
	            return properties[0];
	        }
	        return new NestedProperty(properties);
	    }

	    private static Property WalkProp(ChainableWithArgsContext ctx, ChainableAtomicWithOptContext nextOrNull, bool optional, bool rootedDynamic) {
	        if (nextOrNull == null) {
	            return MakeProperty(ctx, optional, rootedDynamic);
	        }

	        var name = ctx.chainableIdent().GetText();
	        if (nextOrNull.chainableAtomic().chainableArray() != null) {
	            var indexText = nextOrNull.chainableAtomic().chainableArray().expression(0).GetText();
	            var index = int.Parse(indexText);
	            optional |= nextOrNull.q != null;
	            return optional ? (Property) new DynamicIndexedProperty(name, index) : new IndexedProperty(name, index);
	        }
	        else {
	            return MakeProperty(ctx, optional, rootedDynamic);
	        }
	    }

	    private static Property MakeProperty(
		    ChainableWithArgsContext ctx,
		    bool optional,
		    bool rootedDynamic)
	    {
		    var name = ctx.chainableIdent().GetText();
		    if (ctx.lp == null) {
			    return optional | rootedDynamic ? (Property) new DynamicSimpleProperty(name) : new SimpleProperty(name);
		    }

		    var func = ctx.libFunctionArgs().libFunctionArgItem()[0];
		    var key = StringValue.ParseString(func.GetText());
		    return optional ? (Property) new DynamicMappedProperty(name, key) : new MappedProperty(name, key);
	    }

	    private interface IPropAssertion
	    {
		    void Accept(Property property);
	    }
	    
	    private class SimplePropAssertion : IPropAssertion
	    {
	        private readonly string name;
	        private readonly bool dynamic;

	        public SimplePropAssertion(string name) : this(name, false) {
	        }

	        public SimplePropAssertion(string name, bool dynamic) {
	            this.name = name;
	            this.dynamic = dynamic;
	        }

	        public void Accept(Property property) {
	            if (dynamic) {
	                var dyn = (DynamicSimpleProperty) property;
	                Assert.AreEqual(name, dyn.PropertyNameAtomic);
	            } else {
	                var prop = (SimpleProperty) property;
	                Assert.AreEqual(name, prop.PropertyNameAtomic);
	            }
	        }
	    }

	    private class IndexedPropAssertion : IPropAssertion
	    {
		    private readonly string name;
		    private readonly int index;
		    private readonly bool dynamic;

		    public IndexedPropAssertion(
			    string name,
			    int index,
			    bool dynamic)
		    {
			    this.name = name;
			    this.index = index;
			    this.dynamic = dynamic;
		    }

		    public IndexedPropAssertion(
			    string name,
			    int index) : this(name, index, false)
		    {
		    }

		    public void Accept(Property property)
		    {
			    if (dynamic) {
				    var prop = (DynamicIndexedProperty) property;
				    Assert.AreEqual(name, prop.PropertyNameAtomic);
				    Assert.AreEqual(index, prop.Index);
			    }
			    else {
				    var prop = (IndexedProperty) property;
				    Assert.AreEqual(name, prop.PropertyNameAtomic);
				    Assert.AreEqual(index, prop.Index);
			    }
		    }
	    }

	    private class MappedPropAssertion : IPropAssertion
	    {
		    private readonly string name;
		    private readonly string key;
		    private readonly bool dynamic;

		    public MappedPropAssertion(
			    string name,
			    string key,
			    bool dynamic)
		    {
			    this.name = name;
			    this.key = key;
			    this.dynamic = dynamic;
		    }

		    public MappedPropAssertion(
			    string name,
			    string key) : this(name, key, false)
		    {
		    }

		    public void Accept(Property property)
		    {
			    if (dynamic) {
				    var prop = (DynamicMappedProperty) property;
				    Assert.AreEqual(name, prop.PropertyNameAtomic);
				    Assert.AreEqual(key, prop.Key);
			    }
			    else {
				    var prop = (MappedProperty) property;
				    Assert.AreEqual(name, prop.PropertyNameAtomic);
				    Assert.AreEqual(key, prop.Key);
			    }
		    }
	    }

	    private class NestedPropAssertion : IPropAssertion
	    {

		    private readonly Consumer<Property>[] consumers;

		    public NestedPropAssertion(params IPropAssertion[] assertions)
		    {
			    this.consumers = assertions
				    .Select(v => new Consumer<Property>(v.Accept))
				    .ToArray();
		    }

		    public void Accept(Property property)
		    {
			    var nested = (NestedProperty) property;
			    Assert.AreEqual(consumers.Length, nested.Properties.Count);
			    for (var i = 0; i < nested.Properties.Count; i++) {
				    consumers[i].Invoke(nested.Properties[i]);
			    }
		    }
	    }
	}
} // end of namespace
