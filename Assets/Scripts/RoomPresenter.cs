using R3;
using System;

public class RoomPresenter : IDisposable
{
    private readonly RoomModel _model;
    private readonly RoomView _view;
    private readonly MonsterFactory _monsterFactory;

    private CompositeDisposable _disposables = new();

    public RoomPresenter(
        RoomModel model,
        RoomView view,
        MonsterFactory monsterFactory)
    {
        _model = model;
        _view = view;
        _monsterFactory = monsterFactory;

        Initialize();
    }

    private void Initialize()
    {
        foreach (var monster in _model.Monsters)
        {
            SpawnMonster(monster);
        }
    }

    private void SpawnMonster(MonsterModel monster)
    {
        var monsterView = _monsterFactory.Create(monster.Type);
        _view.AddMonsterView(monsterView);

        monster.Health.Subscribe(health =>
        {
            if (health <= 0)
            {
                monsterView.Die();
                _model.Monsters.Remove(monster);
            }
        }).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}