using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerLanding : MonoBehaviour
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
    [SerializeField] private PhaseManager phaseManager;

    [Header("Combo")]
    [SerializeField] private ComboUIController comboUI;
    [SerializeField] private int comboPointMultiplier = 100;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private int _buildingContactCount = 0;

   
    private BuildingTimeController _currentBuilding;

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

        
        BuildingTimeController building = collision.collider.GetComponentInParent<BuildingTimeController>();
        BuildingGoalController goal = collision.collider.GetComponentInParent<BuildingGoalController>();

        bool isGoalBuilding = goal != null;

        float bestNy = GetBestUpNormal(collision);
        bool landedFromAbove = bestNy >= minUpNormal;

        if (landedFromAbove)
        {
            bool isDifferentBuilding =
                  building != _currentBuilding;

            bool hasAnyBuilding =  building != null;

            if (hasAnyBuilding && isDifferentBuilding)
            {
                ExitCurrentBuilding();

     
                _currentBuilding = building;

                EnterCurrentBuilding();

                if (debug)
                {
                    string buildingName =
                        _currentBuilding != null ? _currentBuilding.name :
                        "null";

                    Debug.Log($"[PlayerLanding] OnPlayerEnter -> {buildingName}");
                }
            }
        }

        if (landedFromAbove && _buildingContactCount == 1)
        {
            PlayLandSFX();

            if (cameraLandingShake != null)
                cameraLandingShake.PlayShake();

            BuildingBooster bb = GetLandingBuildingBooster(collision, building);
            PlayFlashBonus(collision, building, bb);
        }

        // Arranque inicial del TAX o reanudación tras respawn:
        // primer aterrizaje válido fuera del start/goal mientras el goal esté apagado.
        if (landedFromAbove && phaseManager != null)
        {
            if (!isGoalBuilding && !phaseManager.GoalEnabled)
            {
                phaseManager.OnExitBuildingLeft();

                if (debug)
                    Debug.Log("[PlayerLanding] TAX started/resumed after valid landing outside GOAL.");
            }
        }

        if (landedFromAbove)
        {
            if (!isGoalBuilding)
            {
                _hasLandedOutsideGoalSinceReset = true;
            }
            else if (phaseManager != null &&
                     phaseManager.GoalEnabled &&
                     !phaseManager.IsCompleted &&
                     !phaseManager.IsTimeUp &&
                     _hasLandedOutsideGoalSinceReset)
            {
                Vector3 goalTopCenter =
                    goal != null ? goal.GetGoalTopCenter() :
                    collision.collider.bounds.center;

                if (debug)
                    Debug.Log("[PlayerLanding] Valid GOAL landing detected.");

                phaseManager.OnPlayerTouchedGoal(goalTopCenter);
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

            if (phaseManager != null && phaseManager.TimerRunning)
                ScoreManager.SetCanAddScore(true);
            else
                ScoreManager.SetCanAddScore(false);

            if (_currentBuilding != null)
            {
                if (debug)
                {
                    string buildingName =_currentBuilding != null ? _currentBuilding.name :
                        "null";

                    Debug.Log($"[PlayerLanding] OnPlayerExit -> {buildingName}");
                }

                ExitCurrentBuilding();
                _currentBuilding = null;
            }
        }
    }

    private void EnterCurrentBuilding()
    {
      

        if (_currentBuilding != null)
            _currentBuilding.OnPlayerEnter();
    }

    private void ExitCurrentBuilding()
    {
             if (_currentBuilding != null)
            _currentBuilding.OnPlayerExit();
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

                    Debug.Log($"[PlayerLanding] Booster granted on {buildingName}");
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
                    Debug.Log($"[PlayerLanding] COMBO START => X{_comboCount} on {comboBuilding.name}");
                }

                return;
            }

            if (comboBuilding != _lastComboBuilding)
            {
                _comboCount++;
                _lastComboBuilding = comboBuilding;

                Debug.Log("reboost");
                Debug.Log($"[PlayerLanding] COMBO++ => X{_comboCount} on {comboBuilding.name}");
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

                Debug.Log($"[PlayerLanding] COMBO CLOSED X{_comboCount} => +{comboPoints} points");
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

    private void PlayFlashBonus(Collision collision, BuildingTimeController buildingController, BuildingBooster bb)
    {
        Transform buildingTransform = GetLandingBuildingTransform(collision, buildingController, bb);
        if (buildingTransform == null || buildingTransform.name == FlashBonusExcludedBuildingName)
            return;

        if (boostersManager == null || bb == null || !boostersManager.IsBoosterActive(bb))
            return;

        Transform flashRoot = FindChildByName(buildingTransform, FlashBonusChildName);
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

    private BuildingBooster GetLandingBuildingBooster(Collision collision, BuildingTimeController building)
    {
        if (building != null)
        {
            BuildingBooster bb = building.GetComponent<BuildingBooster>();
            if (bb != null)
                return bb;

            bb = building.GetComponentInChildren<BuildingBooster>(true);
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

    private Transform GetLandingBuildingTransform(Collision collision, BuildingTimeController building, BuildingBooster bb)
    {
        if (building != null)
            return building.transform;

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
