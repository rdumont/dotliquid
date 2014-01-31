using System.IO;

namespace DotLiquid.Tags
{
    using System.Threading.Tasks;

    public class IfChanged : DotLiquid.Block
	{
		public override async Task RenderAsync(Context context, TextWriter result)
		{
			await context.StackAsync(async () =>
			{
				string tempString;
				using (TextWriter temp = new StringWriter())
				{
					await RenderAllAsync(NodeList, context, temp).ConfigureAwait(false);
					tempString = temp.ToString();
				}

				if (tempString != (context.Registers["ifchanged"] as string))
				{
					context.Registers["ifchanged"] = tempString;
					await result.WriteAsync(tempString).ConfigureAwait(false);
				}
			}).ConfigureAwait(false);
		}
	}
}