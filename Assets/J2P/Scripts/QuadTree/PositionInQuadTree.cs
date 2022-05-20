using System;
using System.Collections;
using System.Text;

namespace CustomPhysics2D
{
	/// <summary>
	/// Represents a position in a quad tree
	/// </summary>
	public struct PositionInQuadTree : IEquatable<PositionInQuadTree>
	{
		public PositionInQuadTreeDepth[] posInDepths;
		public bool inRoot
		{
			get; private set;
		}
		public int storeDepth;

		public PositionInQuadTree(int maxDepth)
		{
			inRoot = maxDepth == 0;
			if (!inRoot)
			{
				posInDepths = new PositionInQuadTreeDepth[maxDepth];
				for (int i = 0; i < posInDepths.Length; i++)
				{
					posInDepths[i].rowIndex = -1;
					posInDepths[i].columnIndex = -1;
				}
			}
			else posInDepths = null;
			storeDepth = 0;
		}

		public void CopyFrom(PositionInQuadTree other)
		{
			storeDepth = other.storeDepth;
			inRoot = other.inRoot;

			if (posInDepths != null && other.posInDepths != null)
				other.posInDepths.CopyTo(posInDepths, 0);
		}

		public void Reset()
		{
			if (inRoot) return;

			inRoot = false;
			for (int i = 0; i < posInDepths.Length; i++)
			{
				posInDepths[i].rowIndex = -1;
				posInDepths[i].columnIndex = -1;
			}
			storeDepth = 0;
		}

		public override string ToString()
		{
			if (posInDepths == null) return inRoot ? "In Root" : string.Empty;

			var sb = new StringBuilder();
			for (int i = 0; i < posInDepths.Length; i++)
			{
				if (posInDepths[i].rowIndex != -1)
				{
					sb.AppendLine(string.Format("Depth:\t{0},\tposition:\t{1}", i, posInDepths[i]));
				}
			}
			return sb.ToString();
		}

		public bool Equals(PositionInQuadTree other)
		{
			if (inRoot ^ other.inRoot) return false;

			if (inRoot) return true;

			if (other.posInDepths.Length != posInDepths.Length) return false;

			return ((IStructuralEquatable)posInDepths).Equals(other.posInDepths, StructuralComparisons.StructuralEqualityComparer);
		}
	}
}
