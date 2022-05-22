using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
    /// <summary>
    /// 四叉树节点信息
    /// </summary>
    public class QuadTreeNode
    {
        private const int MAX_ROW_LENGTH = 2;
        private const int MAX_COLUMN_LENGTH = 2;

        //Include children's items
        public int TotalItemsCount
        {
            get; private set;
        }

        public bool IsLeaf
        {
            get;
        }

        /// <summary>
        /// 该节点下的所有物体，含压线物体
        /// </summary>
        public List<IQuadTreeItem> Items { get; }

        public Rect NodeRect
        {
            get;
        }

        public Rect NodeLooseRect
        {
            get;
        }

        public QuadTreeNode[,] ChildNodes
        {
            get;
        }

        /// <summary>
        /// ChildNodes索引器
        /// </summary>
        /// <param name="position">某四叉树深度的坐标</param>
        /// <returns></returns>
        public QuadTreeNode this[PositionInQuadTreeDepth position]
        {
            get
            {
                if (CorrectPosition(position))
                    return ChildNodes[position.rowIndex, position.columnIndex];
                else
                    throw new System.Exception("Wrong position!" + position.ToString());
            }
            set
            {
                if (CorrectPosition(position))
                    ChildNodes[position.rowIndex, position.columnIndex] = value;
            }
        }

        public QuadTreeNode(Rect rect, int depth, int maxDepth)
        {
            Items = new List<IQuadTreeItem>();
            NodeRect = rect;
            var looseRectSize = 2 * rect.size;
            var looseRectPos = NodeRect.center - NodeRect.size;
            NodeLooseRect = new Rect(looseRectPos, looseRectSize);
            IsLeaf = depth == maxDepth;

            if (IsLeaf == false)
            {
                ChildNodes = new QuadTreeNode[MAX_ROW_LENGTH, MAX_COLUMN_LENGTH];
                var childSize = rect.size / 2;
                for (int i = 0; i < MAX_ROW_LENGTH; i++)
                {
                    for (int j = 0; j < MAX_COLUMN_LENGTH; j++)
                    {
                        var childRectMin = rect.min + new Vector2(j * childSize.x, i * childSize.y);
                        var childRect = new Rect(childRectMin, childSize);
                        ChildNodes[i, j] = new QuadTreeNode(childRect, depth + 1, maxDepth);
                    }
                }
            }
        }

        public void AddItem(IQuadTreeItem item)
        {
            Items.Add(item);
            TotalItemsCount += 1;
        }

        public void AddItemToChildren(IQuadTreeItem item, PositionInQuadTree posInfo, int targetDepth, int curDepth = 0)
        {
            if (targetDepth < 0) return;
            if (curDepth == targetDepth) AddItem(item);
            else
            {
                var temp = this[posInfo.posInDepths[curDepth]];
                temp.AddItemToChildren(item, posInfo, targetDepth, curDepth + 1);
            }
            TotalItemsCount += 1;
        }

        public void RemoveItem(IQuadTreeItem item)
        {
            TotalItemsCount -= 1;
            Items.Remove(item);
        }

        public void RemoveItemInChildren(IQuadTreeItem item, PositionInQuadTree posInfo, int targetDepth, int curDepth = 0)
        {
            if (targetDepth < 0) return;
            if (curDepth == targetDepth) RemoveItem(item);
            else
            {
                var temp = this[posInfo.posInDepths[curDepth]];
                temp.RemoveItemInChildren(item, posInfo, targetDepth, curDepth + 1);
            }
            TotalItemsCount -= 1;
        }

        private static bool CorrectPosition(PositionInQuadTreeDepth position)
        {
            return position.rowIndex < MAX_ROW_LENGTH
                                && position.rowIndex >= 0
                                && position.columnIndex < MAX_COLUMN_LENGTH
                                && position.columnIndex >= 0;
        }
    }
}
