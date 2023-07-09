using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private enum TentacleState { Retracted, Animating, Extended }

    public static PlayerController Instance;

    public bool carried { get; private set; }

    [Header("Tentacle")]
    [SerializeField] private Transform tentacleSource;
    [SerializeField] private float tentacleAnimSpeed = 5;
    private TentacleState tentacleState = TentacleState.Retracted;
    private Vector3 tentacleStart;
    private Vector3 tentacleEnd;
    private Coroutine tentacleAnim;

    [Header("Items")]
    [SerializeField] private RangeF maxItemWeight;
    [HideInInspector] public GrabbableBase currGrab;
    private GrabbableBase grabTarget;
    [SerializeField] private GameObject grabSprite;

    private LineRenderer lr;

    private void Awake()
    {
        Instance = this;

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        grabSprite.SetActive(false);
    }


    void Update()
    {
        if (GameController.Instance.Paused) return;

        tentacleStart = tentacleSource.position;
        tentacleEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tentacleEnd.z = 0;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!currGrab)
                SwitchCoroutine(ExtendTentacle());
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (currGrab)
            {
                if (currGrab.GrabEnd())
                {
                    if (currGrab.grabbableType == GrabbableType.Weapon)
                    {
                        Collider2D ally = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1 << Constants.AllyLayer);
                        CombatBase cb;
                        if (ally && (cb = ally.GetComponent<CombatBase>()))
                        {
                            cb.UnequipWeapon();
                            cb.EquipWeapon((WeaponBase)currGrab);
                        }
                    }

                    currGrab = null;
                    grabSprite.SetActive(false);
                    SwitchCoroutine(RetractTentacle());
                }
            }
            else
                SwitchCoroutine(RetractTentacle());
                
        }
        else if ((currGrab || Input.GetKey(KeyCode.Mouse0)) && tentacleState == TentacleState.Extended)
        {
            if (currGrab)
                currGrab.GrabUpdate(tentacleEnd);
            UpdateTentacle(tentacleStart, tentacleEnd);
        }
    }


    public bool TryGrab(GrabbableBase target, bool forced = false)
    {
        if (currGrab)
        {
            if (!forced)
                return false;

            if (!currGrab.GrabEnd())
                return false;
        }

        if (!target.GrabStart())
            return false;

        if (tentacleState != TentacleState.Extended)
            grabTarget = target;
        else
        {
            currGrab = target;
            grabSprite.SetActive(true);
        }

        return true;
    }

    #region Tentacle Anims
    private void UpdateTentacle(Vector3 start, Vector3 end)
    {
        lr.SetPositions(new Vector3[] { start, end });

        if (currGrab)
        {
            grabSprite.transform.position = currGrab.transform.position;
            grabSprite.transform.rotation = currGrab.transform.rotation;
        }
        else
        {
            grabSprite.transform.position = end;
        }
    }

    private void SwitchCoroutine(IEnumerator routine)
    {
        if (tentacleState == TentacleState.Animating)
            StopCoroutine(tentacleAnim);
        tentacleAnim = StartCoroutine(routine);
    }

    private IEnumerator ExtendTentacle()
    {
        if (tentacleState == TentacleState.Extended)
            yield break;
        tentacleState = TentacleState.Animating;

        lr.enabled = true;

        Vector3 currEndPoint = tentacleStart;
        while (Vector3.Distance(currEndPoint, tentacleEnd) > 0.1f)
        {
            currEndPoint = Vector3.MoveTowards(currEndPoint, tentacleEnd, tentacleAnimSpeed * Time.deltaTime);

            UpdateTentacle(tentacleStart, currEndPoint);

            yield return null;
        }
        
        tentacleState = TentacleState.Extended;
        if (grabTarget)
        {
            currGrab = grabTarget;
            grabSprite.SetActive(true);
        }
        grabTarget = null;
    }

    private IEnumerator RetractTentacle()
    {
        if (tentacleState == TentacleState.Retracted)
            yield break;
        tentacleState = TentacleState.Animating;

        Vector3 currEndPoint = tentacleEnd;
        while (Vector3.Distance(currEndPoint, tentacleStart) > 0.1f)
        {
            currEndPoint = Vector3.MoveTowards(currEndPoint, tentacleStart, tentacleAnimSpeed * Time.deltaTime);

            UpdateTentacle(tentacleStart, currEndPoint);

            yield return null;
        }

        lr.enabled = false;
        tentacleState = TentacleState.Retracted;
        grabSprite.SetActive(false);
    }
    #endregion

    public void SetCarryNpc(SimpleNPC npc)
    {
        carried = true;
    }
}
