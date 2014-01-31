using System.Collections.Generic;

namespace DotLiquid
{
    using System.Threading.Tasks;

    public class Document : Block
	{
		/// <summary>
		/// We don't need markup to open this block
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="markup"></param>
		/// <param name="tokens"></param>
		public override async Task InitializeAsync(string tagName, string markup, List<string> tokens)
		{
			await ParseAsync(tokens).ConfigureAwait(false);
		}

		/// <summary>
		/// There isn't a real delimiter
		/// </summary>
		protected override string BlockDelimiter
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Document blocks don't need to be terminated since they are not actually opened
		/// </summary>
		protected override void AssertMissingDelimitation()
		{
		}
	}
}