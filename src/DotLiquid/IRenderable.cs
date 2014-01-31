using System.IO;

namespace DotLiquid
{
    using System.Threading.Tasks;

    internal interface IRenderable
	{
		Task RenderAsync(Context context, TextWriter result);
	}
}