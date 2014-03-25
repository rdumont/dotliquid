namespace DotLiquid.Tags
{
    public static class BlockExtensions
    {
        public static bool IsOverridden(this Block @this, Context context)
        {
            var blockState = BlockRenderState.Find(context);
            return blockState.NodeLists.ContainsKey(@this);
        }
    }
}
