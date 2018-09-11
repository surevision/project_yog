using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen {

    private const float NormalView = 0.8f;
    private const float HalfView = NormalView / 2;

    private float targetView = NormalView;
    private bool isMiniView = false;

    private float _currView = NormalView;

    public float currView {
        get { return _currView; }
        set { _currView = value; }
    }

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

        if (currView < targetView) {
            currView = Mathf.Min(currView + 0.16f / 8, targetView);
        }
        if (currView > targetView) {
            currView = Mathf.Max(currView - 0.16f / 8, targetView);
        }

    }
}
