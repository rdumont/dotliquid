using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using DotLiquid.NamingConventions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class DropTests
	{
		#region Classes used in tests

		internal class NullDrop : Drop
		{
			public override object BeforeMethod(string method)
			{
				return null;
			}
		}

		internal class ContextDrop : Drop
		{
			public int Scopes
			{
				get { return Context.Scopes.Count; }
			}

			public IEnumerable<int> ScopesAsArray
			{
				get { return Enumerable.Range(1, Context.Scopes.Count); }
			}

			public int LoopPos
			{
				get { return (int) Context["forloop.index"]; }
			}

			public void Break()
			{
				Debugger.Break();
			}

			public override object BeforeMethod(string method)
			{
				return Context[method];
			}
		}

		internal class ProductDrop : Drop
		{
			internal class TextDrop : Drop
			{
				public string[] Array
				{
					get { return new[] { "text1", "text2" }; }
				}

				public string Text
				{
					get { return "text1"; }
				}
			}

			internal class CatchallDrop : Drop
			{
				public override object BeforeMethod(string method)
				{
					return "method: " + method;
				}
			}

			public TextDrop Texts()
			{
				return new TextDrop();
			}

			public CatchallDrop Catchall()
			{
				return new CatchallDrop();
			}

			public new ContextDrop Context
			{
				get { return new ContextDrop(); }
			}

			protected string CallMeNot()
			{
				return "protected";
			}
		}

		internal class EnumerableDrop : Drop, IEnumerable
		{
			public int Size
			{
				get { return 3; }
			}

			public IEnumerator GetEnumerator()
			{
				yield return 1;
				yield return 2;
				yield return 3;
			}
		}

		internal class DataRowDrop : Drop
		{
			private readonly DataRow _dataRow;

			public DataRowDrop(DataRow dataRow)
			{
				_dataRow = dataRow;
			}

			public override object BeforeMethod(string method)
			{
				if (_dataRow.Table.Columns.Contains(method))
					return _dataRow[method];
				return null;
			}
		}

		internal class CamelCaseDrop : Drop
		{
			public int ProductID
			{
				get { return 1; }
			}
		}

		internal static class ProductFilter
		{
			public static string ProductText(object input)
			{
				return ((ProductDrop) input).Texts().Text;
			}
		}

		#endregion

		[Test]
		public void TestProductDrop()
		{
			Assert.DoesNotThrow(() =>
			{
				Template tpl = Template.ParseAsync("  ").Result;
				tpl.RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Wait();
			});
		}

		[Test]
		public void TestDropDoesNotOutputItself()
		{
			string output = Template.ParseAsync(" {{ product }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Result;
			Assert.AreEqual("  ", output);
		}

		[Test]
		public void TestDropWithFilters()
		{
			string output = Template.ParseAsync(" {{ product | product_text }} ").Result
				.RenderAsync(new RenderParameters
				{
					LocalVariables = Hash.FromAnonymousObject(new { product = new ProductDrop() }),
					Filters = new[] { typeof(ProductFilter) }
                }).Result;
			Assert.AreEqual(" text1 ", output);
		}

		[Test]
		public void TestTextDrop()
		{
			string output = Template.ParseAsync(" {{ product.texts.text }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Result;
			Assert.AreEqual(" text1 ", output);
		}

		[Test]
		public void TestTextDrop2()
		{
			string output = Template.ParseAsync(" {{ product.catchall.unknown }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Result;
			Assert.AreEqual(" method: unknown ", output);
		}

		[Test]
		public void TestTextArrayDrop()
		{
			string output = Template.ParseAsync("{% for text in product.texts.array %} {{text}} {% endfor %}").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Result;
			Assert.AreEqual(" text1  text2 ", output);
		}

		[Test]
		public void TestContextDrop()
		{
			string output = Template.ParseAsync(" {{ context.bar }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), bar = "carrot" })).Result;
			Assert.AreEqual(" carrot ", output);
		}

		[Test]
		public void TestNestedContextDrop()
		{
			string output = Template.ParseAsync(" {{ product.context.foo }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop(), foo = "monkey" })).Result;
			Assert.AreEqual(" monkey ", output);
		}

		[Test]
		public void TestProtected()
		{
			string output = Template.ParseAsync(" {{ product.call_me_not }} ").Result
                .RenderAsync(Hash.FromAnonymousObject(new { product = new ProductDrop() })).Result;
			Assert.AreEqual("  ", output);
		}

		[Test]
		public void TestScope()
		{
            Assert.AreEqual("1", Template.ParseAsync("{{ context.scopes }}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })).Result);
            Assert.AreEqual("2", Template.ParseAsync("{%for i in dummy%}{{ context.scopes }}{%endfor%}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
            Assert.AreEqual("3", Template.ParseAsync("{%for i in dummy%}{%for i in dummy%}{{ context.scopes }}{%endfor%}{%endfor%}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
		}

		[Test]
		public void TestScopeThroughProc()
		{
            Assert.AreEqual("1", Template.ParseAsync("{{ s }}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]) })).Result);
            Assert.AreEqual("2", Template.ParseAsync("{%for i in dummy%}{{ s }}{%endfor%}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })).Result);
            Assert.AreEqual("3", Template.ParseAsync("{%for i in dummy%}{%for i in dummy%}{{ s }}{%endfor%}{%endfor%}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), s = (Proc)(c => c["context.scopes"]), dummy = new[] { 1 } })).Result);
		}

		[Test]
		public void TestScopeWithAssigns()
		{
            Assert.AreEqual("variable", Template.ParseAsync("{% assign a = 'variable'%}{{a}}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })).Result);
            Assert.AreEqual("variable", Template.ParseAsync("{% assign a = 'variable'%}{%for i in dummy%}{{a}}{%endfor%}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
            Assert.AreEqual("test", Template.ParseAsync("{% assign header_gif = \"test\"%}{{header_gif}}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })).Result);
            Assert.AreEqual("test", Template.ParseAsync("{% assign header_gif = 'test'%}{{header_gif}}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop() })).Result);
		}

		[Test]
		public void TestScopeFromTags()
		{
            Assert.AreEqual("1", Template.ParseAsync("{% for i in context.scopes_as_array %}{{i}}{% endfor %}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
            Assert.AreEqual("12", Template.ParseAsync("{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
            Assert.AreEqual("123", Template.ParseAsync("{%for a in dummy%}{%for a in dummy%}{% for i in context.scopes_as_array %}{{i}}{% endfor %}{% endfor %}{% endfor %}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1 } })).Result);
		}

		[Test]
		public void TestAccessContextFromDrop()
		{
            Assert.AreEqual("123", Template.ParseAsync("{% for a in dummy %}{{ context.loop_pos }}{% endfor %}").Result.RenderAsync(Hash.FromAnonymousObject(new { context = new ContextDrop(), dummy = new[] { 1, 2, 3 } })).Result);
		}

		[Test]
		public void TestEnumerableDrop()
		{
            Assert.AreEqual("123", Template.ParseAsync("{% for c in collection %}{{c}}{% endfor %}").Result.RenderAsync(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })).Result);
		}

		[Test]
		public void TestEnumerableDropSize()
		{
            Assert.AreEqual("3", Template.ParseAsync("{{collection.size}}").Result.RenderAsync(Hash.FromAnonymousObject(new { collection = new EnumerableDrop() })).Result);
		}

		[Test]
		public void TestNullCatchAll()
		{
            Assert.AreEqual("", Template.ParseAsync("{{ nulldrop.a_method }}").Result.RenderAsync(Hash.FromAnonymousObject(new { nulldrop = new NullDrop() })).Result);
		}

		[Test]
		public void TestDataRowDrop()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("Column1");
			dataTable.Columns.Add("Column2");

			DataRow dataRow = dataTable.NewRow();
			dataRow["Column1"] = "Hello";
			dataRow["Column2"] = "World";

			Template tpl = Template.ParseAsync(" {{ row.column1 }} ").Result;
            Assert.AreEqual(" Hello ", tpl.RenderAsync(Hash.FromAnonymousObject(new { row = new DataRowDrop(dataRow) })).Result);
		}

		[Test]
		public void TestRubyNamingConventionPrintsHelpfulErrorIfMissingPropertyWouldMatchCSharpNamingConvention()
		{
			INamingConvention savedNamingConvention = Template.NamingConvention;
			Template.NamingConvention = new RubyNamingConvention();
			Template template = Template.ParseAsync("{{ value.ProductID }}").Result;
			Assert.AreEqual("Missing property. Did you mean 'product_id'?", template.RenderAsync(Hash.FromAnonymousObject(new
			{
				value = new CamelCaseDrop()
            })).Result);
			Template.NamingConvention = savedNamingConvention;
		}
	}
}