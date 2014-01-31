using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class OutputTests
	{
		private static class FunnyFilter
		{
			public static string MakeFunny(string input)
			{
				return "LOL";
			}

			public static string CiteFunny(string input)
			{
				return "LOL: " + input;
			}

#if NET35
            public static string AddSmiley(string input)
            {
                return AddSmiley(input, ":-)");
            }

            public static string AddSmiley(string input, string smiley)
#else
			public static string AddSmiley(string input, string smiley = ":-)")
#endif
			{
				return input + " " + smiley;
			}

#if NET35
            public static string AddTag(string input)
            {
                return AddTag(input, "p", "foo");
            }

            public static string AddTag(string input, string tag, string id)
#else
			public static string AddTag(string input, string tag = "p", string id = "foo")
#endif
			{
				return string.Format("<{0} id=\"{1}\">{2}</{0}>", tag, id, input);
			}

			public static string Paragraph(string input)
			{
				return string.Format("<p>{0}</p>", input);
			}

			public static string LinkTo(string name, string url)
			{
				return string.Format("<a href=\"{0}\">{1}</a>", url, name);
			}
		}

		private Hash _assigns;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_assigns = Hash.FromAnonymousObject(new
			{
				best_cars = "bmw",
				car = Hash.FromAnonymousObject(new { bmw = "good", gm = "bad" })
			});
		}

		[Test]
		public void TestVariable()
		{
			Assert.AreEqual(" bmw ", Template.ParseAsync(" {{best_cars}} ").Result.RenderAsync(_assigns).Result);
		}

		[Test]
		public void TestVariableTraversing()
		{
            Assert.AreEqual(" good bad good ", Template.ParseAsync(" {{car.bmw}} {{car.gm}} {{car.bmw}} ").Result.RenderAsync(_assigns).Result);
		}

		[Test]
		public void TestVariablePiping()
		{
            Assert.AreEqual(" LOL ", Template.ParseAsync(" {{ car.gm | make_funny }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestVariablePipingWithInput()
		{
            Assert.AreEqual(" LOL: bad ", Template.ParseAsync(" {{ car.gm | cite_funny }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestVariablePipingWithArgs()
		{
            Assert.AreEqual(" bad :-( ", Template.ParseAsync(" {{ car.gm | add_smiley : ':-(' }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestVariablePipingWithNoArgs()
		{
            Assert.AreEqual(" bad :-) ", Template.ParseAsync(" {{ car.gm | add_smiley }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestMultipleVariablePipingWithArgs()
		{
            Assert.AreEqual(" bad :-( :-( ", Template.ParseAsync(" {{ car.gm | add_smiley : ':-(' | add_smiley : ':-(' }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestVariablePipingWithArgs2()
		{
            Assert.AreEqual(" <span id=\"bar\">bad</span> ", Template.ParseAsync(" {{ car.gm | add_tag : 'span', 'bar' }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestVariablePipingWithWithVariableArgs()
		{
            Assert.AreEqual(" <span id=\"good\">bad</span> ", Template.ParseAsync(" {{ car.gm | add_tag : 'span', car.bmw }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestMultiplePipings()
		{
            Assert.AreEqual(" <p>LOL: bmw</p> ", Template.ParseAsync(" {{ best_cars | cite_funny | paragraph }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}

		[Test]
		public void TestLinkTo()
		{
            Assert.AreEqual(" <a href=\"http://typo.leetsoft.com\">Typo</a> ", Template.ParseAsync(" {{ 'Typo' | link_to: 'http://typo.leetsoft.com' }} ").Result.RenderAsync(new RenderParameters { LocalVariables = _assigns, Filters = new[] { typeof(FunnyFilter) } }).Result);
		}
	}
}