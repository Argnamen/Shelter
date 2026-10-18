using DG.Tweening;
using UnityEngine;

public class RoomView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform[] _monstersContainer;
    [SerializeField] private Transform[] _heroesContainers;

    [SerializeField] private Transform[] _movingPoints;
    public Vector2Int Position { get; private set; }
    public RoomType Type { get; private set; }

    private Sequence sequence;

    private bool _isTimeStop = false;

    public void Initialize(Vector2Int position, RoomType type, Faction faction)
    {
        Position = position;
        Type = type;
        name = $"{faction}_{type}Room_{position.x}_{position.y}";

        UpdateVisuals(faction);
    }

    public void TimeStop(bool isTimeStop)
    {
        if (isTimeStop)
            sequence.Pause();
        else
            sequence.Play();
    }

    private void UpdateVisuals(Faction faction)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = GetRoomColor(Type, faction);
        }
    }

    private Color GetRoomColor(RoomType type, Faction faction)
    {
        Color baseColor = type switch
        {
            RoomType.Combat => new Color(1, 0.5f, 0.5f, 1),
            RoomType.Rest => new Color(0.5f, 0.5f, 1, 1),
            RoomType.Treasure => new Color(1, 1, 0.5f, 1),
            RoomType.Stairs => new Color(0.5f, 1, 0.5f, 1),
            _ => Color.white
        };

        // Добавляем оттенок фракции
        return faction switch
        {
            Faction.Player => baseColor,
            Faction.Enemy1 => Color.Lerp(baseColor, Color.red, 0.3f),
            Faction.Enemy2 => Color.Lerp(baseColor, Color.blue, 0.3f),
            Faction.Enemy3 => Color.Lerp(baseColor, Color.green, 0.3f),
            _ => baseColor
        };
    }

    public void AddMonsterView(MonsterView monsterView)
    {
        if(monsterView == null)
            return;

        foreach (var container in _monstersContainer)
        {
            if (container.childCount == 0)
            {
                monsterView.transform.SetParent(container);
                monsterView.transform.localPosition = Vector3.zero;
                break;
            }
        }
    }

    int cycle = 0;
    public void AddHeroView(HeroView heroView)
    {
        if (heroView == null)
            return;

        bool isEnter = false;

        for (int i = 0; i < _heroesContainers.Length; i++)
        {
            if (_heroesContainers[i].childCount == cycle)
            {
                heroView.transform.SetParent(_heroesContainers[i]);
                isEnter = true;
            }
        }

        if (!isEnter)
        {
            cycle++;
            AddHeroView(heroView);
        }
        else
        {
            cycle = 0;
        }
    }

    public void RandomMovingInRoom(MonsterView monsterView)
    {
        if (monsterView == null)
            return;

        var point = _movingPoints[Random.Range(0, _movingPoints.Length)];

        float time = 1f;
        var rotate = monsterView.transform.localRotation;

        if (monsterView.transform.position.x > point.position.x)
        {
            rotate.eulerAngles = Vector3.zero;
        }
        else
        {
            rotate.eulerAngles = Vector3.up * 180;
        }

        monsterView.transform.localRotation = rotate;

        sequence = DOTween.Sequence();

        sequence.Join(monsterView.transform.DOMove(point.transform.position, time).SetEase(Ease.Flash));

        if (point.position.z > 0)
        {
            sequence.Join(monsterView.transform.DOScale(1.5f, time).SetEase(Ease.Flash));
        }
        else if (point.position.z < 0)
        {
            sequence.Join(monsterView.transform.DOScale(0.5f, time).SetEase(Ease.Flash));
        }
        else
        {
            sequence.Join(monsterView.transform.DOScale(1, time).SetEase(Ease.Flash));
        }
    }

    public void ResetMoving(MonsterView monsterView)
    {
        if (monsterView == null)
            return;

        var rotate = monsterView.transform.localRotation;

        sequence.Pause();
        sequence.Kill();
        sequence = DOTween.Sequence();

        rotate.eulerAngles = Vector3.zero;

        monsterView.transform.localRotation = rotate;

        monsterView.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.Flash);
        monsterView.transform.DOScale(1, 0.2f).SetEase(Ease.Flash);
    }

    public void DestroyRoom()
    {
        Destroy(gameObject);
    }
}