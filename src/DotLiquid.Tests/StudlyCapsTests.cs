using NUnit.Framework;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;

namespace DotLiquid.Tests
{
    using System.Threading.Tasks;

    [TestFixture]
	public class StudlyCapsTests
	{
		[Test]
		public async Task TestSimpleVariablesStudlyCaps()
		{
			Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.ParseAsync("{{ Greeting }} {{ Name }}").Result;
            Assert.AreEqual("Hello Tobi", template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })).Result);

			Template.NamingConvention = new CSharpNamingConvention();
            Assert.AreEqual("Hello Tobi", template.RenderAsync(Hash.FromAnonymousObject(new { Greeting = "Hello", Name = "Tobi" })).Result);
            Assert.AreEqual(" ", template.RenderAsync(Hash.FromAnonymousObject(new { greeting = "Hello", name = "Tobi" })).Result);
		}

		[Test]
		public async Task TestTagsStudlyCapsAreNotAllowed()
		{
			Template.NamingConvention = new RubyNamingConvention();
			Assert.Throws<SyntaxException>(async () => await Template.ParseAsync("{% IF user = 'tobi' %}Hello Tobi{% EndIf %}"));
		}

		[Test]
		public async Task TestFiltersStudlyCapsAreNotAllowed()
		{
			Template.NamingConvention = new RubyNamingConvention();
            Template template = Template.ParseAsync("{{ 'hi tobi' | upcase }}").Result;
            Assert.AreEqual("HI TOBI", template.RenderAsync().Result);

			Template.NamingConvention = new CSharpNamingConvention();
            template = Template.ParseAsync("{{ 'hi tobi' | Upcase }}").Result;
            Assert.AreEqual("HI TOBI", template.RenderAsync().Result);
		}

		[Test]
		public async Task TestAssignsStudlyCaps()
		{
			Template.NamingConvention = new RubyNamingConvention();

			await Helper.AssertTemplateResultAsync(".foo.", "{% assign FoO = values %}.{{ fOo[0] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
			await Helper.AssertTemplateResultAsync(".bar.", "{% assign fOo = values %}.{{ fOO[1] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));

			Template.NamingConvention = new CSharpNamingConvention();

			await Helper.AssertTemplateResultAsync(".foo.", "{% assign Foo = values %}.{{ Foo[0] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
			await Helper.AssertTemplateResultAsync(".bar.", "{% assign fOo = values %}.{{ fOo[1] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
		}
	}
}