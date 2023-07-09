using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] protected DoorArrow arrowPrefab;
    [SerializeField] protected float arrowOffset = 3;

    [SerializeField] protected Transform tDoor;
    [SerializeField] protected Transform bDoor;
    [SerializeField] protected Transform lDoor;
    [SerializeField] protected Transform rDoor;

    [SerializeField] protected Vector2 roomSize;

    [HideInInspector] public PlayerController player;
    [HideInInspector] public List<SimpleNPC> npcs;

    [HideInInspector] public List<SimpleEnemy> enemys;
    [HideInInspector] public bool roomCleared = false;

    [HideInInspector] public RoomDoor[] openDoors;
    protected RoomDoor entryDoor;

    [HideInInspector] public bool roomActive = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, roomSize);
    }


    public void SetEntryDoor(RoomDoor door) { entryDoor = door; }

    public void SetPlayers(PlayerController player, List<SimpleNPC> npcs) { this.player = player; this.npcs = npcs; }

    public void AddEnemy(SimpleEnemy enemy)
    {
        SimpleEnemy e = Instantiate(enemy, transform);
        e.transform.localPosition = GetRandomPos();
        enemys.Add(e);
    }

    public void AddObject(Transform obj)
    {
        Transform o = Instantiate(obj, transform);
        o.transform.localPosition = GetRandomPos();
    }

    private void Update()
    {
        if (!roomActive || roomCleared)
            return;

        if (!roomCleared && CheckRoomCleared())
            OnRoomCleared();
    }

    public void StartEntities()
    {

    }

    public void StopEntities()
    {

    }

    public virtual bool CheckRoomCleared()
    {
        enemys.RemoveAll(e => { return e == null; });
        return enemys.Count == 0;
    }
    public void OnRoomCleared()
    {
        roomCleared = true;

        foreach (RoomDoor d in openDoors)
        {
            if (d == entryDoor) { continue; }

            DoorArrow arrow = Instantiate(arrowPrefab, transform);
            arrow.direction = d;
            arrow.transform.up = -RoomGenerator.Instance.GetDoorDirection(d);
            arrow.transform.position = (Vector2)GetDoor(d).position + RoomGenerator.Instance.GetDoorDirection(d) * arrowOffset;
        }

        DoorArrow.dirSelected = false;
    }

    public Transform GetDoor(RoomDoor d)
    {
        switch (d)
        {
            case RoomDoor.Left: return lDoor;
            case RoomDoor.Right: return rDoor;
            case RoomDoor.Top: return tDoor;
            case RoomDoor.Bottom: return bDoor;
            default:
                Debug.LogError("Idk how you got here: " + d.ToString());
                return rDoor;
        }
    }

    public Vector2 GetRandomPos(int tries = 3)
    {
        Vector2 pos = new Vector2(Random.Range(-roomSize.x / 2, roomSize.x / 2),
            Random.Range(-roomSize.y / 2, roomSize.y / 2));

        if (tries > 0 && Physics2D.OverlapCircle(pos, 1, 1 << Constants.EnemyLayer & 1 << Constants.EnvironmentLayer))
            return GetRandomPos(--tries);

        return pos;
    }
}
