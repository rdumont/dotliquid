using DotLiquid.FileSystems;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    using System.Threading.Tasks;

    [TestFixture]
	public class InheritanceTests
	{
		private class TestFileSystem : IFileSystem
		{
			public string ReadTemplateFile(Context context, string templateName)
			{
				string templatePath = (string) context[templateName];

				switch (templatePath)
				{
					case "simple":
						return "test";
					case "complex":
						return @"some markup here...
                                 {% block thing %}
                                     thing block
                                 {% endblock %}
                                 {% block another %}
                                     another block
                                 {% endblock %}
                                 ...and some markup here";
					case "nested":
						return @"{% extends 'complex' %}
                                 {% block thing %}
                                    another thing (from nested)
                                 {% endblock %}";
                    case "outer":
                        return "{% block start %}{% endblock %}A{% block outer %}{% endblock %}Z";
                    case "middle":
				        return @"{% extends 'outer' %}
                                 {% block outer %}B{% block middle %}{% endblock %}Y{% endblock %}";
                    case "middleunless":
                        return @"{% extends 'outer' %}
                                 {% block outer %}B{% unless nomiddle %}{% block middle %}{% endblock %}{% endunless %}Y{% endblock %}";
					default:
						return @"{% extends 'complex' %}
                                 {% block thing %}
                                    thing block (from nested)
                                 {% endblock %}";
				}
			}
		}

		private IFileSystem _originalFileSystem;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_originalFileSystem = Template.FileSystem;
			Template.FileSystem = new TestFileSystem();
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			Template.FileSystem = _originalFileSystem;
		}

		[Test]
		public void CanOutputTheContentsOfTheExtendedTemplate()
		{
			Template template = Template.ParseAsync(
				@"{% extends 'simple' %}
                    {% block thing %}
                        yeah
                    {% endblock %}").Result;

			StringAssert.Contains("test", template.RenderAsync().Result);
		}

		[Test]
		public void CanInherit()
		{
			Template template = Template.ParseAsync(@"{% extends 'complex' %}").Result;

			StringAssert.Contains("thing block", template.RenderAsync().Result);
		}

		[Test]
		public void CanInheritAndReplaceBlocks()
		{
			Template template = Template.ParseAsync(
				@"{% extends 'complex' %}
                    {% block another %}
                        new content for another
                    {% endblock %}").Result;

			StringAssert.Contains("new content for another", template.RenderAsync().Result);
		}

		[Test]
		public void CanProcessNestedInheritance()
		{
			Template template = Template.ParseAsync(
				@"{% extends 'nested' %}
                    {% block thing %}
                        replacing block thing
                    {% endblock %}").Result;

			StringAssert.Contains("replacing block thing", template.RenderAsync().Result);
			StringAssert.DoesNotContain("thing block", template.RenderAsync().Result);
		}

		[Test]
		public async Task CanRenderSuper()
		{
            Template template = await Template.ParseAsync(
				@"{% extends 'complex' %}
                    {% block another %}
                        {{ block.super }} + some other content
                    {% endblock %}");

			StringAssert.Contains("another block", await template.RenderAsync());
			StringAssert.Contains("some other content", await template.RenderAsync());
		}

	    [Test]
	    public void CanDefineBlockInInheritedBlock()
	    {
	        Template template = Template.ParseAsync(
	            @"{% extends 'middle' %}
                  {% block middle %}C{% endblock %}").Result;
            Assert.AreEqual("ABCYZ", template.RenderAsync().Result);
	    }

        [Test]
        public void CanDefineContentInInheritedBlockFromAboveParent()
        {
            Template template = Template.ParseAsync(
                @"{% extends 'middle' %}
                  {% block start %}!{% endblock %}").Result;
            Assert.AreEqual("!ABYZ", template.RenderAsync().Result);
        }

        [Test]
        public void CanRenderBlockContainedInConditional()
        {
            Template template = Template.ParseAsync(
                @"{% extends 'middleunless' %}
                  {% block middle %}C{% endblock %}").Result;
            Assert.AreEqual("ABCYZ", template.RenderAsync().Result);

            template = Template.ParseAsync(
                @"{% extends 'middleunless' %}
                  {% block start %}{% assign nomiddle = true %}{% endblock %}
                  {% block middle %}C{% endblock %}").Result;
            Assert.AreEqual("ABYZ", template.RenderAsync().Result);
        }

        [Test]
        public void RepeatedRendersProduceSameResult()
        {
            Template template = Template.ParseAsync(
                @"{% extends 'middle' %}
                  {% block start %}!{% endblock %}
                  {% block middle %}C{% endblock %}").Result;
            Assert.AreEqual("!ABCYZ", template.RenderAsync().Result);
            Assert.AreEqual("!ABCYZ", template.RenderAsync().Result);
        }
	}
}