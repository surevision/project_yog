using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GameCharacterBase
{
    public enum DIRS
    {
        NONE = 0,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 6,
        UP = 8
    }
    public enum PRIORITIES
    {
        BELOW,
        SAME,
        UPPER
    }
    public int id;                              // ID
    public float x;                               // 地图 X 坐标（格子坐标）
    public float y;                               // 地图 Y 坐标（格子坐标）
    public float realX;                         // 地图 X 坐标（实际像素坐标）
    public float realY;                         // 地图 Y 坐标（实际像素坐标）
    public int moveSpeed = 3;                   // 移动速度
    public string characterName;                // 文件名
    public int moveFrequency = 0;               // 移动频度
    public bool walkAnime = true;               // 步行动画
    public bool stepAnime = false;              // 踏步动画
    public bool directionFix = false;           // 固定朝向
    public int opacity = 255;                   // 不透明度
    public int angle = 0;                   // 角度
    public DIRS direction = DIRS.DOWN;          // 方向
    public int pattern = 1;                     // 图案
    public PRIORITIES priorityType = PRIORITIES.BELOW;            // 优先级类型
    public bool through = false;                    // 穿透
    public int animationId = -1;                    // 动画 ID
    public int balloonId = -1;                      // 心情图标 ID
    public bool transparent = false;                // 透明状态

    protected DIRS originalDirection = DIRS.DOWN;      // 原方向
    protected int originalPattern = 1;                 // 原图案
    protected float animeCount = 0;                    // 动画计数
    protected int stopCount = 0;                       // 停止计数
    protected bool locked = false;                     // 锁的标志
    protected DIRS prelockDirection = DIRS.NONE;       // 被锁上前的方向
    protected bool moveSucceed = true;                 // 移动成功的标志

    public bool isDirty = false;

    protected Intersection.Polygon colliderPolygon;

    protected List<GameInterpreter.MoveRoute> moveRoute;  // 移动路径信息
    protected int moveByRouteIndex; // 移动路径指令索引
    protected int moveWaitCount;    // 移动指令等待计数

    [NonSerialized]
    private List<GameCharacterBase> _lastHit;
    public List<GameCharacterBase> lastHit {
        get { return _lastHit; }
        set { _lastHit = value; }
    }


    public GameCharacterBase() {
        this.originalDirection = DIRS.DOWN;       // 原方向
        this.originalPattern = 1;                 // 原图案
        this.animeCount = 0;                      // 动画计数
        this.stopCount = 0;                       // 停止计数
        this.locked = false;                      // 锁的标志
        this.prelockDirection = DIRS.NONE;        // 被锁上前的方向
        this.moveSucceed = true;                  // 移动成功的标志

        this.moveRoute = null;
        this.moveByRouteIndex = -1;
        this.moveWaitCount = 0;

        this.lastHit = new List<GameCharacterBase>();


        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 0.16f));
        points.Add(new Vector2(0.16f, 0.16f));
        points.Add(new Vector2(0.16f, 0));
        this.colliderPolygon = new Intersection.Polygon(points);
    }

    /// <summary>
    /// 初始化包围盒数据
    /// </summary>
    /// <param name="sprite"></param>
    public virtual void setupCollider(SpriteCharacter sprite) {
    }

    public Intersection.Polygon currCollider() {
        return Intersection.polygonMove(this.colliderPolygon, this.realX, this.realY);
    }

    /// <summary>
    /// 每帧移动距离
    /// </summary>
    /// <returns></returns>
    protected float distancePerFrame() {
        return Mathf.Pow(2, this.moveSpeed) / 256 * 0.16f;
    }

    /// <summary>
    /// 更新
    /// </summary>
    public virtual void update() {
        this.updateAnimation();
        if (this.isMoving()) {
            this.updateMove();
            return;
        }
        this.updateStop();
    }
    /// <summary>
    /// 是否在移动中
    /// </summary>
    /// <returns></returns>
    public virtual bool isMoving() {
        return this.realX != this.x || this.realY != this.y;
    }

    /// <summary>
    /// 是否在强制移动中
    /// </summary>
    /// <returns></returns>
    public virtual bool isForcedMoving() {
        return this.moveByRouteIndex != -1;
    }

    /// <summary>
    /// 增加步数
    /// </summary>
    protected virtual void increaseMove() {
        this.stopCount = 0;
    }

    /// <summary>
    /// 更新脚步动画
    /// </summary>
    protected virtual void updateAnimation() {
        this.updateAnimeCount();
        if (this.animeCount > 24 - this.moveSpeed * 2) {
            this.updateAnimePattern();  // 改图
            this.animeCount = 0;
            this.isDirty = true;
        }
    }

    protected virtual void updateAnimeCount() {
        if (this.isMoving() && this.walkAnime) {
            // 行走的情况
            this.animeCount += 1.5f;
        } else if (this.stepAnime || this.pattern != this.originalPattern) {
            this.animeCount += 1.0f;
        }
    }

    /// <summary>
    /// 更新动画计数
    /// </summary>
    protected virtual void updateAnimePattern() {
        if (!this.stepAnime && this.stopCount > 0) {
            // 无踏步动画
            this.pattern = this.originalPattern;
        } else {
            this.pattern = (this.pattern + 1) % 4;
        }
        //Debug.Log(string.Format("pattern {0}", this, pattern));

    }

    /// <summary>
    /// 更新移动位置
    /// </summary>
    protected virtual void updateMove() {
        if (this.x < this.realX) {
            this.realX = Mathf.Max(this.realX - this.distancePerFrame(), this.x);
        }
        if (this.x > this.realX) {
            this.realX = Mathf.Min(this.realX + this.distancePerFrame(), this.x);
        }
        if (this.y < this.realY) {
            this.realY = Mathf.Max(this.realY - this.distancePerFrame(), this.y);
        }
        if (this.y > this.realY) {
            this.realY = Mathf.Min(this.realY + this.distancePerFrame(), this.y);
        }
    }

    /// <summary>
    /// 更新停止
    /// </summary>
    protected virtual void updateStop() {
        if (!this.locked) {
            this.stopCount += 1;
        }
        // 更新强制移动
        if (this.isForcedMoving()) {
            this.updateRouteMove();
            if (!this.isMoving() && this.moveByRouteIndex >= this.moveRoute.Count) {
                this.moveByRouteIndex = -1;
            }
        }
    }

    protected virtual void updateRouteMove() {
        if (this.moveWaitCount > 0) {
            this.moveWaitCount += 1;
            return;
        }
        this.moveByRoute();
        this.moveByRouteIndex += 1;

    }

    public virtual float offsetStepX() {
        return getBaseStep();
    }
    public virtual float offsetStepY() {
        return getBaseStep();
    }

    protected virtual float getBaseStep() {
        return 16f / 100f;
    }

    protected virtual float getStep() {
        return getBaseStep() / 8;
    }

    /// <summary>
    /// 朝指定方向移动是否可通行
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private bool isPassable(float x, float y, DIRS dir) {
        float step = this.getStep();
        Intersection.Polygon testPolygon = this.currCollider();
        if (dir == DIRS.DOWN) {
            testPolygon = Intersection.polygonMove(testPolygon, 0, -step);
        }
        if (dir == DIRS.LEFT) {
            testPolygon = Intersection.polygonMove(testPolygon, -step, 0);
        }
        if (dir == DIRS.RIGHT) {
            testPolygon = Intersection.polygonMove(testPolygon, step, 0);
        }
        if (dir == DIRS.UP) {
            testPolygon = Intersection.polygonMove(testPolygon, 0, step);
        }
        return GameTemp.gameMap.isPassable(testPolygon, this);
    }

    /// <summary>
    /// 移动指定距离是否可通行
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="distanceX"></param>
    /// <param name="distanceY"></param>
    /// <returns></returns>
    public bool isPassableByDistance(float x, float y, DIRS dir, float distanceX, float distanceY) {
        Intersection.Polygon testPolygon = this.currCollider();
        Debug.Log(testPolygon.points[0].x);
        Debug.Log(testPolygon.points[0].y);
        if (dir == DIRS.DOWN) {
            testPolygon = Intersection.polygonMove(testPolygon, 0, -distanceY);
        }
        if (dir == DIRS.LEFT) {
            testPolygon = Intersection.polygonMove(testPolygon, -distanceX, 0);
        }
        if (dir == DIRS.RIGHT) {
            testPolygon = Intersection.polygonMove(testPolygon, distanceX, 0);
        }
        if (dir == DIRS.UP) {
            testPolygon = Intersection.polygonMove(testPolygon, 0, distanceY);
        }
        Debug.Log(testPolygon.points[0].x);
        Debug.Log(testPolygon.points[0].y);
        Debug.Log(string.Format("GameTemp.gameMap.isPassable(testPolygon, this) {0}, {1}, {2}, {3}, {4}, {5}", 
            GameTemp.gameMap.isPassable(testPolygon, this), testPolygon, this.currCollider(), distanceX, distanceY, dir));
        return GameTemp.gameMap.isPassable(testPolygon, this);
    }

    /// <summary>
    /// 向指定方向行走
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool moveStraight(DIRS dir) {
        if (dir == DIRS.LEFT || dir == DIRS.RIGHT) {
            this.direction = dir;
        }
        this.isDirty = true;
        if (this.isPassable(this.x, this.y, dir)) {
            float step = this.getStep();
            if (dir == DIRS.DOWN) {
                this.y -= step;
            }
            if (dir == DIRS.LEFT) {
                this.x -= step;
            }
            if (dir == DIRS.RIGHT) {
                this.x += step;
            }
            if (dir == DIRS.UP) {
                this.y += step;
            }
            this.increaseMove();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 向指定方向行走一格
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool moveGridStraight(DIRS dir) {
        Debug.Log(string.Format("moveGridStraight {0}", dir));
        if (dir == DIRS.LEFT || dir == DIRS.RIGHT) {
            this.direction = dir;
        }
        this.isDirty = true;
        float step = this.getStep();
        float stepX = 0f;
        float stepY = 0f;
        if (dir == DIRS.LEFT) {
            if (this.x < 0) {
                stepX = (this.getBaseStep() - Mathf.Abs(this.x) % this.getBaseStep()) % this.getBaseStep() + this.getBaseStep();
            } else {
                stepX = Mathf.Abs(this.x) % this.getBaseStep() + this.getBaseStep();
            }
        }
        if (dir == DIRS.RIGHT) {
            if (this.x < 0) {
                stepX = Mathf.Abs(this.x) % this.getBaseStep() + this.getBaseStep();
            } else {
                stepX = (this.getBaseStep() - Mathf.Abs(this.x) % this.getBaseStep()) % this.getBaseStep() + this.getBaseStep();
            }
        }
        if (dir == DIRS.DOWN) {
            if (this.y < 0) {
                stepY = (this.getBaseStep() - Mathf.Abs(this.y) % this.getBaseStep()) % this.getBaseStep() + this.getBaseStep();
            } else {
                stepY = Mathf.Abs(this.y) % this.getBaseStep() + this.getBaseStep();
            }
        }
        if (dir == DIRS.UP) {
            if (this.y < 0) {
                stepY = Mathf.Abs(this.y) % this.getBaseStep() + this.getBaseStep();
            } else {
                stepY = (this.getBaseStep() - Mathf.Abs(this.y) % this.getBaseStep()) % this.getBaseStep() + this.getBaseStep();
            }
        }
        if (stepX > this.getBaseStep()) {
            stepX = this.getBaseStep();
        }
        if (stepY > this.getBaseStep()) {
            stepY = this.getBaseStep();
        }
        while ((stepY > 0 || stepX > 0) && this.isPassableByDistance(this.x, this.y, dir, step, step)) {
            if (dir == DIRS.DOWN) {
                this.y -= step;
                stepY -= step;
            }
            if (dir == DIRS.LEFT) {
                this.x -= step;
                stepX -= step;
            }
            if (dir == DIRS.RIGHT) {
                this.x += step;
                stepX -= step;
            }
            if (dir == DIRS.UP) {
                this.y += step;
                stepY -= step;
            }
            this.increaseMove();
        }
        return true;
    }

    /// <summary>
    /// 根据移动指令行走
    /// </summary>
    public virtual void moveByRoute() {
        if (this.isMoving()) {
            return;
        }
        if (this.moveByRouteIndex < this.moveRoute.Count) {
            GameInterpreter.MoveRoute cmd = this.moveRoute[this.moveByRouteIndex];
            switch (cmd.code) {
                case GameInterpreter.MoveRoute.Cmd.ROUTE_END:
                    this.processRouteEnd();
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_DOWN:
                    this.moveGridStraight(DIRS.DOWN);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_LEFT:
                    this.moveGridStraight(DIRS.LEFT);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_RIGHT:
                    this.moveGridStraight(DIRS.RIGHT);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_UP:
                    this.moveGridStraight(DIRS.UP);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_LOWER_L:
                    //move_diagonal(4, 2)
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_LOWER_R:
                    //move_diagonal(6, 2)
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_UPPER_L:
                    //move_diagonal(4, 8)
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_UPPER_R:
                    //move_diagonal(6, 8)
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_RANDOM:
                    //move_random
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_TOWARD:
                    //move_toward_player
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_AWAY:
                    //move_away_from_player
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_FORWARD:
                    //move_forward
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_BACKWARD:
                    //move_backward
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_JUMP:
                    //jump(params[0], params[1])
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_WAIT:
                    this.moveWaitCount = int.Parse(cmd.args[0]);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_DOWN:
                    this.direction = DIRS.DOWN;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_LEFT:
                    this.direction = DIRS.LEFT;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_RIGHT:
                    this.direction = DIRS.RIGHT;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_UP:
                    this.direction = DIRS.UP;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_90D_R:
                    //turn_right_90
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_90D_L:
                    //turn_left_90
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_180D:
                    //turn_180
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_90D_R_L:
                    //turn_right_or_left_90
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_RANDOM:
                    //turn_random
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_TOWARD:
                    //turn_toward_player
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_AWAY:
                    //turn_away_from_player
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_SWITCH_ON:
                    //$game_switches[params[0]] = true
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_SWITCH_OFF:
                    //$game_switches[params[0]] = false
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_SPEED:
                    //@move_speed = params[0]
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_FREQ:
                    //@move_frequency = params[0]
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_WALK_ANIME_ON:
                    this.walkAnime = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_WALK_ANIME_OFF:
                    this.walkAnime = false;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_STEP_ANIME_ON:
                    this.stepAnime = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_STEP_ANIME_OFF:
                    this.stepAnime = false;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_DIR_FIX_ON:
                    this.directionFix = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_DIR_FIX_OFF:
                    this.directionFix = false;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_THROUGH_ON:
                    this.through = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_THROUGH_OFF:
                    this.through = false;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TRANSPARENT_ON:
                    this.transparent = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TRANSPARENT_OFF:
                    this.transparent = false;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_GRAPHIC:
                    this.characterName = cmd.args[0];
                    this.originalPattern = 1;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_OPACITY:
                    this.opacity = int.Parse(cmd.args[0]);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_BLENDING:
                    //@blend_type = params[0]
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_PLAY_SE:
                    AudioManager.PlaySE(cmd.args[0]);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_SCRIPT:
                    //eval(params[0])
                    break;
            }
        }
    }

    /// <summary>
    /// 设置移动路径
    /// </summary>
    /// <param name="route"></param>
    public virtual void setMoveRoute(List<GameInterpreter.MoveRoute> route) {
        this.moveRoute = route;
        this.moveByRouteIndex = 0;
    }

    /// <summary>
    /// 移动路径 移动路径结束
    /// </summary>
    protected void processRouteEnd() {
        this.moveByRouteIndex = -1;
    }

    public virtual float screenX() {
        return this.realX;
    }

    public virtual float screenY() {
        return this.realY;
    }

    public void setPos(float x, float y) {
        this.x = x;
        this.y = y;
        this.realX = x;
        this.realY = y;
    }

    public void lockup() {
        this.locked = true;
    }
    public void unlock() {
        this.locked = false;
    }

    public override string ToString() {
        return base.ToString() + " \r\n" +
            "x " + this.x.ToString() + " \r\n" +
            "y " + this.y.ToString() + " \r\n" +
            "realX " + this.realX.ToString() + " \r\n" +
            "realY " + this.realY.ToString() + " \r\n" +
            "direction " + this.direction.ToString();
    }
}
