using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlaybackTester : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private KeyCode toggleKey = KeyCode.F8;
    [SerializeField] private float autoStartDelay = 0.5f;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerLanding playerLanding;
    [SerializeField] private PlayerRespawnOnFloor playerRespawn;
    [SerializeField] private PlayerVisualFacing playerVisualFacing;
    [SerializeField] private PlayerVisualAnimatorBridge playerVisualAnimatorBridge;
    [SerializeField] private BuildingGoalController goalController;
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private MonoBehaviour[] extraBehavioursToDisable;

    [Header("Route")]
    [SerializeField] private bool includeGoalAtEnd = true;
    [SerializeField] private float landingHeightOffset = 0.08f;
    [SerializeField] private float landingPause = 0.08f;
    [SerializeField] private float surfaceProbeHeight = 6f;
    [SerializeField] private float surfaceProbeRadius = 0.2f;

    [Header("Jump Motion")]
    [SerializeField] private float secondsPerUnit = 0.09f;
    [SerializeField] private float minJumpDuration = 0.32f;
    [SerializeField] private float maxJumpDuration = 0.9f;
    [SerializeField] private float minArcHeight = 1.4f;
    [SerializeField] private float maxArcHeight = 4.5f;
    [SerializeField] private float arcHeightPerUnit = 0.14f;
    [SerializeField] private bool faceTravelDirection = true;

    [Header("Playback Isolation")]
    [SerializeField] private bool disablePlayerColliders = true;
    [SerializeField] private bool pauseGameplaySystems = true;
    [SerializeField] private bool resetPlayerToStartOnStop = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;
    [SerializeField] private bool drawRouteGizmos = true;
    [SerializeField] private bool drawRouteAlways = true;
    [SerializeField] private float gizmoSphereRadius = 0.35f;
    [SerializeField] private Color routeLineColor = new Color(0.2f, 0.95f, 1f, 0.9f);
    [SerializeField] private Color routePointColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    [SerializeField] private Color goalPointColor = new Color(0.2f, 1f, 0.45f, 0.95f);
    [SerializeField] private Color activeTargetColor = new Color(1f, 0.35f, 0.35f, 1f);

    private readonly List<MonoBehaviour> _disabledBehaviours = new List<MonoBehaviour>();
    private readonly List<ColliderState> _colliderStates = new List<ColliderState>();
    private readonly List<Vector3> _routePoints = new List<Vector3>();

    private Coroutine _playbackRoutine;
    private bool _hasCachedState;
    private Vector3 _cachedPosition;
    private Quaternion _cachedRotation;
    private bool _cachedIsKinematic;
    private bool _cachedUseGravity;
    private int _currentRouteIndex = -1;
    private struct ColliderState
    {
        public Collider collider;
        public bool wasEnabled;
    }

    private IEnumerator Start()
    {
        if (!playOnStart)
            yield break;

        if (autoStartDelay > 0f)
            yield return new WaitForSeconds(autoStartDelay);

        Play();
    }

    private void Update()
    {
        if (toggleKey == KeyCode.None)
            return;

        if (!Input.GetKeyDown(toggleKey))
            return;

        if (_playbackRoutine != null)
            StopPlayback();
        else
            Play();
    }

    private void OnDisable()
    {
        if (_playbackRoutine != null)
        {
            StopCoroutine(_playbackRoutine);
            _playbackRoutine = null;
        }

        RestoreAfterPlayback(true);
    }

    private void OnDrawGizmos()
    {
        if (!drawRouteGizmos || !drawRouteAlways)
            return;

        DrawRouteGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawRouteGizmos || drawRouteAlways)
            return;

        DrawRouteGizmos();
    }

    [ContextMenu("Play Level Tester")]
    public void Play()
    {
        if (!Application.isPlaying)
            return;

        if (_playbackRoutine != null)
            return;

        if (!ResolveReferences())
            return;

        if (!BuildRoute(_routePoints))
        {
            Debug.LogWarning("[LevelPlaybackTester] No se pudo construir una ruta de prueba.");
            return;
        }

        _playbackRoutine = StartCoroutine(PlaybackRoutine());
    }

    [ContextMenu("Stop Level Tester")]
    public void StopPlayback()
    {
        if (_playbackRoutine == null)
            return;

        StopCoroutine(_playbackRoutine);
        _playbackRoutine = null;
        RestoreAfterPlayback(true);
    }

    private IEnumerator PlaybackRoutine()
    {
        CacheState();
        TakeControl();

        if (verboseLogs)
            Debug.Log($"[LevelPlaybackTester] Reproduciendo {_routePoints.Count} saltos.");

        for (int i = 0; i < _routePoints.Count; i++)
        {
            _currentRouteIndex = i;
            Vector3 destination = _routePoints[i];
            yield return JumpTo(destination);
            SnapPlayerToSurface();

            if (landingPause > 0f)
                yield return new WaitForSeconds(landingPause);
        }

        _currentRouteIndex = -1;
        _playbackRoutine = null;
        RestoreAfterPlayback(false);
    }

    private bool ResolveReferences()
    {
        if (playerTransform == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
                playerTransform = taggedPlayer.transform;
        }

        if (playerTransform == null && playerMove != null)
            playerTransform = playerMove.transform;

        if (playerTransform == null && playerJump != null)
            playerTransform = playerJump.transform;

        if (playerTransform == null)
        {
            Debug.LogWarning("[LevelPlaybackTester] No encuentro el Player. Asigna el Transform o usa el tag Player.");
            return false;
        }

        if (playerRigidbody == null)
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();

        if (playerMove == null)
            playerMove = playerTransform.GetComponent<PlayerMove>();

        if (playerJump == null)
            playerJump = playerTransform.GetComponent<PlayerJump>();

        if (playerLanding == null)
            playerLanding = playerTransform.GetComponent<PlayerLanding>();

        if (playerRespawn == null)
            playerRespawn = playerTransform.GetComponent<PlayerRespawnOnFloor>();

        if (playerVisualFacing == null)
            playerVisualFacing = playerTransform.GetComponent<PlayerVisualFacing>();

        if (playerVisualAnimatorBridge == null)
            playerVisualAnimatorBridge = playerTransform.GetComponent<PlayerVisualAnimatorBridge>();

        if (goalController == null)
            goalController = FindObjectOfType<BuildingGoalController>();

        if (phaseManager == null)
            phaseManager = FindObjectOfType<PhaseManager>();

        return true;
    }

    private bool BuildRoute(List<Vector3> routePoints)
    {
        routePoints.Clear();

        BuildingTimeController[] allBuildings = FindObjectsOfType<BuildingTimeController>();
        List<BuildingTimeController> pending = new List<BuildingTimeController>();

        for (int i = 0; i < allBuildings.Length; i++)
        {
            BuildingTimeController building = allBuildings[i];
            if (building == null || !building.isActiveAndEnabled)
                continue;

            if (IsGoalBuilding(building.transform))
                continue;

            pending.Add(building);
        }

        if (pending.Count == 0 && !includeGoalAtEnd)
            return false;

        Vector3 cursor = playerTransform.position;

        while (pending.Count > 0)
        {
            int bestIndex = 0;
            float bestSqrDistance = float.MaxValue;

            for (int i = 0; i < pending.Count; i++)
            {
                Vector3 candidatePoint = GetLandingPoint(pending[i].transform);
                float sqrDistance = GetPlanarSqrDistance(cursor, candidatePoint);

                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    bestIndex = i;
                }
            }

            Vector3 nextPoint = GetLandingPoint(pending[bestIndex].transform);
            routePoints.Add(nextPoint);
            cursor = nextPoint;
            pending.RemoveAt(bestIndex);
        }

        if (includeGoalAtEnd && goalController != null)
            routePoints.Add(GetGoalLandingPoint());

        return routePoints.Count > 0;
    }

    private bool IsGoalBuilding(Transform target)
    {
        if (goalController == null || target == null)
            return false;

        Transform goalTransform = goalController.transform;
        return target == goalTransform ||
               target.IsChildOf(goalTransform) ||
               goalTransform.IsChildOf(target);
    }

    private void CacheState()
    {
        if (_hasCachedState)
            return;

        _cachedPosition = playerTransform.position;
        _cachedRotation = playerTransform.rotation;

        if (playerRigidbody != null)
        {
            _cachedIsKinematic = playerRigidbody.isKinematic;
            _cachedUseGravity = playerRigidbody.useGravity;
        }

        _hasCachedState = true;
    }

    private void TakeControl()
    {
        _disabledBehaviours.Clear();
        DisableBehaviour(playerMove);
        DisableBehaviour(playerJump);
        DisableBehaviour(playerLanding);
        DisableBehaviour(playerRespawn);
        DisableBehaviour(playerVisualFacing);
        DisableBehaviour(playerVisualAnimatorBridge);

        if (pauseGameplaySystems)
            DisableBehaviour(phaseManager);

        if (extraBehavioursToDisable != null)
        {
            for (int i = 0; i < extraBehavioursToDisable.Length; i++)
                DisableBehaviour(extraBehavioursToDisable[i]);
        }

        CacheAndDisablePlayerColliders();

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.useGravity = false;
            playerRigidbody.isKinematic = true;
        }
    }

    private void RestoreAfterPlayback(bool cancelled)
    {
        if (!_hasCachedState)
            return;

        bool resetToStart = cancelled && resetPlayerToStartOnStop;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = _cachedIsKinematic;
            playerRigidbody.useGravity = _cachedUseGravity;

            if (resetToStart)
            {
                playerRigidbody.position = _cachedPosition;
                playerRigidbody.rotation = _cachedRotation;
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        if (resetToStart)
            playerTransform.SetPositionAndRotation(_cachedPosition, _cachedRotation);

        RestorePlayerColliders();

        for (int i = 0; i < _disabledBehaviours.Count; i++)
        {
            if (_disabledBehaviours[i] != null)
                _disabledBehaviours[i].enabled = true;
        }

        _disabledBehaviours.Clear();
        _hasCachedState = false;
        _currentRouteIndex = -1;
    }

    private void DisableBehaviour(MonoBehaviour behaviour)
    {
        if (behaviour == null || !behaviour.enabled)
            return;

        behaviour.enabled = false;
        _disabledBehaviours.Add(behaviour);
    }

    private void CacheAndDisablePlayerColliders()
    {
        _colliderStates.Clear();

        if (!disablePlayerColliders || playerTransform == null)
            return;

        Collider[] colliders = playerTransform.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
                continue;

            _colliderStates.Add(new ColliderState
            {
                collider = collider,
                wasEnabled = collider.enabled
            });

            collider.enabled = false;
        }
    }

    private void RestorePlayerColliders()
    {
        for (int i = 0; i < _colliderStates.Count; i++)
        {
            ColliderState state = _colliderStates[i];
            if (state.collider != null)
                state.collider.enabled = state.wasEnabled;
        }

        _colliderStates.Clear();
    }

    private IEnumerator JumpTo(Vector3 destination)
    {
        SnapPlayerToSurface();

        Vector3 start = playerTransform.position;
        Vector3 planarDelta = new Vector3(
            destination.x - start.x,
            0f,
            destination.z - start.z
        );

        if (faceTravelDirection && planarDelta.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(planarDelta.normalized, Vector3.up);
            playerTransform.rotation = lookRotation;
        }

        float planarDistance = planarDelta.magnitude;
        float duration = Mathf.Clamp(planarDistance * secondsPerUnit, minJumpDuration, maxJumpDuration);
        float arcHeight = Mathf.Clamp(minArcHeight + planarDistance * arcHeightPerUnit, minArcHeight, maxArcHeight);

        if (duration <= 0f)
        {
            SetPlayerPosition(destination);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 position = Vector3.Lerp(start, destination, t);
            float arc = 4f * t * (1f - t);
            position.y += arc * arcHeight;

            SetPlayerPosition(position);
            yield return null;
        }

        SetPlayerPosition(destination);
    }

    private void SetPlayerPosition(Vector3 position)
    {
        if (playerRigidbody != null)
            playerRigidbody.position = position;

        playerTransform.position = position;
    }

    private Vector3 GetGoalLandingPoint()
    {
        if (goalController == null)
            return playerTransform.position;

        Vector3 goalTop = goalController.GetGoalTopCenter();
        return GetSurfaceLandingPoint(goalController.transform, goalTop);
    }

    private Vector3 GetLandingPoint(Transform target)
    {
        Bounds bounds;
        if (TryGetCombinedBounds(target, out bounds))
        {
            Vector3 topCenter = new Vector3(
                bounds.center.x,
                bounds.max.y,
                bounds.center.z
            );

            return GetSurfaceLandingPoint(target, topCenter);
        }

        Vector3 position = target.position;
        return GetSurfaceLandingPoint(target, position);
    }

    private bool TryGetCombinedBounds(Transform target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        bounds = default;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (hasBounds)
            return true;

        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    private float GetPlanarSqrDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    private Vector3 GetSurfaceLandingPoint(Transform target, Vector3 fallbackCenter)
    {
        if (TryGetTopSurfacePoint(target, fallbackCenter, out Vector3 surfacePoint))
        {
            surfacePoint.y += landingHeightOffset;
            return surfacePoint;
        }

        fallbackCenter.y += landingHeightOffset;
        return fallbackCenter;
    }

    private bool TryGetTopSurfacePoint(Transform target, Vector3 center, out Vector3 surfacePoint)
    {
        surfacePoint = default;
        if (target == null)
            return false;

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
            return false;

        float highestY = float.MinValue;
        bool found = false;
        float castDistance = Mathf.Max(surfaceProbeHeight * 2f, 0.1f);
        Vector3[] offsets =
        {
            Vector3.zero,
            new Vector3(surfaceProbeRadius, 0f, 0f),
            new Vector3(-surfaceProbeRadius, 0f, 0f),
            new Vector3(0f, 0f, surfaceProbeRadius),
            new Vector3(0f, 0f, -surfaceProbeRadius),
        };

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled)
                continue;

            for (int j = 0; j < offsets.Length; j++)
            {
                Vector3 origin = new Vector3(
                    center.x + offsets[j].x,
                    center.y + surfaceProbeHeight,
                    center.z + offsets[j].z
                );

                Ray ray = new Ray(origin, Vector3.down);
                if (!collider.Raycast(ray, out RaycastHit hit, castDistance))
                    continue;

                if (hit.point.y > highestY)
                {
                    highestY = hit.point.y;
                    surfacePoint = hit.point;
                    found = true;
                }
            }
        }

        return found;
    }

    private void SnapPlayerToSurface()
    {
        if (playerTransform == null)
            return;

        Vector3 origin = playerTransform.position + Vector3.up * surfaceProbeHeight;
        float castDistance = Mathf.Max(surfaceProbeHeight * 2f, 0.1f);
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        Vector3 snappedPosition = playerTransform.position;
        snappedPosition.y = hit.point.y + landingHeightOffset;
        SetPlayerPosition(snappedPosition);
    }

    private void DrawRouteGizmos()
    {
        List<Vector3> previewRoute = GetRoutePreview();
        if (previewRoute.Count == 0)
            return;

        Vector3 startPoint = GetRouteStartPoint();
        Gizmos.color = routeLineColor;

        Vector3 previous = startPoint;
        for (int i = 0; i < previewRoute.Count; i++)
        {
            Vector3 point = previewRoute[i];
            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        for (int i = 0; i < previewRoute.Count; i++)
        {
            bool isGoalPoint = includeGoalAtEnd && i == previewRoute.Count - 1 && goalController != null;
            bool isCurrentTarget = i == _currentRouteIndex;
            Color pointColor = isGoalPoint ? goalPointColor : routePointColor;

            if (isCurrentTarget)
                pointColor = activeTargetColor;

            Gizmos.color = pointColor;
            Gizmos.DrawSphere(previewRoute[i], gizmoSphereRadius);
            Gizmos.DrawWireSphere(previewRoute[i], gizmoSphereRadius * 1.85f);
        }

        Gizmos.color = routeLineColor;
        Gizmos.DrawWireSphere(startPoint, gizmoSphereRadius * 0.75f);
    }

    private List<Vector3> GetRoutePreview()
    {
        if (_routePoints.Count > 0)
            return _routePoints;

        if (Application.isPlaying)
        {
            if (!ResolveReferences())
                return _routePoints;

            BuildRoute(_routePoints);
            return _routePoints;
        }

        List<Vector3> preview = new List<Vector3>();
        if (!TryResolveEditorReferences())
            return preview;

        BuildRoute(preview);
        return preview;
    }

    private bool TryResolveEditorReferences()
    {
        if (playerTransform == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
                playerTransform = taggedPlayer.transform;
        }

        if (playerTransform == null && playerMove != null)
            playerTransform = playerMove.transform;

        if (playerTransform == null && playerJump != null)
            playerTransform = playerJump.transform;

        if (goalController == null)
            goalController = FindObjectOfType<BuildingGoalController>();

        return playerTransform != null;
    }

    private Vector3 GetRouteStartPoint()
    {
        if (playerTransform != null)
            return playerTransform.position;

        return transform.position;
    }
}
