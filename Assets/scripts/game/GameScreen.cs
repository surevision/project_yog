using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen {

    // 缩放相关
    private const float NormalView = 0.8f;
    private const float HalfView = NormalView / 2;

    private float targetView = NormalView;
    private bool isMiniView = false;

    private float _currView = NormalView;

    public float currView {
        get { return _currView; }
        set { _currView = value; }
    }

    // 渐变相关
    private int _transitionProgress = -1;
    public float transitionProgress {
        get { return Mathf.Max(0, _transitionProgress) / 255.0f; }
    }
    public delegate void onTransitionFinish();
    public onTransitionFinish transitionFinishCallback;
    public void setupTransition(onTransitionFinish callback) {
        this._transitionProgress = 255;
        this.transitionFinishCallback = callback;
    }
    public bool isInTransition() {
        return this._transitionProgress != -1;
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

        // 刷新视野
        if (currView < targetView) {
            currView = Mathf.Min(currView + 0.16f / 8, targetView);
        }
        if (currView > targetView) {
            currView = Mathf.Max(currView - 0.16f / 8, targetView);
        }
        // 刷新渐变
        if (this._transitionProgress > 0) {
            this._transitionProgress = Mathf.Max(this._transitionProgress, 0);
            if (this._transitionProgress == 0) {
                this._transitionProgress = -1;
                if (this.transitionFinishCallback != null) {
                    transitionFinishCallback.Invoke();
                }
            }
        }
    }
}
