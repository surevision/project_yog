﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GameCharacterBase
{
	[Serializable]
	[XLua.LuaCallCSharp]
    public enum DIRS
    {
        NONE = 0,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 6,
        UP = 8
    }

    [Serializable]
    public enum PRIORITIES
    {
		[EnumAttirbute("在角色之下")]
		BELOW,
		[EnumAttirbute("和角色同层")]
		SAME,
		[EnumAttirbute("在角色之上")]
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
    public int angle = 0;                   	// 角度
    public DIRS direction = DIRS.DOWN;          // 方向
    public int pattern = 1;                     // 图案
    public PRIORITIES priorityType = PRIORITIES.BELOW;				// 优先级类型
    public bool through = false;                    // 穿透
	public List<string> loopAnimNames = new List<string>();			// 循环的动画文件名
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
		this.loopAnimNames = new List<string>();	// 循环的动画
        this.moveSucceed = true;                  // 移动成功的标志
        this.moveRoute = null;
        this.moveByRouteIndex = -1;
        this.moveWaitCount = 0;

        this.lastHit = new List<GameCharacterBase>();


        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(0, 0));
		points.Add(new Vector2(0, (Util.GRID_WIDTH - 1) / Util.PPU));
		points.Add(new Vector2((Util.GRID_WIDTH - 1) / Util.PPU, (Util.GRID_WIDTH - 1) / Util.PPU));
		points.Add(new Vector2((Util.GRID_WIDTH - 1) / Util.PPU, 0));
        this.colliderPolygon = new Intersection.Polygon(points);
    }

    /// <summary>
    /// 初始化包围盒数据
    /// </summary>
    /// <param name="sprite"></param>
    public virtual void setupCollider(SpriteCharacter sprite) {
    }

	/// <summary>
	/// 当前位置碰撞盒
	/// </summary>
	/// <returns>The collider.</returns>
    public Intersection.Polygon currCollider() {
        return Intersection.polygonMove(this.colliderPolygon, this.realX, this.realY);
    }

	/// <summary>
	/// 目标位置碰撞盒
	/// </summary>
	/// <returns>The collider.</returns>
	public Intersection.Polygon targetCollider() {
		return Intersection.polygonMove(this.colliderPolygon, this.x, this.y);
	}
	/// <summary>
	/// 取用于判断接近重合的包围盒
	/// </summary>
	/// <returns>The middle collider.</returns>
	/// <param name="scaleRate">Scale rate.</param>
	public Intersection.Polygon currMidCollider(float scaleRate = 0.5f) {
		return Intersection.polygonScale(this.currCollider(), scaleRate);
	}

	protected virtual float getMoveSpeed() {
		return this.moveSpeed;
	}

    /// <summary>
    /// 每帧移动距离
    /// </summary>
    /// <returns></returns>
    protected float distancePerFrame() {
		return Mathf.Pow(2, this.getMoveSpeed()) / 256 * Util.GRID_WIDTH / Util.PPU;
    }

    /// <summary>
    /// 更新
    /// </summary>
	public virtual void update() {
		if (!this.directionFix) {
			this.updateAnimation();
		}
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
		if (this.animeCount > 24 - this.getMoveSpeed() * 2) {
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

    /// <summary>
    /// 设置到图块位置
    /// </summary>
    /// <param name="cellPos"></param>
    public virtual void setCellPosition(Vector2Int cellPos) {
        Vector2 pos = GameTemp.gameMap.getTileWorldPos(cellPos.x, cellPos.y);
        this.x = pos.x;
        this.y = pos.y;
        this.realX = pos.x;
        this.realY = pos.y;
        Debug.Log(pos);
    }

    protected virtual void updateRouteMove() {
        if (this.moveWaitCount > 0) {
            this.moveWaitCount -= 1;
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
        return Util.GRID_WIDTH / Util.PPU;
    }

    public virtual float getStep() {
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
		Intersection.Polygon testPolygon = Intersection.polygonMove(this.colliderPolygon, x, y);//this.currCollider();
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
		return GameTemp.gameMap.isPassable(testPolygon, this) && GameTemp.gameMap.isPreOtherEventsPassable(testPolygon, this);
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
		Intersection.Polygon testPolygon = Intersection.polygonMove(this.colliderPolygon, x, y);//this.currCollider();
//        Debug.Log(testPolygon.points[0].x);
//        Debug.Log(testPolygon.points[0].y);
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
//        Debug.Log(testPolygon.points[0].x);
//        Debug.Log(testPolygon.points[0].y);
//        Debug.Log(string.Format("GameTemp.gameMap.isPassable(testPolygon, this) {0}, {1}, {2}, {3}, {4}, {5}", 
//            GameTemp.gameMap.isPassable(testPolygon, this), testPolygon, this.currCollider(), distanceX, distanceY, dir));
		return GameTemp.gameMap.isPassable(testPolygon, this) && GameTemp.gameMap.isPreOtherEventsPassable(testPolygon, this);
    }

    /// <summary>
    /// 向指定方向行走
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool moveStraight(DIRS dir) {
//        if (dir == DIRS.LEFT || dir == DIRS.RIGHT) {
            this.direction = dir;
//        }
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
		if (dir == DIRS.NONE) {
			return true;
		}
        //if (dir == DIRS.LEFT || dir == DIRS.RIGHT) {
            this.direction = dir;
        //}
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
		while ((((dir == DIRS.UP || dir == DIRS.DOWN) && stepY > 0) || ((dir == DIRS.LEFT || dir == DIRS.RIGHT) && stepX > 0)) && 
				this.isPassableByDistance(this.x, this.y, dir, step, step)) {
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
	/// 取反向
	/// </summary>
	/// <returns>The reverse dir.</returns>
	/// <param name="dir">Dir.</param>
	public static DIRS getReverseDir(DIRS dir) {
		return (DIRS)(10 - (int)dir);
	}

	/// <summary>
	/// 向反向前进一格
	/// </summary>
	/// <returns><c>true</c>, if grid straight reverse was moved, <c>false</c> otherwise.</returns>
	/// <param name="currDir">Curr dir.</param>
	public bool moveGridStraightReverse(DIRS currDir) {
		DIRS reverseDir = getReverseDir(currDir);
		bool result = moveGridStraight(reverseDir);
		this.direction = currDir;
		return result;
	}

	/// <summary>
	/// 向指定角色移动
	/// </summary>
	/// <returns><c>true</c>, if towards was moved, <c>false</c> otherwise.</returns>
	/// <param name="charId">Char identifier.</param>
	public bool moveTowards(int charId) {
		GameCharacterBase character = GameTemp.gameMap.interpreter.getCharacter(charId);
		float targetX = character.realX;
		float targetY = character.realY;
		float offsetX = Mathf.Abs(targetX - this.realX);
		float offsetY = Mathf.Abs(targetY - this.realY);
		DIRS dir = DIRS.NONE;
		if (this.isPassable(this.realX, this.realY, DIRS.UP) &&
		    this.isPassable(this.realX, this.realY, DIRS.DOWN) &&
		    this.isPassable(this.realX, this.realY, DIRS.LEFT) &&
		    this.isPassable(this.realX, this.realY, DIRS.RIGHT)) {
			// 四面无障碍
			if (offsetX > offsetY) {
				if (this.realX < targetX) {
					dir = DIRS.RIGHT;
				} else {
					dir = DIRS.LEFT;
				}
			} else {
				if (this.realY < targetY) {
					dir = DIRS.UP;
				} else {
					dir = DIRS.DOWN;
				}
			}
		} else {
			if (offsetX > offsetY && (new System.Random()).Next(100) > 30) {
				if (this.realX < targetX) {
					if ((new System.Random()).Next(100) > 20) {
						dir = DIRS.RIGHT;
					} else {
						dir = DIRS.LEFT;
					}
				} else {
					if ((new System.Random()).Next(100) > 20) {
						dir = DIRS.LEFT;
					} else {
						dir = DIRS.RIGHT;
					}
				}
			} else {
				if (this.realY < targetY) {
					if ((new System.Random()).Next(100) > 20) {
						dir = DIRS.UP;
					} else {
						dir = DIRS.DOWN;
					}
				} else {
					if ((new System.Random()).Next(100) > 20) {
						dir = DIRS.DOWN;
					} else {
						dir = DIRS.UP;
					}
				}
			}
		}
		return this.moveGridStraight(dir);
	}

	/// <summary>
	/// 前进一格
	/// </summary>
	/// <returns><c>true</c>, if forward was moved, <c>false</c> otherwise.</returns>
	public bool moveForward() {
		return this.moveGridStraight(this.direction);
	}

	/// <summary>
	/// 后退一格
	/// </summary>
	/// <returns><c>true</c>, if backward was moved, <c>false</c> otherwise.</returns>
	public bool moveBackward() {
		return this.moveGridStraightReverse(this.direction);
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
					this.moveTowards(int.Parse(cmd.args[0]));
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_AWAY:
                    //move_away_from_player
                    break;
				case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_FORWARD:
					this.moveForward();
                    break;
				case GameInterpreter.MoveRoute.Cmd.ROUTE_MOVE_BACKWARD:
					this.moveBackward();
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_JUMP:
                    //jump(params[0], params[1])
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_WAIT:
                    this.moveWaitCount = int.Parse(cmd.args[0]);
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_DOWN:
                    this.direction = DIRS.DOWN;
                    this.isDirty = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_LEFT:
                    this.direction = DIRS.LEFT;
                    this.isDirty = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_RIGHT:
                    this.direction = DIRS.RIGHT;
                    this.isDirty = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TURN_UP:
                    this.direction = DIRS.UP;
                    this.isDirty = true;
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
					this.moveSpeed = int.Parse(cmd.args[0]);
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
					this.isDirty = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_TRANSPARENT_OFF:
					this.transparent = false;
					this.isDirty = true;
                    break;
                case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_GRAPHIC:
                    this.characterName = cmd.args[0];
					this.originalPattern = 1;
					this.isDirty = true;
                    break;
				case GameInterpreter.MoveRoute.Cmd.ROUTE_CHANGE_OPACITY:
					this.opacity = int.Parse(cmd.args[0]);
					this.isDirty = true;
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

	/// <summary>
	/// Plaies the anime.
	/// </summary>
	/// <param name="animName">Animation name.</param>
	/// <param name="loop">If set to <c>true</c> loop.</param>
	public virtual void playAnim(string animName, bool loop = false) {
		if (loop) {
			this.loopAnimNames.Add(animName);
		}
	}

	/// <summary>
	/// 停止一个循环动画，不考虑序号
	/// </summary>
	/// <param name="animName">Animation name.</param>
	public virtual void stopLoopAnim(string animName) {
		this.loopAnimNames.Remove(animName);
	}

	/// <summary>
	/// 是否和人物碰撞
	/// </summary>
	/// <returns><c>true</c>, if hitting char was ised, <c>false</c> otherwise.</returns>
	/// <param name="character">Character.</param>
	public virtual bool isHittingChar(GameCharacterBase character) {
		Intersection.Polygon eventCollider = character.currCollider();
		if (Intersection.polygonPolygon(eventCollider, this.currCollider())) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// 是否和人物重合
	/// </summary>
	/// <returns><c>true</c>, if over char, <c>false</c> otherwise.</returns>
	/// <param name="character">Character.</param>
	public virtual bool isOverChar(GameCharacterBase character) {
		Intersection.Polygon eventCollider = character.currMidCollider();
		if (Intersection.polygonPolygon(eventCollider, this.currMidCollider())) {
			return true;
		}
		return false;
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
