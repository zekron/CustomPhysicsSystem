using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
    public class QuadTree
    {
        private Rect _worldRect;
        private int _maxDepth;
        /// <summary>
        /// Store the grid size for each depth, index is depth
        /// </summary>
        private Vector2[] _gridSizes;
        private QuadTreeNode _root;

        private List<IQuadTreeItem> _cacheItemsFound = new List<IQuadTreeItem>();
        private Queue<QuadTreeNode> _traverseNodeQueue = new Queue<QuadTreeNode>();

        public bool NeedDebug
        {
            get; set;
        }

        public int MaxDepth => _maxDepth;

        public Rect WorldRect => _worldRect;

        //Support rectangle range
        public QuadTree(Rect worldRect, int maxDepth)
        {
            NeedDebug = false;
            _worldRect = worldRect;
            _maxDepth = maxDepth;
            _gridSizes = new Vector2[maxDepth + 1];
            for (int i = 0; i <= maxDepth; i++)
            {
                var width = worldRect.width / (Mathf.Pow(2, i));
                var height = worldRect.height / (Mathf.Pow(2, i));
                _gridSizes[i] = new Vector2(width, height);
            }
            _root = new QuadTreeNode(_worldRect, 0, maxDepth);
        }

        public static QuadTree Create(Rect worldRect, int maxDepth)
        {
            return new QuadTree(worldRect, maxDepth);
        }

        internal int GetDepth(Vector2 size)
        {
            for (int i = _gridSizes.Length - 1; i >= 0; i--)
            {
                if (size.x <= _gridSizes[i].x && size.y <= _gridSizes[i].y)
                {
                    return i;
                }
            }

            Debug.LogError("Size is bigger than QuadTree Max Range");
            return -1;
        }

        /// <summary>
        /// <para>根据物体的 <paramref name="center"/> 和 <paramref name="size"/> 更新 <paramref name="posInfo"/></para>
        /// <para>Refresh <paramref name="posInfo"/> based on <paramref name="center"/> and <paramref name="size"/></para>
        /// </summary>
        /// <param name="center">物体的中心坐标</param>
        /// <param name="size">物体的大小(Rect.width, Rect.height)</param>
        /// <param name="posInfo">需要更新的位置信息</param>
        internal void GetPosInfo(Vector2 center, Vector2 size, ref PositionInQuadTree posInfo)
        {
            posInfo.Reset();

            var depth = GetDepth(size);
            if (depth == 0) return;

            var gridsize = _gridSizes[depth];

            int row = Mathf.FloorToInt((center.y - _worldRect.yMin) / gridsize.y);
            int column = Mathf.FloorToInt((center.x - _worldRect.xMin) / gridsize.x);

            var storeDepth = posInfo.storeDepth = depth;

            for (int i = 0; i < depth; i++)
            {
                storeDepth--;
                posInfo.posInDepths[storeDepth].rowIndex = (row >> i) & 1;
                posInfo.posInDepths[storeDepth].columnIndex = (column >> i) & 1;
            }
        }

        /// <summary>
        /// 更新 <paramref name="item"/> 在四叉树中的位置
        /// </summary>
        /// <param name="item"></param>
        internal void UpdateItem(IQuadTreeItem item)
        {
            var newPosInfo = item.CurrentPosInQuadTree;
            GetPosInfo(item.ItemCenter, item.ItemSize, ref newPosInfo);
            if (newPosInfo.Equals(item.LastPosInQuadTree) && !newPosInfo.inRoot) return;

            var lastPosInfo = item.LastPosInQuadTree;
            if (lastPosInfo.posInDepths != null)
            {
                //从上一帧位置信息中移除当前物体
                _root.RemoveItemInChildren(item, lastPosInfo, lastPosInfo.storeDepth - 1);
            }
            if (NeedDebug)
            {
                Debug.LogFormat("Remove item in:\n{0}", lastPosInfo);
                Debug.LogFormat("Add item in:\n{0}", newPosInfo);
            }

            //item.currentPosInQuadTree.Copy( newPosInfo );
            lastPosInfo.CopyFrom(newPosInfo);
            item.LastPosInQuadTree = lastPosInfo;

            //更新物体的当前帧位置信息
            if (item.LastPosInQuadTree.inRoot)
            {
                _root.AddItem(item);
                return;
            }
            _root.AddItemToChildren(item, lastPosInfo, lastPosInfo.storeDepth - 1);
        }

        /// <summary>
        /// Get items that might intersect with the rayRect
        /// </summary>
        internal List<IQuadTreeItem> GetItems(Rect rayRect)
        {
            _cacheItemsFound.Clear();
            _traverseNodeQueue.Clear();
            AddNodeToTraverseQueue(_root, rayRect);
            while (_traverseNodeQueue.Count > 0)
            {
                var currentChild = _traverseNodeQueue.Dequeue();
                foreach (IQuadTreeItem item in currentChild.Items)
                {
                    if (item.ItemRect.Intersects(rayRect))
                    {
                        _cacheItemsFound.Add(item);
                    }
                }

                if (currentChild.IsLeaf == false)
                {
                    foreach (var node in currentChild.ChildNodes)
                    {
                        AddNodeToTraverseQueue(node, rayRect);
                    }
                }
            }
            return _cacheItemsFound;
        }

        private void AddNodeToTraverseQueue(QuadTreeNode node, Rect rayRect)
        {
            if (node.TotalItemsCount > 0 && node.NodeLooseRect.Intersects(rayRect))
            {
                _traverseNodeQueue.Enqueue(node);
            }
        }
    }
}
