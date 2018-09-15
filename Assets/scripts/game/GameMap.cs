using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameMap {

    public static string[] layers = {
        "LayerPassage",
        "Layer3",
        "LayerEvents",
        "Layer2",
        "Layer1",
    };

    public enum Layers {
        LayerPassage,
        Layer3,
        LayerEvents,
        Layer2,
        Layer1

    }

    public class MapInfo {

        public string name;

        public Vector3Int minTile;
        public Vector3Int maxTile;

        public int width;
        public int height;

        public List<Intersection.Polygon> passageColliders = new List<Intersection.Polygon>();  // 障碍碰撞盒数组

        public override string ToString() {
            return string.Format("{0}: name {1} minTile {2}, maxTile {3}, width {4}, height {5}",
                base.ToString(),
                name,
                minTile,
                maxTile,
                width,
                height);
        }

    }

    public MapInfo mapInfo;

    public string getLayerName(Layers layer) {
        return layers[(int)layer];
    }

    /// <summary>
    /// 读取地图信息
    /// </summary>
    /// <param name="tilemapNode"></param>
    public void setupMap(GameObject tilemapNode) {
        this.mapInfo = new MapInfo();
        
        tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().CompressBounds();
        int minX = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.xMin;
        int minY = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.yMin;
        int maxX = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.xMax;
        int maxY = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.yMax;
        foreach (string layerName in layers) {
            tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().CompressBounds();
            minX = Mathf.Min(minX, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMin);
            minY = Mathf.Min(minY, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.yMin);
            maxX = Mathf.Max(maxX, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMax);
            maxY = Mathf.Max(maxY, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.yMax);
            Debug.Log(string.Format("min x {0}, layer:{1}", tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMin, layerName));
        }
        this.mapInfo.name = tilemapNode.name;
        this.mapInfo.minTile = new Vector3Int(minX, minY, 0);
        this.mapInfo.maxTile = new Vector3Int(maxX, maxY, 0);
        this.mapInfo.width = maxX - minX;
        this.mapInfo.height = maxY - minY;

        // 读取碰撞信息
        Tilemap passageLayer = ((SceneMap)SceneManager.Scene).getMapNode().transform.Find(getLayerName(Layers.LayerPassage)).GetComponent<Tilemap>();
        passageLayer.color = new Color(0, 0, 0, 0);
        passageLayer.CompressBounds();
        Vector3Int min = passageLayer.cellBounds.min;
        Vector3Int max = passageLayer.cellBounds.max;
        for (int x = min.x; x < max.x; x += 1) {
            for (int y = min.y; y < max.y; y += 1) {
                if (passageLayer.GetTile(new Vector3Int(x, y, min.z)) != null) {
                    Vector3 worldPos = passageLayer.CellToWorld(new Vector3Int(x, y, min.z));
                    Vector3 size = passageLayer.cellSize;
                    List<Vector2> points = new List<Vector2>();
                    points.Add(new Vector2(worldPos.x, worldPos.y));
                    points.Add(new Vector2(worldPos.x, worldPos.y + size.y));
                    points.Add(new Vector2(worldPos.x + size.x, worldPos.y + size.y));
                    points.Add(new Vector2(worldPos.x + size.x, worldPos.y));
                    Intersection.Polygon polygon = new Intersection.Polygon(points);
                    this.mapInfo.passageColliders.Add(polygon);
                }
            }
        }

        Debug.Log(string.Format("setup map {0}", this.mapInfo));
    }

    /// <summary>
    /// 格子坐标转世界坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 getTileWorldPos(int x, int y) {
        x = this.mapInfo.minTile.x + x;
        y = this.mapInfo.minTile.y + y;
        Vector3 worldPos = ((SceneMap)SceneManager.Scene).getMapNode().transform.Find(layers[0]).GetComponent<Tilemap>().CellToWorld(new Vector3Int(x, y, 0));
        return new Vector2(worldPos.x, worldPos.y);
    }

    /// <summary>
    /// 判断格子是否可通行
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool isPassable(int x, int y) {
        Vector2 worldPos = this.getTileWorldPos(x, y);
        return isPassable(worldPos.x, worldPos.y);
    }

    /// <summary>
    /// 判断坐标是否可通行
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool isPassable(float x, float y) {
        Tilemap passageLayer = ((SceneMap)SceneManager.Scene).getMapNode().transform.Find(getLayerName(Layers.LayerPassage)).GetComponent<Tilemap>();
        Vector3Int tilePos = passageLayer.WorldToCell(new Vector3(x, y, 0.5f));
        TileBase tile = passageLayer.GetTile(new Vector3Int(tilePos.x, tilePos.y, tilePos.z));
        return tile == null;
    }

    /// <summary>
    /// 判断碰撞盒是否可通行
    /// </summary>
    /// <param name="polygon"></param>
    /// <returns></returns>
    public bool isPassable(Intersection.Polygon polygon) {
        foreach (Intersection.Polygon mapCollider in this.mapInfo.passageColliders) {
            if (Intersection.polygonPolygon(mapCollider, polygon)) {
                return false;
            }
        }
        return true;
    }
}
