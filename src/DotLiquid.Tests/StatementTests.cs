using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class StatementTests
	{
		[Test]
		public void TestTrueEqlTrue()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if true == true %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestTrueNotEqlTrue()
		{
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if true != true %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestTrueLqTrue()
		{
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if 0 > 0 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestOneLqZero()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 1 > 0 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestZeroLqOne()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 0 < 1 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestZeroLqOrEqualOne()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 0 <= 0 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestZeroLqOrEqualOneInvolvingNil()
		{
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if null <= 0 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if 0 <= null %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestZeroLqqOrEqualOne()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 0 >= 0 %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestStrings()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 'test' == 'test' %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestStringsNotEqual()
		{
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if 'test' != 'test' %} true {% else %} false {% endif %} ").Result.RenderAsync().Result);
		}

		[Test]
		public void TestVarAndStringEqual()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if var == 'hello there!' %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = "hello there!" })).Result);
		}

		[Test]
		public void TestVarAndStringAreEqualBackwards()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if 'hello there!' == var %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = "hello there!" })).Result);
		}

		[Test]
		public void TestIsCollectionEmpty()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if array == empty %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { array = new object[] { } })).Result);
		}

		[Test]
		public void TestIsNotCollectionEmpty()
		{
			Assert.AreEqual("  false  ", Template.ParseAsync(" {% if array == empty %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { array = new[] { 1, 2, 3 } })).Result);
		}

		[Test]
		public void TestNil()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if var == nil %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = (object) null })).Result);
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if var == null %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = (object) null })).Result);
		}

		[Test]
		public void TestNotNil()
		{
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if var != nil %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = 1 })).Result);
			Assert.AreEqual("  true  ", Template.ParseAsync(" {% if var != null %} true {% else %} false {% endif %} ").Result.RenderAsync(Hash.FromAnonymousObject(new { var = 1 })).Result);
		}
	}
}