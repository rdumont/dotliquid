using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;

namespace DotLiquid.Tags
{
    using System.Threading.Tasks;

    public class BlockDrop : Drop
	{
		private readonly Block _block;
	    private readonly TextWriter _result;

		public BlockDrop(Block block, TextWriter result)
		{
		    _block = block;
		    _result = result;
		}

	    public async Task Super()
		{
			await _block.CallSuperAsync(Context, _result).ConfigureAwait(false);
		}
	}

    // Keeps track of the render-time state of all Blocks for a given Context
    internal class BlockRenderState
    {
        public Dictionary<Block, Block> Parents { get; private set; }
        public Dictionary<Block, List<object>> NodeLists { get; private set; }

        public BlockRenderState()
        {
            Parents = new Dictionary<Block, Block>();
            NodeLists = new Dictionary<Block, List<object>>();
        }

        public List<object> GetNodeList(Block block)
        {
            List<object> nodeList;
            if (!NodeLists.TryGetValue(block, out nodeList)) nodeList = block.NodeList;
            return nodeList;
        }

        // Searches up the scopes for the inner-most BlockRenderState (though there should be only one)
        public static BlockRenderState Find(Context context)
        {
            foreach (Hash scope in context.Scopes)
            {
                object blockState;
                if (scope.TryGetValue("blockstate", out blockState))
                {
                    return blockState as BlockRenderState;
                }
            }
            return null;
        }
    }

	/// <summary>
	/// The Block tag is used in conjunction with the Extends tag to provide template inheritance.
	/// For an example please refer to the Extends tag.
	/// </summary>
	public class Block : DotLiquid.Block
	{
		private static readonly Regex Syntax = new Regex(@"(\w+)");

		internal string BlockName { get; set; }

		public override async Task InitializeAsync(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
				BlockName = syntaxMatch.Groups[1].Value;
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagSyntaxException"));

			if (tokens != null)
			{
				await base.InitializeAsync(tagName, markup, tokens).ConfigureAwait(false);
			}
		}

		internal override void AssertTagRulesViolation(List<object> rootNodeList)
		{
			rootNodeList.ForEach(n =>
			{
				Block b1 = n as Block;

				if (b1 != null)
				{
					List<object> found = rootNodeList.FindAll(o =>
					{
						Block b2 = o as Block;
						return b2 != null && b1.BlockName == b2.BlockName;
					});

					if (found != null && found.Count > 1)
					{
						throw new SyntaxException(Liquid.ResourceManager.GetString("BlockTagAlreadyDefinedException"), b1.BlockName);
					}
				}
			});
		}

		public override async Task RenderAsync(Context context, TextWriter result)
		{
			BlockRenderState blockState = BlockRenderState.Find(context);
			await context.StackAsync(async () =>
			{
				context["block"] = new BlockDrop(this, result);
				await RenderAllAsync(GetNodeList(blockState), context, result).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

        // Gets the render-time node list from the node state
        internal List<object> GetNodeList(BlockRenderState blockState)
        {
            return blockState == null ? NodeList : blockState.GetNodeList(this);
        }

        public async Task AddParentAsync(Dictionary<Block, Block> parents, List<object> nodeList)
        {
            Block parent;
            if(parents.TryGetValue(this, out parent))
            {
                await parent.AddParentAsync(parents, nodeList).ConfigureAwait(false);
            }
            else
            {
                parent = new Block();
                await parent.InitializeAsync(TagName, BlockName, null).ConfigureAwait(false);
                parent.NodeList = new List<object>(nodeList);
                parents[this] = parent;
            }
        }

		public async Task CallSuperAsync(Context context, TextWriter result)
		{
            BlockRenderState blockState = BlockRenderState.Find(context);
		    Block parent;
            if (blockState != null
                && blockState.Parents.TryGetValue(this, out parent)
                && parent != null)
		    {
		        await parent.RenderAsync(context, result).ConfigureAwait(false);
		    }
		}
	}
}