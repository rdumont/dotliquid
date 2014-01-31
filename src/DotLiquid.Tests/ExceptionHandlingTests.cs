using DotLiquid.Exceptions;
using NUnit.Framework;

namespace DotLiquid.Tests
{
	[TestFixture]
	public class ExceptionHandlingTests
	{
		private class ExceptionDrop : Drop
		{
			public void ArgumentException()
			{
				throw new ArgumentException("argument exception");
			}

			public void SyntaxException()
			{
				throw new SyntaxException("syntax exception");
			}
		}

		[Test]
		public void TestSyntaxException()
		{
			Template template = null;
            Assert.DoesNotThrow(() => { template = Template.ParseAsync(" {{ errors.syntax_exception }} ").Result; });
            string result = template.RenderAsync(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() })).Result;
			Assert.AreEqual(" Liquid syntax error: syntax exception ", result);

			Assert.AreEqual(1, template.Errors.Count);
			Assert.IsInstanceOf<SyntaxException>(template.Errors[0]);
		}

		[Test]
		public void TestArgumentException()
		{
			Template template = null;
            Assert.DoesNotThrow(() => { template = Template.ParseAsync(" {{ errors.argument_exception }} ").Result; });
            string result = template.RenderAsync(Hash.FromAnonymousObject(new { errors = new ExceptionDrop() })).Result;
			Assert.AreEqual(" Liquid error: argument exception ", result);

			Assert.AreEqual(1, template.Errors.Count);
			Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
		}

		[Test]
		public void TestMissingEndTagParseTimeError()
		{
			Assert.Throws<SyntaxException>(async () => await Template.ParseAsync(" {% for a in b %} ... "));
		}

		[Test]
		public void TestUnrecognizedOperator()
		{
			Template template = null;
            Assert.DoesNotThrow(() => { template = Template.ParseAsync(" {% if 1 =! 2 %}ok{% endif %} ").Result; });
            Assert.AreEqual(" Liquid error: Unknown operator =! ", template.RenderAsync().Result);

			Assert.AreEqual(1, template.Errors.Count);
			Assert.IsInstanceOf<ArgumentException>(template.Errors[0]);
		}
	}
}