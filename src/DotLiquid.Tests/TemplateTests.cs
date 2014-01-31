using System.IO;
using System.Net;
using System.Web;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class TemplateTests
	{
		[Test]
		public void TestTokenizeStrings()
		{
			CollectionAssert.AreEqual(new[] { " " }, Template.Tokenize(" "));
			CollectionAssert.AreEqual(new[] { "hello world" }, Template.Tokenize("hello world"));
		}

		[Test]
		public void TestTokenizeVariables()
		{
			CollectionAssert.AreEqual(new[] { "{{funk}}" }, Template.Tokenize("{{funk}}"));
			CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " " }, Template.Tokenize(" {{funk}} "));
			CollectionAssert.AreEqual(new[] { " ", "{{funk}}", " ", "{{so}}", " ", "{{brother}}", " " }, Template.Tokenize(" {{funk}} {{so}} {{brother}} "));
			CollectionAssert.AreEqual(new[] { " ", "{{  funk  }}", " " }, Template.Tokenize(" {{  funk  }} "));
		}

		[Test]
		public void TestTokenizeBlocks()
		{
			CollectionAssert.AreEqual(new[] { "{%comment%}" }, Template.Tokenize("{%comment%}"));
			CollectionAssert.AreEqual(new[] { " ", "{%comment%}", " " }, Template.Tokenize(" {%comment%} "));

			CollectionAssert.AreEqual(new[] { " ", "{%comment%}", " ", "{%endcomment%}", " " }, Template.Tokenize(" {%comment%} {%endcomment%} "));
			CollectionAssert.AreEqual(new[] { "  ", "{% comment %}", " ", "{% endcomment %}", " " }, Template.Tokenize("  {% comment %} {% endcomment %} "));
		}

		[Test]
		public void TestInstanceAssignsPersistOnSameTemplateObjectBetweenParses()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns", t.ParseInternalAsync("{% assign foo = 'from instance assigns' %}{{ foo }}").Result.RenderAsync().Result);
			Assert.AreEqual("from instance assigns", t.ParseInternalAsync("{{ foo }}").Result.RenderAsync().Result);
		}

		[Test]
		public void TestInstanceAssignsPersistOnSameTemplateParsingBetweenRenders()
		{
			Template t = Template.ParseAsync("{{ foo }}{% assign foo = 'foo' %}{{ foo }}").Result;
			Assert.AreEqual("foo", t.RenderAsync().Result);
			Assert.AreEqual("foofoo", t.RenderAsync().Result);
		}

		[Test]
		public void TestCustomAssignsDoNotPersistOnSameTemplate()
		{
			Template t = new Template();
            Assert.AreEqual("from custom assigns", t.ParseInternalAsync("{{ foo }}").Result.RenderAsync(Hash.FromAnonymousObject(new { foo = "from custom assigns" })).Result);
			Assert.AreEqual("", t.ParseInternalAsync("{{ foo }}").Result.RenderAsync().Result);
		}

		[Test]
		public void TestCustomAssignsSquashInstanceAssigns()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns", t.ParseInternalAsync("{% assign foo = 'from instance assigns' %}{{ foo }}").Result.RenderAsync().Result);
            Assert.AreEqual("from custom assigns", t.ParseInternalAsync("{{ foo }}").Result.RenderAsync(Hash.FromAnonymousObject(new { foo = "from custom assigns" })).Result);
		}

		[Test]
		public void TestPersistentAssignsSquashInstanceAssigns()
		{
			Template t = new Template();
			Assert.AreEqual("from instance assigns",
				t.ParseInternalAsync("{% assign foo = 'from instance assigns' %}{{ foo }}").Result.RenderAsync().Result);
			t.Assigns["foo"] = "from persistent assigns";
			Assert.AreEqual("from persistent assigns", t.ParseInternalAsync("{{ foo }}").Result.RenderAsync().Result);
		}

		[Test]
		public void TestLambdaIsCalledOnceFromPersistentAssignsOverMultipleParsesAndRenders()
		{
			Template t = new Template();
			int global = 0;
			t.Assigns["number"] = (Proc) (c => ++global);
			Assert.AreEqual("1", t.ParseInternalAsync("{{number}}").Result.RenderAsync().Result);
			Assert.AreEqual("1", t.ParseInternalAsync("{{number}}").Result.RenderAsync().Result);
			Assert.AreEqual("1", t.RenderAsync().Result);
		}

		[Test]
		public void TestLambdaIsCalledOnceFromCustomAssignsOverMultipleParsesAndRenders()
		{
			Template t = new Template();
			int global = 0;
			Hash assigns = Hash.FromAnonymousObject(new { number = (Proc) (c => ++global) });
			Assert.AreEqual("1", t.ParseInternalAsync("{{number}}").Result.RenderAsync(assigns).Result);
			Assert.AreEqual("1", t.ParseInternalAsync("{{number}}").Result.RenderAsync(assigns).Result);
			Assert.AreEqual("1", t.RenderAsync(assigns).Result);
		}

		[Test]
		public void TestErbLikeTrimmingLeadingWhitespace()
		{
			Template t = Template.ParseAsync("foo\n\t  {%- if true %}hi tobi{% endif %}").Result;
			Assert.AreEqual("foo\nhi tobi", t.RenderAsync().Result);
		}

		[Test]
		public void TestErbLikeTrimmingTrailingWhitespace()
		{
			Template t = Template.ParseAsync("{% if true -%}\nhi tobi\n{% endif %}").Result;
			Assert.AreEqual("hi tobi\n", t.RenderAsync().Result);
		}

		[Test]
		public void TestErbLikeTrimmingLeadingAndTrailingWhitespace()
		{
			Template t = Template.ParseAsync(@"<ul>
{% for item in tasks -%}
    {%- if true -%}
	<li>{{ item }}</li>
    {%- endif -%}
{% endfor -%}
</ul>").Result;
			Assert.AreEqual(@"<ul>
	<li>foo</li>
	<li>bar</li>
	<li>baz</li>
</ul>", t.RenderAsync(Hash.FromAnonymousObject(new { tasks = new [] { "foo", "bar", "baz" } })).Result);
		}

		[Test]
		public void TestRenderToStreamWriter()
		{
			Template template = Template.ParseAsync("{{test}}").Result;

			using (TextWriter writer = new StringWriter())
			{
				template.RenderAsync(writer, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) }).Wait();

				Assert.AreEqual("worked", writer.ToString());
			}
		}

		[Test]
		public void TestRenderToStream()
		{
			Template template = Template.ParseAsync("{{test}}").Result;

			var output = new MemoryStream();
            template.RenderAsync(output, new RenderParameters { LocalVariables = Hash.FromAnonymousObject(new { test = "worked" }) }).Wait();

			output.Seek(0, SeekOrigin.Begin);

			using (TextReader reader = new StreamReader(output))
			{
				Assert.AreEqual("worked", reader.ReadToEnd());
			}
		}

		public class MySimpleType
		{
			public string Name { get; set; }

			public override string ToString()
			{
				return "Foo";
			}
		}

        [Test]
		public void TestRegisterSimpleType()
		{
			Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" });
			Template template = Template.ParseAsync("{{context.Name}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "worked" } })).Result;

			Assert.AreEqual("worked", output);
		}

		[Test]
		public void TestRegisterSimpleTypeToString()
		{
			Template.RegisterSafeType(typeof(MySimpleType), new[] { "ToString" });
			Template template = Template.ParseAsync("{{context}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() })).Result;

			// Doesn't automatically call ToString().
			Assert.AreEqual(string.Empty, output);
		}

        [Test]
        public void TestRegisterSimpleTypeToStringWhenTransformReturnsComplexType()
        {
            Template.RegisterSafeType(typeof(MySimpleType), o =>
                {
                    return o;
                });

            Template template = Template.ParseAsync("{{context}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() })).Result;

            // Does automatically call ToString because Variable.Render calls ToString on objects during rendering.
            Assert.AreEqual("Foo", output);
        }

		[Test]
		public void TestRegisterSimpleTypeTransformer()
		{
			Template.RegisterSafeType(typeof(MySimpleType), o => o.ToString());
			Template template = Template.ParseAsync("{{context}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() })).Result;

			// Uses safe type transformer.
			Assert.AreEqual("Foo", output);
		}

        [Test]
        public void TestRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(MySimpleType), new[] { "Name" }, m => m.ToString());

            Template template = Template.ParseAsync("{{context}}{{context.Name}}").Result; // 

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType() { Name = "Bar" } })).Result;

            // Uses safe type transformer.
            Assert.AreEqual("FooBar", output);
        }

        public class NestedMySimpleType
        {
            public string Name { get; set; }

            public NestedMySimpleType Nested { get; set; }

            public override string ToString()
            {
                return "Foo";
            }
        }

        [Test]
        public void TestNestedRegisterRegisterSafeTypeWithValueTypeTransformer()
        {
            Template.RegisterSafeType(typeof(NestedMySimpleType), new[] { "Name", "Nested" }, m => m.ToString());

            Template template = Template.ParseAsync("{{context}}{{context.Name}} {{context.Nested}}{{context.Nested.Name}}").Result; // 

            var inner = new NestedMySimpleType() { Name = "Bar2" };

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new NestedMySimpleType() { Nested = inner, Name = "Bar" } })).Result;

            // Uses safe type transformer.
            Assert.AreEqual("FooBar FooBar2", output);
        }

        [Test]
        public void TestOverrideDefaultBoolRenderingWithValueTypeTransformer()
        {
            Template.RegisterValueTypeTransformer(typeof(bool), m => (bool)m ? "Win" : "Fail");

            Template template = Template.ParseAsync("{{var1}} {{var2}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { var1 = true, var2 = false })).Result;

            Assert.AreEqual("Win Fail", output);
        }

		[Test]
		public void TestHtmlEncodingFilter()
		{
#if NET35
			Template.RegisterValueTypeTransformer(typeof(string), m => HttpUtility.HtmlEncode((string) m));
#else
            Template.RegisterValueTypeTransformer(typeof(string), m => WebUtility.HtmlEncode((string) m));
#endif
			Template template = Template.ParseAsync("{{var1}} {{var2}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { var1 = "<html>", var2 = "Some <b>bold</b> text." })).Result;

			Assert.AreEqual("&lt;html&gt; Some &lt;b&gt;bold&lt;/b&gt; text.", output);
		}

		public interface IMySimpleInterface2
		{
			string Name { get; }
		}

		public class MySimpleType2 : IMySimpleInterface2
		{
			public string Name { get; set; }
		}

        [Test]
        public void TestRegisterSimpleTypeTransformIntoAnonymousType()
        {
            // specify a transform function
            Template.RegisterSafeType(typeof(MySimpleType2), x => new { Name = ((MySimpleType2)x).Name } );
            Template template = Template.ParseAsync("{{context.Name}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } })).Result;

            Assert.AreEqual("worked", output);
        }

		[Test]
		public void TestRegisterInterfaceTransformIntoAnonymousType()
		{
			// specify a transform function
			Template.RegisterSafeType(typeof(IMySimpleInterface2), x => new { Name = ((IMySimpleInterface2) x).Name });
			Template template = Template.ParseAsync("{{context.Name}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } })).Result;

			Assert.AreEqual("worked", output);
		}

		public class MyUnsafeType2
		{
			public string Name { get; set; }
		}

		[Test]
		public void TestRegisterSimpleTypeTransformIntoUnsafeType()
		{
			// specify a transform function
			Template.RegisterSafeType(typeof(MySimpleType2), x => new MyUnsafeType2 { Name = ((MySimpleType2)x).Name });
			Template template = Template.ParseAsync("{{context.Name}}").Result;

            var output = template.RenderAsync(Hash.FromAnonymousObject(new { context = new MySimpleType2 { Name = "worked" } })).Result;

			Assert.AreEqual("", output);
		}
	}
}