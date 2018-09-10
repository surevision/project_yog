using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen {

    private const float NormalView = 0.8f;
    private const float HalfView = NormalView / 2;

    private float currView = NormalView;

    public float CurrView {
        get { return currView; }
        set { currView = value; }
    }
    private float targetView = NormalView;
    private bool isMiniView = false;

    public void toggleView() {
        if (isMiniView) {
            this.showNormalView();
        } else {
            this.showHalfView();
        }
    }

    public void showNormalView() {
        isMiniView = false;
        targetView = NormalView;
    }

    public void showHalfView() {
        isMiniView = true;
        targetView = HalfView;
    }

    public void update() {

        if (CurrView < targetView) {
            CurrView = Mathf.Min(CurrView + 0.16f / 8, targetView);
        }
        if (CurrView > targetView) {
            CurrView = Mathf.Max(CurrView - 0.16f / 8, targetView);
        }

    }
}
