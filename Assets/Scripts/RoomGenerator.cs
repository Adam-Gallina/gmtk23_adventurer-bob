using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RoomDoor { Top, Bottom, Left, Right }
public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator Instance { get; private set; }

    [SerializeField] protected RoomController roomPrefab;
    [HideInInspector] public RoomController CurrRoom;
    protected Dictionary<RoomDoor, RoomController> nextRooms;
    public Vector2 roomPos = new Vector2(-3.3f, 0);

    [Header("Room Transition")]
    [SerializeField] protected float offset;
    [SerializeField] protected float roomTransitionDuration = 1;
    [SerializeField] protected float npcTimeout = 10;

    [Header("Room Randomization")]
    [SerializeField] [Range(0, 1)] private float lootChance = .5f;
    [SerializeField] private LootWeights[] lootWeights;
    [SerializeField] private EnemyWeights[] enemyWeights;
    [SerializeField] private int startDifficulty;
    [SerializeField] private int difficultyScale;
    [HideInInspector] public int roomCount = 0;
    [SerializeField] private EnvironmentWeights[] environmentWeights;
    [SerializeField] private int environmentWeight;

    private void Awake()
    {
        Instance = this;
    }

    public RoomController GenerateRoom(RoomDoor entryDoor)
    {
        RoomController room = Instantiate(roomPrefab);
        room.gameObject.SetActive(false);

        // Set random doors
        List<RoomDoor> doors = new List<RoomDoor>() { entryDoor };
        List<RoomDoor> remaining = new List<RoomDoor>() { RoomDoor.Left, RoomDoor.Top, RoomDoor.Bottom };
        remaining.Remove(entryDoor);
        doors.Add(remaining[Random.Range(0, remaining.Count)]);
        foreach (RoomDoor door in remaining)
            if (!doors.Contains(door) && Random.Range(0, 100) <= 33)
                doors.Add(door);

        room.openDoors = doors.ToArray();
        room.SetEntryDoor(entryDoor);


        int eWeight = startDifficulty + roomCount * difficultyScale;
        int lWeight = eWeight;
        // Set enemies
        List<EnemyWeights> availableEnemies = new List<EnemyWeights>();
        foreach (EnemyWeights e in enemyWeights)
            if (e.minThreshold <= eWeight)
                availableEnemies.Add(e);
        while (eWeight > 0)
        {
            EnemyWeights enemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
            eWeight -= enemy.weight;
            room.AddEnemy(enemy.enemy);
        }

        // Set loot
        List<LootWeights> availableLoot = new List<LootWeights>();
        foreach (LootWeights l in lootWeights)
            if (l.minThreshold <= eWeight)
                availableLoot.Add(l);
        if (availableLoot.Count > 0)
        {
            while (lWeight > 0)
            {
                if (Random.Range(0, 100) / 100f < lootChance)
                {
                    LootWeights item = availableLoot[Random.Range(0, availableLoot.Count)];
                    lWeight -= item.weight;
                    room.AddObject(item.item.transform);
                }
                else
                    lWeight = 0;
            }
        }

        // Set environment
        int envWeight = environmentWeight;
        while (envWeight > 0)
        {
            List<EnvironmentWeights> availableEnv = new List<EnvironmentWeights>();
            foreach (EnvironmentWeights env in environmentWeights)
                if (env.weight < envWeight)
                    availableEnv.Add(env);
            if (availableEnv.Count == 0)
                break;

            EnvironmentWeights choice = availableEnv[Random.Range(0, availableEnv.Count)];
            envWeight -= choice.weight;
            room.AddObject(choice.item);
        }

        return room;
    }

    public void SetActiveRoom(RoomController room)
    {
        if (CurrRoom)
        {
            room.SetPlayers(CurrRoom.player, CurrRoom.npcs);
            Destroy(CurrRoom.gameObject);
        }

        room.StartEntities();
        room.gameObject.SetActive(true);
        room.roomActive = true;
        CurrRoom = room;
    }

    public void GenerateRooms(RoomDoor prev, RoomDoor[] doors)
    {
        nextRooms = new Dictionary<RoomDoor, RoomController>();

        foreach (RoomDoor r in doors)
        {
            if (r == prev) continue;

            nextRooms.Add(r, GenerateRoom(GetOppositeDoor(r)));
        }
    }

    public void ChangeRoom(RoomDoor dir)
    {
        if (!nextRooms.ContainsKey(dir))
        {
            Debug.LogError("Can't change to room " + dir.ToString());
            Debug.LogError("Available rooms:");
            foreach (RoomDoor r in nextRooms.Keys)
                Debug.Log(r.ToString());
            return;
        }

        foreach (RoomDoor d in nextRooms.Keys)
        {
            if (d != dir)
            {
                Destroy(nextRooms[d].gameObject);
            }
            else
            {
                StartCoroutine(RoomAnim(d, nextRooms[d]));
            }
        }

        roomCount++;
    }

    private IEnumerator RoomAnim(RoomDoor door, RoomController room)
    {
        foreach (SimpleNPC npc in CurrRoom.npcs)
            npc.MoveTowardsDoor(CurrRoom.GetDoor(door));

        float startTime = Time.time;
        while (Time.time < startTime + npcTimeout)
        {
            bool complete = true;
            foreach (SimpleNPC npc in CurrRoom.npcs)
                if (Vector3.Distance(npc.transform.position, CurrRoom.GetDoor(door).position) > 3)
                    complete = false;
            if (complete)
                break;
            yield return new WaitForEndOfFrame();
        }

        Vector2[] npcStarts = new Vector2[CurrRoom.npcs.Count];
        Vector2[] npcTargets = new Vector2[CurrRoom.npcs.Count];
        for (int i = 0; i < CurrRoom.npcs.Count; i++)
        {
            CurrRoom.npcs[i].MoveTowardsDoor(null);
            CurrRoom.npcs[i].SetColliders(false);
            npcStarts[i] = CurrRoom.npcs[i].transform.position;
            npcTargets[i] = roomPos - ((Vector2)CurrRoom.npcs[i].transform.position - roomPos);
        }

        CurrRoom.StopEntities();
        room.gameObject.SetActive(true);

        Vector2 dir = GetDoorDirection(door);
        Vector2 startPos = roomPos + dir * -offset;
        Vector2 exitPos = roomPos + dir * offset;
        startTime = Time.time;

        while (Time.time < startTime + roomTransitionDuration)
        {
            float t = (Time.time - startTime) / roomTransitionDuration;
            room.transform.position = startPos + (roomPos - startPos) * t;
            CurrRoom.transform.position = roomPos + (exitPos - roomPos) * t;

            for (int i = 0; i < CurrRoom.npcs.Count; i++)
            {
                CurrRoom.npcs[i].transform.position = npcStarts[i] + (npcTargets[i] - npcStarts[i]) * t;
            }

            yield return new WaitForEndOfFrame();
        }

        foreach (SimpleNPC npc in CurrRoom.npcs)
            npc.SetColliders(true);

        SetActiveRoom(room);
        GenerateRooms(GetOppositeDoor(door), room.openDoors);
    }

    public RoomDoor GetOppositeDoor(RoomDoor d)
    {
        switch (d)
        {
            case RoomDoor.Left:
                return RoomDoor.Right;
            case RoomDoor.Right:
                return RoomDoor.Left;
            case RoomDoor.Top:
                return RoomDoor.Bottom;
            case RoomDoor.Bottom:
                return RoomDoor.Top;
            default:
                Debug.LogError("Idk how you got here: " + d.ToString());
                return RoomDoor.Right;
        }
    }

    public Vector2 GetDoorDirection(RoomDoor d)
    {
        switch (d)
        {
            case RoomDoor.Left:
                return new Vector2(1, 0);
            case RoomDoor.Right:
                return new Vector2(-1, 0);
            case RoomDoor.Top:
                return new Vector2(0, -1);
            case RoomDoor.Bottom:
                return new Vector2(0, 1);
            default:
                Debug.LogError("Idk how you got here: " + d.ToString());
                return new Vector2(1, 0);
        }
    }
}

[System.Serializable]
public struct EnemyWeights
{
    public SimpleEnemy enemy;
    public int weight;
    public int minThreshold;
}

[System.Serializable]
public struct LootWeights
{
    public ItemBase item;
    public int weight;
    public int minThreshold;
}

[System.Serializable]
public struct EnvironmentWeights
{
    public Transform item;
    public int weight;
    public int minThreshold;
}