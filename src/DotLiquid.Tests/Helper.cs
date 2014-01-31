using NUnit.Framework;

namespace DotLiquid.Tests
{
    using System.Threading.Tasks;

    public class Helper
	{
		public static async Task AssertTemplateResultAsync(string expected, string template, Hash localVariables)
		{
            Assert.AreEqual(expected, await (await Template.ParseAsync(template).ConfigureAwait(false)).RenderAsync(localVariables).ConfigureAwait(false));
		}

		public static async Task AssertTemplateResultAsync(string expected, string template)
		{
			await AssertTemplateResultAsync(expected, template, null).ConfigureAwait(false);
		}
	}
}