using UnityEngine;
using R3;

public class DisposableCollector : MonoBehaviour
{
    private CompositeDisposable _disposables = new();

    public CompositeDisposable Disposables => _disposables;

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
