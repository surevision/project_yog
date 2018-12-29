using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {
	public static int PPU = 100;
	public static float GRID_WIDTH = 32.0f;
    public static float WIDTH = 800.0f;
    public static float HEIGHT = 600.0f;

    private static Util instance = new Util();
    private Util() { }

    public float _getWidthScale() {
        return Screen.width / WIDTH;
    }
}
