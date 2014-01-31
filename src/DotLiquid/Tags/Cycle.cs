using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
    using System.Threading.Tasks;

    /// <summary>
	/// Cycle is usually used within a loop to alternate between values, like colors or DOM classes.
	/// 
	///   {% for item in items %}
	///    <div class="{% cycle 'red', 'green', 'blue' %}"> {{ item }} </div>
	///   {% end %}
	/// 
	///    <div class="red"> Item one </div>
	///    <div class="green"> Item two </div>
	///    <div class="blue"> Item three </div>
	///    <div class="red"> Item four </div>
	///    <div class="green"> Item five</div>
	/// </summary>
	public class Cycle : Tag
	{
		private static readonly Regex SimpleSyntax = R.B(R.Q(@"^{0}+"), Liquid.QuotedFragment);
		private static readonly Regex NamedSyntax = R.B(R.Q(@"^({0})\s*\:\s*(.*)"), Liquid.QuotedFragment);

		private string[] _variables;
		private string _name;

		public override async Task InitializeAsync(string tagName, string markup, List<string> tokens)
		{
			Match match = NamedSyntax.Match(markup);
			if (match.Success)
			{
				_variables = VariablesFromString(match.Groups[2].Value);
				_name = match.Groups[1].Value;
			}
			else
			{
				match = SimpleSyntax.Match(markup);
				if (match.Success)
				{
					_variables = VariablesFromString(markup);
					_name = "'" + string.Join(string.Empty, _variables) + "'";
				}
				else
				{
					throw new SyntaxException(Liquid.ResourceManager.GetString("CycleTagSyntaxException"));
				}
			}

			await base.InitializeAsync(tagName, markup, tokens).ConfigureAwait(false);
		}

		private static string[] VariablesFromString(string markup)
		{
			return markup.Split(',').Select(var =>
			{
				Match match = Regex.Match(var, string.Format(R.Q(@"\s*({0})\s*"), Liquid.QuotedFragment));
				return (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value))
					? match.Groups[1].Value
					: null;
			}).ToArray();
		}

		public override async Task RenderAsync(Context context, TextWriter result)
		{
			context.Registers["cycle"] = context.Registers["cycle"] ?? new Hash(0);

			await context.StackAsync(async () =>
			{
				string key = context[_name].ToString();
				int iteration = (int) (((Hash) context.Registers["cycle"])[key] ?? 0);
				await result.WriteAsync(context[_variables[iteration]].ToString()).ConfigureAwait(false);
				++iteration;
				if (iteration >= _variables.Length)
					iteration = 0;
				((Hash) context.Registers["cycle"])[key] = iteration;
			}).ConfigureAwait(false);
		}
	}
}