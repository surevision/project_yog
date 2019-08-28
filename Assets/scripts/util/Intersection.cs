using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 辅助类，用于测试形状与形状是否相交
/// </summary>
[Serializable]
public class Intersection {

    [Serializable]
    public class Polygon {
        public List<Vector2> points = null;
        public Polygon() {
            this.points = new List<Vector2>();
        }
        public Polygon(List<Vector2> points) {
            this.points = new List<Vector2>();
            this.points.Clear();
            foreach (Vector2 point in points) {
                this.points.Add(point);
            }
        }
        public int length {
            get { return points.Count; }
        }

        public Vector2 this[int i] {
            get { return this.points[i]; }
        }

        public override string ToString() {
            string result = "Polygon: ";
            foreach (Vector2 point in points) {
                result += point.ToString();
            }
            return result;
        }
    }

	/// <summary>
	/// 平移
	/// </summary>
	/// <returns>The move.</returns>
	/// <param name="polygon">Polygon.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
    public static Polygon polygonMove(Polygon polygon, float x, float y) {
        Polygon newPolygon = new Polygon();
        for (int i = 0; i < polygon.points.Count; i += 1) {
            Vector2 point = polygon.points[i];
            newPolygon.points.Add(new Vector2(point.x + x, point.y + y));
        }
        return newPolygon;
    }

	/// <summary>
	/// 以中心为锚点缩放
	/// </summary>
	/// <returns>The scale.</returns>
	/// <param name="polygon">Polygon.</param>
	/// <param name="scaleRate">Scale rate.</param>
	public static Polygon polygonScale(Polygon polygon, float scaleRate) {
		Polygon newPolygon = new Polygon();
		// 计算多边形范围
		float left = polygon.points[0].x;
		float top = polygon.points[0].y;
		float right = polygon.points[0].x;
		float down = polygon.points[0].y;
		float x = left;
		float y = top;
		float w = right - left;
		float h = top - down;
		foreach (Vector2 point in polygon.points) {
			if (point.x < left) {
				left = point.x;
			}
			if (point.x > right) {
				right = point.x;
			}
			if (point.y < down) {
				down = point.y;
			}
			if (point.y > top) {
				top = point.y;
			}
		}
		x = left;
		y = top;
		w = right - left;
		h = top - down;

		for (int i = 0; i < polygon.points.Count; i += 1) {
			Vector2 point = new Vector2(polygon.points[i].x, polygon.points[i].y);
			// 平移坐标到左上为原点
			point.x += -x;
			point.y += -y;
			// 缩放坐标
			point.x *= scaleRate;
			point.y *= scaleRate;
			// 平移还原为缩放后坐标
			point.x += x + w / 2 * scaleRate;
			point.y += y - h / 2 * scaleRate;
			newPolygon.points.Add(point);
		}
		return newPolygon;
	}

    /// <summary>
    /// 计算两个数字是否接近相等,阈值是dvalue
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="fvalue"></param>
    /// <returns></returns>
    public static bool isApproximately(float a, float b, float fvalue) {
        float delta = a - b;
        return delta >= -fvalue && delta <= fvalue;
    }


    /// <summary>
    /// 测试线段与线段是否相交
    /// </summary>
    /// <param name="a1">The start point of the first line</param>
    /// <param name="a2">The end point of the first line</param>
    /// <param name="b1">The start point of the second line</param>
    /// <param name="b2">The end point of the second line</param>
    /// <returns></returns>
    public static bool lineLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
        // jshint camelcase:false

