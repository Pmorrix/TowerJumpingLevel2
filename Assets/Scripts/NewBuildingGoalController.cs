using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NewBuildingGoalController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NewPhaseManager newPhaseManager;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private BuildingPulse buildingPulse;

    [Header("Target")]
    [SerializeField] private int materialIndex = 1;
    [SerializeField] private string colorProperty = "_BaseColor";

    [Header("Goal Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private GameObject goalCoin;

    private bool _goalEnabled;
    private MaterialPropertyBlock _mpb;

    private readonly HashSet<Collider> _playerCollidersOnGoal = new HashSet<Collider>();

    public bool IsPlayerOnGoal => _playerCollidersOnGoal.Count > 0;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (buildingPulse == null)
            buildingPulse = GetComponent<BuildingPulse>();

        _mpb = new MaterialPropertyBlock();
        ForceResetVisual();
    }

    private void OnDisable()
    {
        _playerCollidersOnGoal.Clear();
    }

    public void SetGoalEnabled(bool enabled)
    {
        _goalEnabled = enabled;

        if (!_goalEnabled)
        {
            ForceResetVisual();
            return;
        }

        ApplyColor(normalColor);

        if (buildingPulse != null)
            buildingPulse.enabled = true;

        if (goalCoin != null)
            goalCoin.SetActive(true);
    }

    public Vector3 GetGoalTopCenter()
    {
        if (targetRenderer == null)
            return transform.position;

        Bounds b = targetRenderer.bounds;

        return new Vector3(
            b.center.x,
            b.max.y,
            b.center.z
        );
    }

    private void ForceResetVisual()
    {
        if (buildingPulse != null)
            buildingPulse.enabled = false;

        if (goalCoin != null)
            goalCoin.SetActive(false);

        ApplyColor(normalColor);
    }

    private void ApplyColor(Color c)
    {
        if (targetRenderer == null)
            return;

        Material[] mats = targetRenderer.sharedMaterials;
        if (mats == null || materialIndex < 0 || materialIndex >= mats.Length)
            return;

        Material mat = mats[materialIndex];
        if (mat == null || !mat.HasProperty(colorProperty))
            return;

        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(_mpb, materialIndex);
        _mpb.SetColor(colorProperty, c);
        targetRenderer.SetPropertyBlock(_mpb, materialIndex);
    }

    private void OnCollisionEnter(Collision collision)
    {
        RegisterPlayerEnter(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        RegisterPlayerExit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterPlayerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        RegisterPlayerExit(other);
    }

    private void RegisterPlayerEnter(Collider other)
    {
        if (!IsPlayerCollider(other))
            return;

        _playerCollidersOnGoal.Add(other);

        if (_goalEnabled && newPhaseManager != null)
            newPhaseManager.OnPlayerTouchedGoal(GetGoalTopCenter());
    }

    private void RegisterPlayerExit(Collider other)
    {
        if (!IsPlayerCollider(other))
            return;

        _playerCollidersOnGoal.Remove(other);

        if (!IsPlayerOnGoal && newPhaseManager != null)
            newPhaseManager.OnPlayerExitedGoalArea();
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;

        Transform root = other.transform.root;
        if (root != null && root.CompareTag("Player"))
            return true;

        return false;
    }
}