using System.Globalization;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    using System.Threading.Tasks;

    [TestFixture]
	public class AssignTests
	{
		[Test]
		public async Task TestAssignedVariable()
		{
			await Helper.AssertTemplateResultAsync(".foo.", "{% assign foo = values %}.{{ foo[0] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
			await Helper.AssertTemplateResultAsync(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
				Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
		}

		[Test]
		public async Task TestAssignDecimal()
		{
			await Helper.AssertTemplateResultAsync(string.Format("10{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = decimal %}{{ foo }}",
				Hash.FromAnonymousObject(new { @decimal = 10.05d }));
		}

		[Test, SetCulture("en-GB")]
		public async Task TestAssignDecimalInlineWithEnglishDecimalSeparator()
		{
			await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2.5 %}{{ foo }}");
		}

		[Test, SetCulture("en-GB")]
		public async Task TestAssignDecimalInlineWithEnglishGroupSeparator()
		{
			await Helper.AssertTemplateResultAsync("2500",
				"{% assign foo = 2,500 %}{{ foo }}");
		}

		[Test, SetCulture("fr-FR")]
		public async Task TestAssignDecimalInlineWithFrenchDecimalSeparator()
		{
			await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2,5 %}{{ foo }}");
		}

		[Test, SetCulture("fr-FR")]
		public async Task TestAssignDecimalInlineWithInvariantDecimalSeparatorInFrenchCulture()
		{
			await Helper.AssertTemplateResultAsync(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
				"{% assign foo = 2.5 %}{{ foo }}");
		}

		[Test]
		public async Task TestAssignWithFilter()
		{
			await Helper.AssertTemplateResultAsync(".bar.", "{% assign foo = values | split: ',' %}.{{ foo[1] }}.", 
				Hash.FromAnonymousObject(new { values = "foo,bar,baz" }));
		}
	}
}