        float ua_t = (b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x);
        float ub_t = (a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x);
        float u_b = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);

        if (u_b != 0) {
            float ua = ua_t / u_b;
            float ub = ub_t / u_b;

            if (0 <= ua && ua <= 1 && 0 <= ub && ub <= 1) {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// 测试线段与矩形是否相交
    /// </summary>
    /// <param name="a1">The start point of the line</param>
    /// <param name="a2">The end point of the line</param>
    /// <param name="b">The rect</param>
    /// <returns></returns>
    public static bool lineRect(Vector2 a1, Vector2 a2, Rect b ) {
        Vector2 r0 = new Vector2(b.x, b.y);
        Vector2 r1 = new Vector2(b.x, b.yMax);
        Vector2 r2 = new Vector2(b.xMax, b.yMax);
        Vector2 r3 = new Vector2(b.xMax, b.y);

        if (lineLine(a1, a2, r0, r1))
            return true;

        if (lineLine(a1, a2, r1, r2))
            return true;

        if (lineLine(a1, a2, r2, r3))
            return true;

        if (lineLine(a1, a2, r3, r0))
            return true;

        return false;
    }
    
    /// <summary>
    /// 测试线段与多边形是否相交
    /// </summary>
    /// <param name="a1">The start point of the line</param>
    /// <param name="a2">The end point of the line</param>
    /// <param name="b">The polygon, a set of points</param>
    /// <returns></returns>
    public static bool linePolygon(Vector2 a1, Vector2 a2, Polygon b ) {
        int length = b.length;

        for (int i = 0; i < length; ++i) {
            Vector2 b1 = b[i];
            Vector2 b2 = b[(i + 1) % length];

            if (lineLine(a1, a2, b1, b2))
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// 测试矩形与矩形是否相交
    /// </summary>
    /// <param name="a">The first rect</param>
    /// <param name="b">The second rect</param>
    /// <returns></returns>
    public static bool rectRect(Rect a, Rect b ) {
        // jshint camelcase:false

        float a_min_x = a.x;
        float a_min_y = a.y;
        float a_max_x = a.x + a.width;
        float a_max_y = a.y + a.height;

        float b_min_x = b.x;
        float b_min_y = b.y;
        float b_max_x = b.x + b.width;
        float b_max_y = b.y + b.height;

        return a_min_x <= b_max_x &&
               a_max_x >= b_min_x &&
               a_min_y <= b_max_y &&
               a_max_y >= b_min_y
               ;
    }
    
    /// <summary>
    /// 测试矩形与多边形是否相交
    /// </summary>
    /// <param name="a">The rect</param>
    /// <param name="b">The polygon, a set of points</param>
    /// <returns></returns>
    public static bool rectPolygon(Rect a, Polygon b ) {
        int i, l;
        Vector2 r0 = new Vector2(a.x, a.y);
        Vector2 r1 = new Vector2(a.x, a.yMax);
        Vector2 r2 = new Vector2(a.xMax, a.yMax);
        Vector2 r3 = new Vector2(a.xMax, a.y);

        // intersection check
        if (linePolygon(r0, r1, b))
            return true;

        if (linePolygon(r1, r2, b))
            return true;

        if (linePolygon(r2, r3, b))
            return true;

        if (linePolygon(r3, r0, b))
            return true;

        // check if a contains b
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(a.xMin, a.yMin));
        points.Add(new Vector2(a.xMin, a.yMax));
        points.Add(new Vector2(a.xMax, a.yMax));
        points.Add(new Vector2(a.xMax, a.yMin));
        Polygon polygonA = new Polygon(points);
        for (i = 0, l = b.length; i < l; ++i) {
            if (pointInPolygon(b[i], polygonA))
                return true;
        }

        // check if b contains a
        if (pointInPolygon(r0, b))
            return true;

        if (pointInPolygon(r1, b))
            return true;

        if (pointInPolygon(r2, b))
            return true;

        if (pointInPolygon(r3, b))
            return true;

        return false;
    }
    
    /// <summary>
    /// 测试多边形与多边形是否相交
    /// </summary>
    /// <param name="a">The first polygon, a set of points</param>
    /// <param name="b">The second polygon, a set of points</param>
    /// <returns></returns>
    public static bool polygonPolygon(Polygon a, Polygon b ) {
        int i, l;

        // check if a intersects b
        for (i = 0, l = a.length; i < l; ++i) {
            Vector2 a1 = a[i];
            Vector2 a2 = a[(i + 1) % l];

            if (linePolygon(a1, a2, b))
                return true;
        }

        // check if a contains b
        for (i = 0, l = b.length; i < l; ++i) {
            if (pointInPolygon(b[i], a))
                return true;
        }

        // check if b contains a
        for (i = 0, l = a.length; i < l; ++i) {
            if (pointInPolygon(a[i], b))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 测试一个点是否在一个多边形中
    /// </summary>
    /// <param name="point">The point</param>
    /// <param name="polygon">The polygon, a set of points</param>
    /// <returns></returns>
    public static bool pointInPolygon(Vector2 point, Polygon polygon) {
        bool inside = false;
        float x = point.x;
        float y = point.y;

        // use some raycasting to test hits
        // https://github.com/substack/point-in-polygon/blob/master/index.js
        int length = polygon.length;

        for (int i = 0, j = length - 1; i < length; j = i++) {
            float xi = polygon[i].x, yi = polygon[i].y,
                xj = polygon[j].x, yj = polygon[j].y;
            bool intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);

            if (intersect) inside = !inside;
        }

        return inside;
    }

    /// <summary>
    /// 计算点到直线的距离。如果这是一条线段并且垂足不在线段内，则会计算点到线段端点的距离。
    /// </summary>
    /// <param name="point">The point</param>
    /// <param name="start">The start point of line</param>
    /// <param name="end">The end point of line</param>
    /// <param name="isSegment">whether this line is a segment</param>
    /// <returns></returns>
    public static float pointLineDistance(Vector2 point, Vector2 start, Vector2 end, bool isSegment) {
        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float d = dx * dx + dy * dy;
        float t = ((point.x - start.x) * dx + (point.y - start.y) * dy) / d;
        Vector2 p;

        if (!isSegment) {
            p = new Vector2(start.x + t * dx, start.y + t * dy);
        } else {
            if (d != 0) {
                if (t < 0) p = start;
                else if (t > 1) p = end;
                else p = new Vector2(start.x + t * dx, start.y + t * dy);
            } else {
                p = start;
            }
        }

        dx = point.x - p.x;
        dy = point.y - p.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
