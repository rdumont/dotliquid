using System;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class VariableResolutionTests
	{
		[Test]
		public void TestSimpleVariable()
		{
            Template template = Template.ParseAsync("{{test}}").Result;
			Assert.AreEqual("worked", template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked" })).Result);
            Assert.AreEqual("worked wonderfully", template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked wonderfully" })).Result);
		}

		[Test]
		public void TestSimpleWithWhitespaces()
		{
            Template template = Template.ParseAsync("  {{ test }}  ").Result;
            Assert.AreEqual("  worked  ", template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked" })).Result);
            Assert.AreEqual("  worked wonderfully  ", template.RenderAsync(Hash.FromAnonymousObject(new { test = "worked wonderfully" })).Result);
		}

		[Test]
		public void TestIgnoreUnknown()
		{
            Template template = Template.ParseAsync("{{ test }}").Result;
            Assert.AreEqual("", template.RenderAsync().Result);
		}

		[Test]
		public void TestHashScoping()
		{
            Template template = Template.ParseAsync("{{ test.test }}").Result;
            Assert.AreEqual("worked", template.RenderAsync(Hash.FromAnonymousObject(new { test = new { test = "worked" } })).Result);
		}

		[Test]
		public void TestPresetAssigns()
		{
            Template template = Template.ParseAsync("{{ test }}").Result;
			template.Assigns["test"] = "worked";
            Assert.AreEqual("worked", template.RenderAsync().Result);
		}

		[Test]
		public void TestReuseParsedTemplate()
		{
            Template template = Template.ParseAsync("{{ greeting }} {{ name }}").Result;
			template.Assigns["greeting"] = "Goodbye";
            Assert.AreEqual("Hello Tobi", template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })).Result);
            Assert.AreEqual("Hello ", template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", unknown = "Tobi" })).Result);
            Assert.AreEqual("Hello Brian", template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Brian" })).Result);
            Assert.AreEqual("Goodbye Brian", template.RenderAsync(Hash.FromAnonymousObject(new { name = "Brian" })).Result);
			CollectionAssert.AreEqual(Hash.FromAnonymousObject(new { greeting = "Goodbye" }), template.Assigns);
		}

		[Test]
		public void TestAssignsNotPollutedFromTemplate()
		{
            Template template = Template.ParseAsync("{{ test }}{% assign test = 'bar' %}{{ test }}").Result;
			template.Assigns["test"] = "baz";
            Assert.AreEqual("bazbar", template.RenderAsync().Result);
            Assert.AreEqual("bazbar", template.RenderAsync().Result);
            Assert.AreEqual("foobar", template.RenderAsync(Hash.FromAnonymousObject(new { test = "foo" })).Result);
            Assert.AreEqual("bazbar", template.RenderAsync().Result);
		}

		[Test]
		public void TestHashWithDefaultProc()
		{
            Template template = Template.ParseAsync("Hello {{ test }}").Result;
			Hash assigns = new Hash((h, k) => { throw new Exception("Unknown variable '" + k + "'"); });
			assigns["test"] = "Tobi";
			Assert.AreEqual("Hello Tobi", template.RenderAsync(new RenderParameters
			{
				LocalVariables = assigns,
				RethrowErrors = true
            }).Result);
			assigns.Remove("test");
			Exception ex = Assert.Throws<Exception>(async () => await template.RenderAsync(new RenderParameters
			{
				LocalVariables = assigns,
				RethrowErrors = true
            }));
			Assert.AreEqual("Unknown variable 'test'", ex.Message);
		}
	}
}