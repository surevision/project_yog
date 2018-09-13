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

    public class MapInfo {

        public string name;

        public Vector3 minTile;
        public Vector3 maxTile;

        public float width;
        public float height;

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

    /// <summary>
    /// 读取地图信息
    /// </summary>
    /// <param name="tilemapNode"></param>
    public void setupMap(GameObject tilemapNode) {
        this.mapInfo = new MapInfo();
        
        tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().CompressBounds();
        float minX = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.xMin;
        float minY = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.yMin;
        float maxX = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.xMax;
        float maxY = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().cellBounds.yMax;
        foreach (string layerName in layers) {
            tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().CompressBounds();
            minX = Mathf.Min(minX, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMin);
            minY = Mathf.Min(minY, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.yMin);
            maxX = Mathf.Max(maxX, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMax);
            maxY = Mathf.Max(maxY, tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.yMax);
            Debug.Log(string.Format("min x {0}, layer:{1}", tilemapNode.transform.Find(layerName).GetComponent<Tilemap>().cellBounds.xMin, layerName));
        }
        this.mapInfo.name = tilemapNode.name;
        this.mapInfo.minTile = new Vector3(minX, minY, 0);
        this.mapInfo.maxTile = new Vector3(maxX, maxY, 0);
        this.mapInfo.width = maxX - minX;
        this.mapInfo.height = maxY - minY;

        Debug.Log(string.Format("setup map {0}", this.mapInfo));
    }
}
