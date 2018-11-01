using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameMap {

    public static string[] layers = {
        "LayerPassage",
        "Layer3",
        "LayerPlayer",
        "LayerEvents",
        "Layer2",
        "Layer1",
    };

    public enum Layers {
        LayerPassage,
        Layer3,
        LayerPlayer,
        LayerEvents,
        Layer2,
        Layer1

    }

    public class MapInfo {

        public string name;
        public int mapId;

        public Vector3Int minTile;
        public Vector3Int maxTile;

        public Vector3 minTileWorld;
        public Vector3 maxTileWorld;

        public int width;
        public int height;

        public List<Intersection.Polygon> passageColliders = new List<Intersection.Polygon>();  // 障碍碰撞盒数组

        public override string ToString() {
            return string.Format("{0}: name {1} minTile {2}, maxTile {3}, width {4}, height {5}, minTileWorld {6}, maxTileWorld {7}",
                base.ToString(),
                name,
                minTile,
                maxTile,
                width,
                height,
                minTileWorld,
                maxTileWorld);
        }

    }

    public GameInterpreter interpreter;
    public MapInfo mapInfo;
    public List<GameEvent> events;
    public bool needRefresh;

    public string getLayerName(Layers layer) {
        return layers[(int)layer];
    }

    /// <summary>
    /// 读取地图信息
    /// </summary>
    /// <param name="tilemapNode"></param>
    public void setupMap(GameObject tilemapNode) {
        
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
        bool isNew = this.mapInfo == null || !tilemapNode.name.Equals(this.mapInfo.name);
        if (isNew) {
            this.mapInfo = new MapInfo();
            this.mapInfo.name = tilemapNode.name;
            this.mapInfo.minTile = new Vector3Int(minX, minY, 0);
            this.mapInfo.maxTile = new Vector3Int(maxX, maxY, 0);
            this.mapInfo.width = maxX - minX;
            this.mapInfo.height = maxY - minY;
            this.mapInfo.minTileWorld = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().CellToWorld(new Vector3Int(minX, minY, 0));
            this.mapInfo.maxTileWorld = tilemapNode.transform.Find(layers[0]).GetComponent<Tilemap>().CellToWorld(new Vector3Int(maxX, maxY, 0));
        }

        // 读取碰撞信息
        Tilemap passageLayer = tilemapNode.transform.Find(getLayerName(Layers.LayerPassage)).GetComponent<Tilemap>();
        passageLayer.color = new Color(0, 0, 0, 0);
        passageLayer.CompressBounds();
        Vector3Int min = passageLayer.cellBounds.min;
        Vector3Int max = passageLayer.cellBounds.max;
        this.mapInfo.passageColliders.Clear();
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

        // 读取事件信息
        Tilemap eventLayer = tilemapNode.transform.Find(getLayerName(Layers.LayerEvents)).GetComponent<Tilemap>();
        SpriteEvent[] spriteEvents = eventLayer.GetComponentsInChildren<SpriteEvent>();
        Debug.Log(string.Format("sprites len {0}", spriteEvents.Length));
        if (isNew) {
            this.events = new List<GameEvent>();
            int i = 0;
            foreach (SpriteEvent s in spriteEvents) {
                this.events.Add((GameEvent)s.character);
                ((GameEvent)s.character).setup(i);
                i += 1;
            }
        } else {
            int i = 0;
            foreach (SpriteEvent s in spriteEvents) {
                s.setEvent(this.events[i]);
                i += 1;
            }
        }

        // 初始化事件解释器
        this.interpreter = new GameInterpreter(0, true);

        this.refresh();

        Debug.Log(string.Format("setup map {0}", this.mapInfo));
    }

    public void refresh() {

        foreach (GameEvent e in this.events) {
            e.refresh();
        }
        this.needRefresh = false;
    }

    public void update() {
        if (this.needRefresh) {
            this.refresh();
        }
        this.interpreter.update();
        // 确认键事件启动判定
        if (!this.interpreter.isRunning()) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                foreach (GameCharacterBase character in GameTemp.gamePlayer.lastHit) {
                    GameEvent e = (GameEvent)character;
                    if (e.trigger == GameInterpreter.TriggerTypes.Confirm) {
                        e.start();
                        GameTemp.gamePlayer.lastHit.Clear();
                        break;
                    }
                }
            }
        }
        // 更新事件
        foreach (GameEvent e in this.events) {
            e.update();
        }
    }

    /// <summary>
    /// 格子坐标转世界坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 getTileWorldPos(int x, int y) {
        //x = this.mapInfo.minTile.x + x;
        //y = this.mapInfo.minTile.y + y;
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
    /// <param name="character"></param>
    /// <returns></returns>
    public bool isPassable(Intersection.Polygon polygon, GameCharacterBase character) {
        // 检测图块通行
        foreach (Intersection.Polygon mapCollider in this.mapInfo.passageColliders) {
            if (Intersection.polygonPolygon(mapCollider, polygon)) {
                return false;
            }
        }
        // 检测事件通行
        character.lastHit.Clear();
        foreach (GameEvent e in this.events) {
            Intersection.Polygon eventCollider = e.currCollider();
            if (Intersection.polygonPolygon(eventCollider, polygon)) {
                if (character != null && character != e) {
                    character.lastHit.Add(e);
                }
                if (!e.erased && e.priorityType == GameCharacterBase.PRIORITIES.SAME && character != e) {
                    return false;
                }
            }
        }
        return true;
    }
}
