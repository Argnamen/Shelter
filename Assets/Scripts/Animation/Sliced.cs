using DG.Tweening;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliced : MonoBehaviour
{
    private CompositeDisposable _disposables = new();
    private void OnEnable()
    {
        Anim();
    }

    private void Anim()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(this.transform.DOLocalMoveY(0.021f, 1f));
        sequence.Append(this.transform.DOLocalMoveY(0f, 1f));
        sequence.Append(this.transform.DOLocalMoveY(-0.021f, 1f));
        sequence.Append(this.transform.DOLocalMoveY(0f, 1f));
        sequence.SetLoops(100, LoopType.Restart);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
