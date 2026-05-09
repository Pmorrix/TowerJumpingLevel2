using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically distributes direct child UI elements of a RectTransform
/// horizontally and/or vertically, similar to a lightweight layout group.
/// Attach this script to a UI GameObject that has a RectTransform (the "container").
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class AutoGridDistributor : MonoBehaviour
{
    [Header("Distribution Axes")]
    [Tooltip("If enabled, children are positioned left-to-right within the container.")]
    [SerializeField] private bool distributeHorizontally = true;

    [Tooltip("If enabled, children are positioned top-to-bottom within the container.")]
    [SerializeField] private bool distributeVertically = false;

    [Header("Layout")]
    [Tooltip("Number of columns. If 0, it is automatically chosen based on child count (and rows if specified).")]
    [Min(0)]
    [SerializeField] private int columns = 0;

    [Tooltip("Number of rows. If 0, it is automatically chosen based on child count (and columns if specified).")]
    [Min(0)]
    [SerializeField] private int rows = 0;

    [Tooltip("Space between items (X = horizontal, Y = vertical) in pixels.")]
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);

    [Tooltip("Padding inside the container (Left, Right, Top, Bottom) in pixels.")]
    [SerializeField] private RectOffset padding = new RectOffset(10, 10, 10, 10);

    [Tooltip("If true, children are forced to a uniform size that fits the container. If false, their current size is preserved.")]
    [SerializeField] private bool forceUniformChildSize = true;

    [Tooltip("If true, the layout updates every frame while enabled (useful during runtime when children change).")]
    [SerializeField] private bool updateContinuously = false;

    private RectTransform _container;

    private void OnEnable()
    {
        _container = GetComponent<RectTransform>();
        Rebuild();
    }

    private void OnValidate()
    {
        _container = GetComponent<RectTransform>();
        Rebuild();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Container resized -> re-layout.
        Rebuild();
    }

    private void OnTransformChildrenChanged()
    {
        // Children added/removed/reordered -> re-layout.
        Rebuild();
    }

    private void Update()
    {
        // Optional continuous updating (e.g., if child sizes change externally each frame).
        if (updateContinuously)
            Rebuild();
    }

    /// <summary>
    /// Rebuild the layout by positioning (and optionally sizing) direct children.
    /// </summary>
    public void Rebuild()
    {
        if (!isActiveAndEnabled) return;
        if (_container == null) _container = GetComponent<RectTransform>();

        // Collect active child RectTransforms (direct children only).
        int childCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var rt = transform.GetChild(i) as RectTransform;
            if (rt != null && rt.gameObject.activeSelf)
                childCount++;
        }
        if (childCount == 0) return;

        // Determine how many items per row/column based on axis selection.
        // If both axes are enabled -> grid.
        // If only horizontal -> single row.
        // If only vertical -> single column.
        bool grid = distributeHorizontally && distributeVertically;

        int cols;
        int rws;

        if (!distributeHorizontally && !distributeVertically)
        {
            // Nothing to do.
            return;
        }
        else if (!grid)
        {
            if (distributeHorizontally)
            {
                rws = 1;
                cols = childCount;
            }
            else
            {
                cols = 1;
                rws = childCount;
            }
        }
        else
        {
            // Grid mode: honor columns/rows if provided; otherwise compute.
            cols = columns;
            rws = rows;

            if (cols <= 0 && rws <= 0)
            {
                // Choose a near-square grid.
                cols = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                rws = Mathf.CeilToInt(childCount / (float)cols);
            }
            else if (cols <= 0 && rws > 0)
            {
                cols = Mathf.CeilToInt(childCount / (float)rws);
            }
            else if (rws <= 0 && cols > 0)
            {
                rws = Mathf.CeilToInt(childCount / (float)cols);
            }

            cols = Mathf.Max(1, cols);
            rws = Mathf.Max(1, rws);
        }

        // Container usable area (inside padding).
        Rect rect = _container.rect;
        float innerWidth = rect.width - padding.left - padding.right;
        float innerHeight = rect.height - padding.top - padding.bottom;

        // Avoid division by zero.
        int usedCols = Mathf.Max(1, cols);
        int usedRows = Mathf.Max(1, rws);

        // Compute cell size if uniform.
        Vector2 cellSize = Vector2.zero;
        if (forceUniformChildSize)
        {
            float totalSpacingX = (usedCols - 1) * spacing.x;
            float totalSpacingY = (usedRows - 1) * spacing.y;

            float w = (innerWidth - totalSpacingX) / usedCols;
            float h = (innerHeight - totalSpacingY) / usedRows;

            // Clamp at non-negative to avoid inverted sizes in tiny containers.
            cellSize = new Vector2(Mathf.Max(0f, w), Mathf.Max(0f, h));
        }

        // Start point: top-left in the container's local space.
        // We'll position via anchors and pivot set to top-left for predictable layout.
        // (We do this per child to avoid requiring any other components.)
        int index = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf)
                continue;

            // Compute grid coordinates.
            int col, row;
            if (!grid)
            {
                // Single row or single column.
                col = distributeHorizontally ? index : 0;
                row = distributeVertically ? index : 0;
            }
            else
            {
                col = index % usedCols;
                row = index / usedCols;
            }

            // If we exceeded planned rows, expand rows dynamically to fit all items.
            // This prevents items from stacking at the bottom if child count > cols*rows.
            if (grid && row >= usedRows)
            {
                usedRows = row + 1;

                if (forceUniformChildSize)
                {
                    float totalSpacingY = (usedRows - 1) * spacing.y;
                    float h = (innerHeight - totalSpacingY) / usedRows;
                    cellSize.y = Mathf.Max(0f, h);
                }
            }

            // Force child anchors/pivot to top-left so (x,y) offsets are intuitive.
            child.anchorMin = new Vector2(0f, 1f);
            child.anchorMax = new Vector2(0f, 1f);
            child.pivot = new Vector2(0f, 1f);

            // Apply uniform size if enabled.
            if (forceUniformChildSize)
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
            if (forceUniformChildSize)
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

            // Position: x increases to the right, y decreases downward (top-left pivot).
            float x = padding.left + col * (GetChildWidth(child, cellSize.x) + spacing.x);
            float y = -padding.top - row * (GetChildHeight(child, cellSize.y) + spacing.y);

            // If only one axis is enabled, keep the other axis where it belongs:
            // - Horizontal only: keep current Y (top-left anchored), but still apply padding top.
            // - Vertical only: keep current X (top-left anchored), but still apply padding left.
            if (!grid)
            {
                if (distributeHorizontally && !distributeVertically)
                {
                    // One row: y fixed at top padding.
                    y = -padding.top;
                }
                else if (distributeVertically && !distributeHorizontally)
                {
                    // One column: x fixed at left padding.
                    x = padding.left;
                }
            }

            child.anchoredPosition = new Vector2(x, y);

            index++;
        }
    }

    // Helper to get width used for stepping. If uniform sizing is active, use that; otherwise use current rect width.
    private float GetChildWidth(RectTransform child, float uniformWidth)
    {
        return forceUniformChildSize ? uniformWidth : child.rect.width;
    }

    // Helper to get height used for stepping. If uniform sizing is active, use that; otherwise use current rect height.
    private float GetChildHeight(RectTransform child, float uniformHeight)
    {
        return forceUniformChildSize ? uniformHeight : child.rect.height;
    }
}