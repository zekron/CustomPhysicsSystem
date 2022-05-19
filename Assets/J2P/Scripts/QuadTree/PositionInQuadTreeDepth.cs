namespace CustomPhysics2D
{
    /// <summary>
    /// Represents a position in a quad tree's depth
    /// </summary>
    public struct PositionInQuadTreeDepth
    {
        public int rowIndex;
        public int columnIndex;

        public override string ToString() => string.Format("({0}, {1})", rowIndex, columnIndex);
    }
}
