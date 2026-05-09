using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class NewPlayerLanding : MonoBehaviour
{
    private const string FlashBonusChildName = "FlashBonus";
    private const string FlashBonusExcludedBuildingName = "Building0";

    [Header("Filter")]
    [SerializeField] private LayerMask buildingLayers;

    [Header("Landing")]
    [SerializeField] private float minUpNormal = 0.60f;

    [Header("Booster")]
    [SerializeField] private BoostersManager boostersManager;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private bool grantPlayerBoosterOnBuildingBooster = true;

    [Header("Score")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Phase")]
    [SerializeField] private NewPhaseManager newPhaseManager;

    [Header("Combo")]
    [SerializeField] private ComboUIController comboUI;
    [SerializeField] private int comboPointMultiplier = 100;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private int _buildingContactCount = 0;

   
    private NewBuildingTimeController _currentNewBuilding;

    private int _comboCount = 0;
    private GameObject _lastComboBuilding;

    private string _lastBoosterBuildingName = null;
    private bool _hasLandedOutsideGoalSinceReset = false;

    [Header("SFX")]
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip boosterPickupClip;
    [SerializeField] private AudioClip comboClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float landVolume = 3f;
    [SerializeField] private float boosterVolume = 3f;
    [SerializeField] private float comboVolume = 1f;

    [Header("Camera Shake")]
    [SerializeField] private CameraLandingShake cameraLandingShake;

    private void Awake()
    {
        if (playerJump == null)
            playerJump = GetComponent<PlayerJump>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (cameraLandingShake == null)
            cameraLandingShake = FindAnyObjectByType<CameraLandingShake>();

        GameAudio.ConfigureSfxSource(audioSource);

        _hasLandedOutsideGoalSinceReset = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsBuildingLayer(collision.gameObject.layer))
            return;

        _buildingContactCount++;
        ScoreManager.SetCanAddScore(false);

        
        NewBuildingTimeController newBuilding = collision.collider.GetComponentInParent<NewBuildingTimeController>();
        NewBuildingGoalController newGoal = collision.collider.GetComponentInParent<NewBuildingGoalController>();

        bool isGoalBuilding = newGoal != null;

        float bestNy = GetBestUpNormal(collision);
        bool landedFromAbove = bestNy >= minUpNormal;

        if (landedFromAbove)
        {
            bool isDifferentBuilding =
                  newBuilding != _currentNewBuilding;

            bool hasAnyBuilding =  newBuilding != null;

            if (hasAnyBuilding && isDifferentBuilding)
            {
                ExitCurrentBuilding();

     
                _currentNewBuilding = newBuilding;

                EnterCurrentBuilding();

                if (debug)
                {
                    string buildingName =
                        _currentNewBuilding != null ? _currentNewBuilding.name :
                        "null";

                    Debug.Log($"[NewPlayerLanding] OnPlayerEnter -> {buildingName}");
                }
            }
        }

        if (landedFromAbove && _buildingContactCount == 1)
        {
            PlayLandSFX();

            if (cameraLandingShake != null)
                cameraLandingShake.PlayShake();

            BuildingBooster bb = GetLandingBuildingBooster(collision, newBuilding);
            PlayFlashBonus(collision, newBuilding, bb);
        }

        // Arranque inicial del TAX o reanudación tras respawn:
        // primer aterrizaje válido fuera del start/goal mientras el goal esté apagado.
        if (landedFromAbove && newPhaseManager != null)
        {
            if (!isGoalBuilding && !newPhaseManager.GoalEnabled)
            {
                newPhaseManager.OnExitBuildingLeft();

                if (debug)
                    Debug.Log("[NewPlayerLanding] TAX started/resumed after valid landing outside GOAL.");
            }
        }

        if (landedFromAbove)
        {
            if (!isGoalBuilding)
            {
                _hasLandedOutsideGoalSinceReset = true;
            }
            else if (newPhaseManager != null &&
                     newPhaseManager.GoalEnabled &&
                     !newPhaseManager.IsCompleted &&
                     !newPhaseManager.IsTimeUp &&
                     _hasLandedOutsideGoalSinceReset)
            {
                Vector3 goalTopCenter =
                    newGoal != null ? newGoal.GetGoalTopCenter() :
                    collision.collider.bounds.center;

                if (debug)
                    Debug.Log("[NewPlayerLanding] Valid GOAL landing detected.");

                newPhaseManager.OnPlayerTouchedGoal(goalTopCenter);
                return;
            }
        }

        if (landedFromAbove)
            HandleBoosterAndCombo(collision);
    }

    private void PlayLandSFX()
    {
        if (landClip != null && audioSource != null)
            GameAudio.PlaySfx(audioSource, landClip, landVolume);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsBuildingLayer(collision.gameObject.layer))
            return;

        _buildingContactCount--;

        if (_buildingContactCount <= 0)
        {
            _buildingContactCount = 0;

            if (newPhaseManager != null && newPhaseManager.TimerRunning)
                ScoreManager.SetCanAddScore(true);
            else
                ScoreManager.SetCanAddScore(false);

            if (_currentNewBuilding != null)
            {
                if (debug)
                {
                    string buildingName =_currentNewBuilding != null ? _currentNewBuilding.name :
                        "null";

                    Debug.Log($"[NewPlayerLanding] OnPlayerExit -> {buildingName}");
                }

                ExitCurrentBuilding();
                _currentNewBuilding = null;
            }
        }
    }

    private void EnterCurrentBuilding()
    {
      

        if (_currentNewBuilding != null)
            _currentNewBuilding.OnPlayerEnter();
    }

    private void ExitCurrentBuilding()
    {
             if (_currentNewBuilding != null)
            _currentNewBuilding.OnPlayerExit();
    }

    private void HandleBoosterAndCombo(Collision collision)
    {
        string buildingName = collision.transform.name;

        BuildingBooster bb = collision.collider.GetComponentInParent<BuildingBooster>();
        if (bb == null)
        {
            GameObject goByName = GameObject.Find(buildingName);
            if (goByName != null)
                bb = goByName.GetComponent<BuildingBooster>();
        }

        bool hasCircles =
            bb != null &&
            boostersManager != null &&
            boostersManager.IsBoosterActive(bb);

        bool boosterStateActive = playerJump != null && playerJump.IsBoosterStateActive;

        if (hasCircles && grantPlayerBoosterOnBuildingBooster)
        {
            if (_lastBoosterBuildingName != buildingName)
            {
                _lastBoosterBuildingName = buildingName;

                if (playerJump != null && playerJump.TryGrantBooster())
                {
                    boosterStateActive = true;
                    PlayBoosterPickupSFX();

                    if (scoreManager != null)
                        scoreManager.OnBoosterUsed();

                    Debug.Log($"[NewPlayerLanding] Booster granted on {buildingName}");
                }
            }
        }
        else
        {
            _lastBoosterBuildingName = null;
        }

        GameObject comboBuilding = bb != null ? bb.gameObject : collision.transform.root.gameObject;

        if (hasCircles)
        {
            if (_comboCount == 0)
            {
                if (boosterStateActive)
                {
                    _comboCount = 1;
                    _lastComboBuilding = comboBuilding;

                    Debug.Log("reboost");
                    Debug.Log($"[NewPlayerLanding] COMBO START => X{_comboCount} on {comboBuilding.name}");
                }

                return;
            }

            if (comboBuilding != _lastComboBuilding)
            {
                _comboCount++;
                _lastComboBuilding = comboBuilding;

                Debug.Log("reboost");
                Debug.Log($"[NewPlayerLanding] COMBO++ => X{_comboCount} on {comboBuilding.name}");
            }

            return;
        }

        CloseComboIfNeeded();
    }

    private void CloseComboIfNeeded()
    {
        if (_comboCount >= 2)
        {
            if (comboUI != null)
                comboUI.ShowCombo(_comboCount);

            PlayComboSFX();

            if (scoreManager != null)
            {
                int comboPoints = _comboCount * comboPointMultiplier;
                scoreManager.AddScore(comboPoints);

                Debug.Log($"[NewPlayerLanding] COMBO CLOSED X{_comboCount} => +{comboPoints} points");
            }
        }

        ResetCombo();
    }

    private void PlayBoosterPickupSFX()
    {
        if (boosterPickupClip != null && audioSource != null)
            GameAudio.PlaySfx(audioSource, boosterPickupClip, boosterVolume);
    }

    private void PlayComboSFX()
    {
        if (comboClip != null && audioSource != null)
            GameAudio.PlaySfx(audioSource, comboClip, comboVolume);
    }

    private void PlayFlashBonus(Collision collision, NewBuildingTimeController newBuilding, BuildingBooster bb)
    {
        Transform building = GetLandingBuildingTransform(collision, newBuilding, bb);
        if (building == null || building.name == FlashBonusExcludedBuildingName)
            return;

        if (boostersManager == null || bb == null || !boostersManager.IsBoosterActive(bb))
            return;

        Transform flashRoot = FindChildByName(building, FlashBonusChildName);
        if (flashRoot == null)
            return;

        if (!flashRoot.gameObject.activeSelf)
            flashRoot.gameObject.SetActive(true);

        ParticleSystem[] particles = flashRoot.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null)
                continue;

            particles[i].gameObject.SetActive(true);
            particles[i].Clear(true);
            particles[i].Play(true);
        }
    }

    private BuildingBooster GetLandingBuildingBooster(Collision collision, NewBuildingTimeController newBuilding)
    {
        if (newBuilding != null)
        {
            BuildingBooster bb = newBuilding.GetComponent<BuildingBooster>();
            if (bb != null)
                return bb;

            bb = newBuilding.GetComponentInChildren<BuildingBooster>(true);
            if (bb != null)
                return bb;
        }

        BuildingBooster parentBb = collision.collider.GetComponentInParent<BuildingBooster>();
        if (parentBb != null)
            return parentBb;

        GameObject goByName = GameObject.Find(collision.transform.name);
        if (goByName != null)
        {
            BuildingBooster bb = goByName.GetComponent<BuildingBooster>();
            if (bb != null)
                return bb;

            return goByName.GetComponentInChildren<BuildingBooster>(true);
        }

        return null;
    }

    private Transform GetLandingBuildingTransform(Collision collision, NewBuildingTimeController newBuilding, BuildingBooster bb)
    {
        if (newBuilding != null)
            return newBuilding.transform;

        if (bb != null)
        {
            if (FindChildByName(bb.transform, FlashBonusChildName) != null)
                return bb.transform;

            if (bb.transform.parent != null && FindChildByName(bb.transform.parent, FlashBonusChildName) != null)
                return bb.transform.parent;

            return bb.transform;
        }

        return collision.collider.transform;
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
                return children[i];
        }

        return null;
    }

    private void ResetCombo()
    {
        _comboCount = 0;
        _lastComboBuilding = null;
    }

    public void ResetComboState()
    {
        ResetCombo();
        if (comboUI != null)
            comboUI.ForceHide();
    }

    public void ResetGoalProgress()
    {
        _hasLandedOutsideGoalSinceReset = false;
    }

    private float GetBestUpNormal(Collision collision)
    {
        float bestNy = -1f;
        foreach (var c in collision.contacts)
            if (c.normal.y > bestNy) bestNy = c.normal.y;
        return bestNy;
    }

    private bool IsBuildingLayer(int layer)
    {
        return (buildingLayers.value & (1 << layer)) != 0;
    }

    public void CloseComboOnGoal()
    {
        CloseComboIfNeeded();
    }
}
