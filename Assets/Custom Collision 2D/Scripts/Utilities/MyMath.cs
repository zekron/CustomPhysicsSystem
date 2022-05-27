using UnityEngine;

public class MyMath
{
    public class LinePoint
    {
        public static Vector3 RotateAroundOrigin(Vector3 point, float angle)
        {
            return RotateAroundPoint(point, Vector3.zero, angle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="aroundPoint"></param>
        /// <param name="angle">angle in radians</param>
        /// <returns></returns>
        public static Vector3 RotateAroundPoint(Vector3 point, Vector3 aroundPoint, float angle)
        {
            return new Vector3(
                (point.x - aroundPoint.x) * Mathf.Cos(angle) - (point.y - aroundPoint.y) * Mathf.Sin(angle) + aroundPoint.x,
                (point.y - aroundPoint.y) * Mathf.Cos(angle) + (point.x - aroundPoint.x) * Mathf.Sin(angle) + aroundPoint.y);
        }

        public static Vector3 GetPointInLineWithX(Vector3 pointA, Vector3 pointB, float newPointX)
        {
            if (pointB.x - pointA.x < 0) newPointX = -newPointX;

            return new Vector2(newPointX, (newPointX - pointA.x) * (pointB.y - pointA.y) / (pointB.x - pointA.x) + pointA.y);
        }
        public static Vector3 GetPointInLineWithY(Vector3 pointA, Vector3 pointB, float newPointY)
        {
            if (pointB.x - pointA.x < 0) newPointY = -newPointY;

            return new Vector2((newPointY - pointA.y) * (pointB.x - pointA.x) / (pointB.y - pointA.y) - pointA.x, newPointY);
        }
        public static Vector3 GetPointInLineWithDistance(Vector3 pointA, Vector3 pointB, float distance)
        {
            float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x);
            return GetPointInLineWithX(pointA, pointB,
                pointB.x - pointA.x > 0 ? pointB.x + distance * Mathf.Cos(angle) : pointA.x + distance * Mathf.Cos(angle));
        }
    }
}

