using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
    public interface IQuadTreeItem
    {
        Vector2 ItemSize
        {
            get;
        }
        Vector2 ItemCenter
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
