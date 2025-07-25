using UnityEngine;

[ExecuteInEditMode]
public class LockOnUI : MonoBehaviour
{
    private Camera playerCam;
    private MaterialPropertyBlock materialProperties;
    private Mesh lockedOnMesh;
    private Mesh aimMesh;

    public int numSegments = 50;
    public Material mat;
    public float thickness;

    [Header(header: "Locked On")] public float lockedRadiusMultiplier = 1.2f;

    public float lockedAngle;
    public Color lockedColor;
    public Vector2 surfaceDstFadeOutRange = new(x: 250, y: 80);

    [Header(header: "Aimed")] public float aimedRadiusMutliplier = 1.3f;

    public float aimedAngle;
    public Color aimedColor;

    public void DrawLockOnUI(CelestialBody body, bool lockedOn)
    {
        Init();

        var bodyCentre = body.transform.position;

        var pixelsPerUnit = (playerCam.WorldToScreenPoint(bodyCentre) -
                             playerCam.WorldToScreenPoint(bodyCentre + playerCam.transform.up)).magnitude;

        var worldThickness = thickness / pixelsPerUnit;

        var innerRadius = body.radius * (lockedOn ? lockedRadiusMultiplier : aimedRadiusMutliplier);
        var outerRadius = innerRadius + worldThickness;

        var numIncrements = Mathf.Max(a: 5, numSegments);

        var angle = lockedOn ? lockedAngle : aimedAngle;
        var angleIncrement = angle / (numIncrements - 1f) * Mathf.Deg2Rad;

        var verts = new Vector3[numIncrements * 2];
        var norms = new Vector3[numIncrements * 2];
        var tris = new int[(numIncrements - 1) * 2 * 3];

        // Calculate verts and triangles
        for (var i = 0; i < numIncrements; i++)
        {
            var currAngle = angleIncrement * i;
            var dir = new Vector3(Mathf.Cos(currAngle), Mathf.Sin(currAngle));
            verts[i * 2] = dir * innerRadius;
            verts[i * 2 + 1] = dir * outerRadius;

            if (i < numIncrements - 1)
            {
                tris[i * 6] = i * 2;
                tris[i * 6 + 1] = i * 2 + 1;
                tris[i * 6 + 2] = i * 2 + 2;

                tris[i * 6 + 3] = i * 2 + 2;
                tris[i * 6 + 4] = i * 2 + 1;
                tris[i * 6 + 5] = i * 2 + 3;
            }
        }

        var mesh = lockedOn ? lockedOnMesh : aimMesh;

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateBounds();

        // Draw mesh 4 times around planet
        var drawAngle = 45 - angle / 2;
        var dirToPlayer = (playerCam.transform.position - body.transform.position).normalized;

        var rot = Quaternion.AngleAxis(drawAngle, dirToPlayer) *
                  Quaternion.LookRotation(dirToPlayer, playerCam.transform.up);

        var rot90 = Quaternion.AngleAxis(angle: 90, dirToPlayer);

        var dstToBodySurface = Mathf.Max(a: 0,
            (playerCam.transform.position - body.transform.position).magnitude - body.radius);

        var alpha = Mathf.InverseLerp(surfaceDstFadeOutRange.y, surfaceDstFadeOutRange.x, dstToBodySurface);
        var displayCol = lockedOn ? lockedColor : aimedColor;

        displayCol = new Color(displayCol.r, displayCol.g, displayCol.b, alpha);
        materialProperties.SetColor(name: "_Color", displayCol);

        for (var i = 0; i < 4; i++)
        {
            rot = rot90 * rot;

            Graphics.DrawMesh(mesh, body.transform.position, rot, mat, layer: 0, camera: null, submeshIndex: 0,
                materialProperties, castShadows: false, receiveShadows: false, useLightProbes: false);

            drawAngle += 90;
        }
    }

    private void Init()
    {
        if (materialProperties == null)
        {
            materialProperties = new MaterialPropertyBlock();
        }

        if (lockedOnMesh == null)
        {
            lockedOnMesh = new Mesh();
        }

        if (aimMesh == null)
        {
            aimMesh = new Mesh();
        }

        if (playerCam == null)
        {
            playerCam = Camera.main;
        }
    }
}