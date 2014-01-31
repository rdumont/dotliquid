using System;
using System.Collections.Generic;
using DotLiquid.Exceptions;
using NUnit.Framework;
using DotLiquid.Tags;

namespace DotLiquid.Tests.Tags
{
	[TestFixture]
	public class LiteralTests
	{
		[Test]
		public void TestEmptyLiteral()
		{
            Template t = Template.ParseAsync("{% literal %}{% endliteral %}").Result;
            Assert.AreEqual(string.Empty, t.RenderAsync().Result);
            t = Template.ParseAsync("{{{}}}").Result;
            Assert.AreEqual(string.Empty, t.RenderAsync().Result);
		}

		[Test]
		public void TestSimpleLiteralValue()
		{
            Template t = Template.ParseAsync("{% literal %}howdy{% endliteral %}").Result;
            Assert.AreEqual("howdy", t.RenderAsync().Result);
		}

		[Test]
		public void TestLiteralsIgnoreLiquidMarkup()
		{
            Template t = Template.ParseAsync("{% literal %}{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}{% endliteral %}").Result;
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", t.RenderAsync().Result);
		}

		[Test]
		public void TestShorthandSyntax()
		{
            Template t = Template.ParseAsync("{{{{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}}}}").Result;
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", t.RenderAsync().Result);
		}

		[Test]
		public void TestLiteralsDontRemoveComments()
		{
            Template t = Template.ParseAsync("{{{ {# comment #} }}}").Result;
            Assert.AreEqual("{# comment #}", t.RenderAsync().Result);
		}

		[Test]
		public void TestFromShorthand()
		{
			Assert.AreEqual("{% literal %}gnomeslab{% endliteral %}", Literal.FromShortHand("{{{gnomeslab}}}"));
		}

		[Test]
		public void TestFromShorthandIgnoresImproperSyntax()
		{
			Assert.AreEqual("{% if 'hi' == 'hi' %}hi{% endif %}", Literal.FromShortHand("{% if 'hi' == 'hi' %}hi{% endif %}"));
		}
	}
}