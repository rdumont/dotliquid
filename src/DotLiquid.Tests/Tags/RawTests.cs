using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    using System.Threading.Tasks;

    [TestFixture]
    public class RawTests
    {
        [Test]
        public async Task TestTagInRaw()
        {
			await Helper.AssertTemplateResultAsync("{% comment %} test {% endcomment %}",
				"{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
        }

		[Test]
		public async Task TestOutputInRaw()
		{
			await Helper.AssertTemplateResultAsync("{{ test }}",
				"{% raw %}{{ test }}{% endraw %}");
		}
	}
}