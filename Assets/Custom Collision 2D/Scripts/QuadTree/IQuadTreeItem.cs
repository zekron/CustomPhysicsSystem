using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
    public interface IQuadTreeItem
    {
        Vector2 Size
        {
            get;
        }
        Vector2 Center
        {
            get;
        }
        Rect ItemRect
        {
            get;
        }
        CustomCollider2D SelfCollider
        {
            get;
        }
        PositionInQuadTree LastPosInQuadTree
        {
            get; set;
        }
        PositionInQuadTree CurrentPosInQuadTree
        {
            get; set;
        }
    }
}
