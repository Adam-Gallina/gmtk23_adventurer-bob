using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public bool Paused { get; private set; } = false;

    [SerializeField] private PlayerController backpackPrefab;
    [SerializeField] private SimpleNPC[] partyPrefabs;
    [HideInInspector] public WeaponBase currNpcWeapon;

    [SerializeField] private RoomController shopRoomPrefab;

    [SerializeField] private Vector2 partySpawnPoint;
    [SerializeField] private float partySpawnRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(partySpawnPoint, partySpawnRadius);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupShop();
    }

    public void SetupShop()
    {
        PlayerController player = Instantiate(backpackPrefab);

        List<SimpleNPC> npcs = new List<SimpleNPC>();
        foreach (SimpleNPC npc in partyPrefabs)
        {
            npcs.Add(Instantiate(npc, partySpawnPoint + Random.insideUnitCircle * partySpawnRadius, Quaternion.identity));
        }
        npcs[0].SetBackpack(player);
        if (currNpcWeapon)
            npcs[0].EquipWeapon(currNpcWeapon);

        RoomController shopRoom = Instantiate(shopRoomPrefab);
        shopRoom.transform.position = RoomGenerator.Instance.roomPos;
        shopRoom.SetEntryDoor(RoomDoor.Top);
        shopRoom.openDoors = new RoomDoor[] { RoomDoor.Top, RoomDoor.Left };
        shopRoom.SetPlayers(player, npcs);

        GameUI.Instance.SetNPCs(npcs.ToArray());

        RoomGenerator.Instance.roomCount = 0;
        RoomGenerator.Instance.SetActiveRoom(shopRoom);
        RoomGenerator.Instance.GenerateRooms(RoomDoor.Top, shopRoom.openDoors);
    }

    public void StartRun()
    {
        RoomGenerator.Instance.ChangeRoom(RoomDoor.Left);

        RoomGenerator.Instance.CurrRoom.npcs.ForEach(npc => { npc.OnStartRun(); });

        GameUI.Instance.SetRoomUI(UiType.Game);
    }

    public void EndRun()
    {
        if (RoomGenerator.Instance.CurrRoom.npcs[0]) 
        {
            if (currNpcWeapon = RoomGenerator.Instance.CurrRoom.npcs[0].currWeapon)
                currNpcWeapon.UnequipWeapon();
        }
        if (RoomGenerator.Instance.CurrRoom.player != null)
            Destroy(RoomGenerator.Instance.CurrRoom.player.gameObject);
        RoomGenerator.Instance.CurrRoom.npcs.ForEach(n => { if (n) { Destroy(n.gameObject); } });
        Destroy(RoomGenerator.Instance.CurrRoom.gameObject);
        RoomGenerator.Instance.CurrRoom = null;
        SetupShop();

        GameUI.Instance.SetEnd(false);

        GameUI.Instance.SetRoomUI(UiType.Shop);
    }

    #region Pausing
    public void TogglePause() { SetPause(!Paused); }
    public void SetPause(bool paused) 
    { 
        Paused = paused;
        Time.timeScale = paused ? 0 : 1;
    }
    #endregion
}
