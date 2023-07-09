using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UiType { Game, Shop }
public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("NPC Health")]
    [SerializeField] private Transform healthPanel;
    [SerializeField] private Transform healthbarPrefab;
    private Dictionary<SimpleNPC, Slider> npcHealth;

    [Header("Pause menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject startButton;

    [Header("Run end")]
    [SerializeField] private GameObject endText;
    [SerializeField] private GameObject endBtn;

    [Header("Shop")]
    [SerializeField] private GameObject shopInv;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMPro.TMP_Text priceText;
    [SerializeField] private TMPro.TMP_Text goldText;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMPro.TMP_Text damageText;
    [SerializeField] private TMPro.TMP_Text healthText;
    [SerializeField] private TMPro.TMP_Text backpackText;

    [Header("Title")]
    [SerializeField] private GameObject title;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetRoomUI(UiType.Shop);

        UpgradeButton(0);
        UpgradeButton(1);
        UpgradeButton(2);

        Invoke(nameof(HideTitle), 30);
    }

    public void HideTitle()
    {
        title.SetActive(false);
    }

    public void SetNPCs(SimpleNPC[] npcs)
    {
        if (npcHealth != null) 
            foreach (Slider s in npcHealth.Values)
                Destroy(s.transform.parent.gameObject);
        npcHealth = new Dictionary<SimpleNPC, Slider>();

        foreach (SimpleNPC npc in npcs)
        {
            Transform healthbar = Instantiate(healthbarPrefab, healthPanel);
            healthbar.GetComponentInChildren<TMPro.TMP_Text>().text = npc.Nickname;
            npcHealth.Add(npc, healthbar.GetComponentInChildren<Slider>());
        }
    }

    public void SetPause(bool paused)
    {
        pauseMenu.SetActive(paused);
    }

    public void SetEnd(bool end)
    {
        endText.SetActive(end);
        endBtn.SetActive(end);
    }

    public void SetPriceText(int amount)
    {
        priceText.text = "Sell for " + amount.ToString() + " gold";
    }
    public void SetGoldText(int amount)
    {
        goldText.text = "You have " + amount.ToString() + " gold";
    }

    public void SetRoomUI(UiType ui)
    {
        switch (ui)
        {
            case UiType.Game:
                startButton.SetActive(false);
                pauseButton.SetActive(true);
                shopInv.SetActive(false);
                shopPanel.SetActive(false);
                upgradePanel.SetActive(false);
                HideTitle();
                break;
            case UiType.Shop:
                startButton.SetActive(true);
                pauseButton.SetActive(false);
                shopInv.SetActive(true);
                shopPanel.SetActive(true);
                upgradePanel.SetActive(true);
                break;
            default:
                Debug.LogError("Don't know how to enable " + ui.ToString());
                SetRoomUI(UiType.Shop);
                break;
        }
    }

    private void Update()
    {
        if (npcHealth != null)
        {
            foreach (SimpleNPC npc in npcHealth.Keys)
            {
                if (npc)
                    npcHealth[npc].value = npc.currHealth / npc.maxHealth;
            }
        }
    }

    public void TogglePauseButton()
    {
        GameController.Instance.TogglePause();
        SetPause(GameController.Instance.Paused);
    }

    public void StartRunButton()
    {
        GameController.Instance.StartRun();
    }

    public void EndRunButton()
    {
        GameController.Instance.EndRun();
        GameController.Instance.SetPause(false);
        SetPause(false);
    }

    public void SellButton()
    {
        ShopController.Instance.SellAll();
    }

    public void UpgradeButton(int upgrade)
    {
        switch (upgrade)
        {
            case 0:
                if (Upgrades.Instance.damageMod.CanUpgrade())
                    Upgrades.Instance.damageMod.DoUpgrade();
                damageText.text = $"Damage (lvl {Upgrades.Instance.damageMod.level})\n" + (Upgrades.Instance.damageMod.Max ? "Max level" : $"{Upgrades.Instance.damageMod.Price} gold");
                damageText.GetComponentInParent<Button>().interactable = !Upgrades.Instance.damageMod.Max;
                break;
            case 1:
                if (Upgrades.Instance.healthTotal.CanUpgrade())
                    Upgrades.Instance.healthTotal.DoUpgrade();
                healthText.text = $"Health (lvl {Upgrades.Instance.healthTotal.level})\n" + (Upgrades.Instance.healthTotal.Max ? "Max level" : $"{Upgrades.Instance.healthTotal.Price} gold");
                healthText.GetComponentInParent<Button>().interactable = !Upgrades.Instance.healthTotal.Max;
                break;
            case 2:
                if (Upgrades.Instance.backpackSize.CanUpgrade())
                    Upgrades.Instance.backpackSize.DoUpgrade();
                backpackText.text = $"Backpack (lvl {Upgrades.Instance.backpackSize.level})\n" + (Upgrades.Instance.backpackSize.Max ? "Max level" : $"{Upgrades.Instance.backpackSize.Price} gold");
                backpackText.GetComponentInParent<Button>().interactable = !Upgrades.Instance.backpackSize.Max;
                break;
            default:
                Debug.LogError("Invalid upgrade num: " + upgrade);
                break;

        }
        SetGoldText(ShopController.Instance.totalGold);
    }
}
