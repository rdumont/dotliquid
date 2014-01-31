using System.Collections.Generic;
using System.IO;

namespace DotLiquid
{
    using System.Threading.Tasks;

    public class Tag : IRenderable
	{
		public List<object> NodeList { get; protected set; }
		protected string TagName { get; private set; }
		protected string Markup { get; private set; }

		/// <summary>
		/// Only want to allow Tags to be created in inherited classes or tests.
		/// </summary>
		protected internal Tag()
		{
		}

		internal virtual void AssertTagRulesViolation(List<object> rootNodeList)
		{
		}

		public virtual async Task InitializeAsync(string tagName, string markup, List<string> tokens)
		{
			TagName = tagName;
			Markup = markup;
			await ParseAsync(tokens).ConfigureAwait(false);
		}

		protected virtual Task ParseAsync(List<string> tokens)
		{
		    return Task.Delay(0);
		}

		public string Name
		{
			get { return GetType().Name.ToLower(); }
		}

		public virtual Task RenderAsync(Context context, TextWriter result)
		{
		    return Task.Delay(0);
		}

		/// <summary>
		/// Primarily intended for testing.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal async Task<string> RenderAsync(Context context)
		{
			using (TextWriter result = new StringWriter())
			{
				await RenderAsync(context, result).ConfigureAwait(false);
				return result.ToString();
			}
		}
	}
}