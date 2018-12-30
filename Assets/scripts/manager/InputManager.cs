using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager {
    public enum GameKey {
        NONE = -1,

        DOWN = 2,
        LEFT = 4,
        RIGHT = 6,
        UP = 8,

        B,
        C,

        L,
        R,

        SHIFT,
        CTRL,

        F1,F2,F3,F4,F5,F6,F7,F8,F9,

    }

    private static Dictionary<KeyCode, GameKey> keyMap;

    private static Dictionary<GameKey, int> keyStatus;

    public static void init() {
        Debug.Log("init input manager");
        keyStatus = new Dictionary<GameKey, int>();
        keyMap = new Dictionary<KeyCode,GameKey>();
        keyMap.Add(KeyCode.DownArrow, GameKey.DOWN);
        keyMap.Add(KeyCode.LeftArrow, GameKey.LEFT);
        keyMap.Add(KeyCode.RightArrow, GameKey.RIGHT);
        keyMap.Add(KeyCode.UpArrow, GameKey.UP);
        
        keyMap.Add(KeyCode.Escape, GameKey.B);
        keyMap.Add(KeyCode.X, GameKey.B);
        keyMap.Add(KeyCode.KeypadEnter, GameKey.C);
        keyMap.Add(KeyCode.Space, GameKey.C);
        keyMap.Add(KeyCode.Z, GameKey.C);

        keyMap.Add(KeyCode.Q, GameKey.L);
        keyMap.Add(KeyCode.W, GameKey.R);

        keyMap.Add(KeyCode.LeftShift, GameKey.SHIFT);
        keyMap.Add(KeyCode.LeftControl, GameKey.CTRL);
        
        keyMap.Add(KeyCode.F1, GameKey.F1);
        keyMap.Add(KeyCode.F2, GameKey.F2);
        keyMap.Add(KeyCode.F3, GameKey.F3);
        keyMap.Add(KeyCode.F4, GameKey.F4);
        keyMap.Add(KeyCode.F5, GameKey.F5);
        keyMap.Add(KeyCode.F6, GameKey.F6);
        keyMap.Add(KeyCode.F7, GameKey.F7);
        keyMap.Add(KeyCode.F8, GameKey.F8);
        keyMap.Add(KeyCode.F9, GameKey.F9);

    }

	public static void clear() {
		keyStatus = new Dictionary<GameKey,int>();
	}

    public static void update() {
        //Debug.Log("update input manager");
        foreach (KeyCode k in keyMap.Keys) {
            if (Input.GetKeyDown(k)) {
                if (!keyStatus.ContainsKey(keyMap[k])) {
                    keyStatus.Add(keyMap[k], 0);
                }
            }
        }
        foreach (KeyCode k in keyMap.Keys) {
            if (Input.GetKeyUp(k)) {
                if (keyStatus.ContainsKey(keyMap[k])) {
                    keyStatus.Remove(keyMap[k]);
                }
            }
        }
    }

    public static void lateUpdate() {
        // 更新按下的key
        List<GameKey> currKeys = new List<GameKey>();
        foreach (GameKey code in keyStatus.Keys) {
            currKeys.Add(code);
        }
        foreach (GameKey code in currKeys) {
            keyStatus[code] += 1;
        }
    }

    /// <summary>
    /// 强制标记放开
    /// </summary>
    /// <param name="key"></param>
	public static void releaseKey(GameKey code) {
        if (keyStatus.ContainsKey(code)) {
            keyStatus.Remove(code);
        }
	}

	public static bool isKeyUp(GameKey code) {
		return keyStatus.ContainsKey(code) && keyStatus[code] == -1;
	}

	public static bool isTrigger(GameKey code) {
		return keyStatus.ContainsKey(code) && keyStatus[code] == 1;
	}
    
	public static bool isPress(GameKey code) {
		return keyStatus.ContainsKey(code) && keyStatus[code] >= 1;
	}

    public static bool isRepeat(GameKey code) {
		return keyStatus.ContainsKey(code) && keyStatus[code] > 1;
	}

    /// <summary>
    /// 取最后按下的方向键
    /// </summary>
    public static GameKey DIR4() {
        List<GameKey> keys = new List<GameKey>(new GameKey[]{ GameKey.DOWN, GameKey.LEFT, GameKey.RIGHT, GameKey.UP });
        bool keyDownFlag = false;
        foreach (GameKey code in keys) {
            if (isPress(code)) {
                keyDownFlag = true;
                break;
            }
        }
        if (!keyDownFlag) {
            return GameKey.NONE;
        }
        keys.Sort((code1, code2) => {
            int frames1 = keyStatus.ContainsKey(code1) ? keyStatus[code1] : int.MaxValue;
            int frames2 = keyStatus.ContainsKey(code2) ? keyStatus[code2] : int.MaxValue;
            return frames1.CompareTo(frames2);
        });
        return keys[0];
    }
}
