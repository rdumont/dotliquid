using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
    using System.Threading.Tasks;

    /// <summary>
	/// Capture stores the result of a block into a variable without rendering it inplace.
	/// 
	/// {% capture heading %}
	/// Monkeys!
	/// {% endcapture %}
	/// ...
	/// <h1>{{ heading }}</h1>
	/// 
	/// Capture is useful for saving content for use later in your template, such as
	/// in a sidebar or footer.
	/// </summary>
	public class Capture : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(@"(\w+)");

		private string _to;

		public override async Task InitializeAsync(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
				_to = syntaxMatch.Groups[1].Value;
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("CapureTagSyntaxException"));

			await base.InitializeAsync(tagName, markup, tokens).ConfigureAwait(false);
		}

		public override async Task RenderAsync(Context context, TextWriter result)
		{
			using (TextWriter temp = new StringWriter())
			{
				await base.RenderAsync(context, temp).ConfigureAwait(false);
				context.Scopes.Last()[_to] = temp.ToString();
			}
		}
	}
}