using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
    using System.Threading.Tasks;

    [TestFixture]
	public class ParsingQuirksTests
	{
		[Test]
        public void TestErrorWithCss()
		{
			const string text = " div { font-weight: bold; } ";
            Template template = Template.ParseAsync(text).Result;
            Assert.AreEqual(text, template.RenderAsync().Result);
			Assert.AreEqual(1, template.Root.NodeList.Count);
			Assert.IsInstanceOf<string>(template.Root.NodeList[0]);
		}

		[Test]
        public void TestRaiseOnSingleCloseBrace()
		{
			Assert.Throws<SyntaxException>(async () => await Template.ParseAsync("text {{method} oh nos!"));
		}

		[Test]
        public void TestRaiseOnLabelAndNoCloseBrace()
		{
			Assert.Throws<SyntaxException>(async () => await Template.ParseAsync("TEST {{ "));
		}

		[Test]
        public void TestRaiseOnLabelAndNoCloseBracePercent()
		{
			Assert.Throws<SyntaxException>(async () => await Template.ParseAsync("TEST {% "));
		}

		[Test]
        public void TestErrorOnEmptyFilter()
		{
			Assert.DoesNotThrow(async () =>
			{
				await Template.ParseAsync("{{test |a|b|}}");
				await Template.ParseAsync("{{test}}");
				await Template.ParseAsync("{{|test|}}");
			});
		}

		[Test]
		public async Task TestMeaninglessParens()
		{
			Hash assigns = Hash.FromAnonymousObject(new { b = "bar", c = "baz" });
			await Helper.AssertTemplateResultAsync(" YES ", "{% if a == 'foo' or (b == 'bar' and c == 'baz') or false %} YES {% endif %}", assigns);
		}

		[Test]
		public async Task TestUnexpectedCharactersSilentlyEatLogic()
		{
			await Helper.AssertTemplateResultAsync(" YES ", "{% if true && false %} YES {% endif %}");
			await Helper.AssertTemplateResultAsync("", "{% if false || true %} YES {% endif %}");
		}
	}
}