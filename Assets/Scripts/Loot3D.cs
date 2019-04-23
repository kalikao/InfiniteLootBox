using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Loot representation in 3D, after spawning from a chest.
/// </summary>
public class Loot3D : MonoBehaviour
{
    Vector3 startPos;
    Vector3 startScale;
    Vector3 rotationTurns;
    Quaternion startRotation;
    private GameObject mPoof;
    ParticleSystem.MainModule psm;
    /// <summary>
    /// Sets up the particle system used as aura.
    /// </summary>
    /// <remarks>
    /// Separate parameters for each lootTier to better differentiate between tiers.
    /// </remarks>
    void setupParticleSystem()
    {
        if (lootTier <= 1) return;

        var ps = gameObject.AddComponent<ParticleSystem>();
        var psr = gameObject.GetComponent<ParticleSystemRenderer>();
        psm = ps.main;
        var pss = ps.shape;
        var pscol = ps.colorOverLifetime;
        var pssz = ps.sizeOverLifetime;
        var psem = ps.emission;

        ps.Stop();
        if (lootTier == 2)
            psem.rateOverTime = new ParticleSystem.MinMaxCurve(0.6f);
        else if (lootTier == 3)
            psem.rateOverTime = new ParticleSystem.MinMaxCurve(0.7f);
        else if (lootTier == 4)
            psem.rateOverTime = new ParticleSystem.MinMaxCurve(1);

        if (lootTier == 2)
            psm.maxParticles = 1;
        else
            psm.maxParticles = 5;
        psm.startRotation3D = true;
        psm.prewarm = true;
        psm.loop = true;
        // psm.simulationSpace = ParticleSystemSimulationSpace.World;
        psm.simulationSpace = ParticleSystemSimulationSpace.Local;
        psm.duration = 1f;
        if (lootTier == 2)
            psm.startLifetime = new ParticleSystem.MinMaxCurve(0.7f);
        else if (lootTier == 3)
            psm.startLifetime = new ParticleSystem.MinMaxCurve(1.7f);
        else if (lootTier == 4)
            psm.startLifetime = new ParticleSystem.MinMaxCurve(5f);
        psm.startSize = new ParticleSystem.MinMaxCurve(2f);
        psm.startSpeed = new ParticleSystem.MinMaxCurve(0);
        var tierCol = GameController.instance.tierColors[lootTier];
        if (lootTier == 2)
            psm.startColor = new Color(tierCol.r, tierCol.g, tierCol.b, 0.35f);
        else if (lootTier == 3)
            psm.startColor = new Color(tierCol.r, tierCol.g, tierCol.b, 0.45f);
        else if (lootTier == 4)
            psm.startColor = new Color(tierCol.r, tierCol.g, tierCol.b, 0.4f);

        pscol.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                      new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
        pscol.color = grad;

        pssz.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.3f);
        curve.AddKey(1f, 1.0f);
        pssz.size = new ParticleSystem.MinMaxCurve(1f, curve);

        psr.renderMode = ParticleSystemRenderMode.Mesh;
        psr.sortMode = ParticleSystemSortMode.OldestInFront;
        psr.mesh = GetComponent<MeshFilter>().mesh;
        // psr.alignment = ParticleSystemRenderSpace.World;
        psr.alignment = ParticleSystemRenderSpace.Local;
        psr.material = GameController.instance.AuraMat;

        pss.shapeType = ParticleSystemShapeType.Sphere;
        pss.radius = 0.0001f;
        pss.radiusThickness = 0;
        ps.Play();

    }

    void Start()
    {
        mPoof = (GameObject)Resources.Load("Poof");
        var ps = mPoof.GetComponent<ParticleSystem>();
        var psm = ps.main;
        psm.maxParticles = GameController.Settings.PoofParticlesMax;
        var pse = ps.emission;
        pse.rateOverTime = GameController.Settings.PoofParticlesRate;

        //maybe should make a setupOnChild version, for multi-part objects
        setupParticleSystem();

        startPos = transform.position;
        startScale = transform.localScale;
        int rMin = -2, rMax = 2;
        rotationTurns = new Vector3(360 * Random.Range(rMin, rMax), 360 * Random.Range(rMin, rMax), 360 * Random.Range(rMin, rMax));
        if (rotationTurns.x == 0) rotationTurns.x = 360;
        if (rotationTurns.y == 0) rotationTurns.y = 360;
        if (rotationTurns.z == 0) rotationTurns.z = 360;
        startRotation = transform.rotation;
    }
    /// <summary>
    /// Get the name of the item.
    /// </summary>
    /// <remarks>
    /// Returns public property of the prefab if it's set, or the name of the prefab otherwise.
    /// </remarks>
    public string Name
    {
        get { return lootName.Trim() != "" ? lootName : iconName; }
        set { lootName = value; }
    }

    [HideInInspector]
    public string iconName;

    [HideInInspector]
    public int lootTier;
    public string lootName;
    float tRot = 0; //Rotation parametrization
    float tPos = 0; //Position parametrization
    float tSca = 0; //Scale parametrization
    bool isInInventory = false;
    void Update()
    {
        // return; // Stop movement for screenshots with ScreenshotCamera in another scene.

        var cam = Camera.main;
        tRot += 0.21f * Time.deltaTime;
        tPos += 0.21f * Time.deltaTime;
        tSca += 1f * Time.deltaTime;
        if (tPos > 1) tPos = 1;
        if (tRot > 1) tRot = 1;
        if (tSca > 1) tSca = 1;
        transform.localScale = startScale * tSca;
        transform.rotation = startRotation;
        transform.Rotate(Vector3.LerpUnclamped(Vector3.zero, rotationTurns, EasingFunction.EaseInOutCubic(0, 1, tRot)));

        Vector3 targetPosition = cam.ViewportToWorldPoint(new Vector3(0.25f, 0.77f, 4f + cam.nearClipPlane));
        transform.position = Vector3.LerpUnclamped(startPos, targetPosition, 0.2f * tPos + 0.8f * EasingFunction.EaseOutElastic(0, 1, tPos));
        if (tPos == 1 && tRot == 1 && !isInInventory)
        {
            var pf = Instantiate(mPoof, transform.position, transform.rotation);
            var pfpsm = pf.GetComponent<ParticleSystem>().main;
            pfpsm.startColor = GameController.instance.tierColors[lootTier];
            Destroy(pf, 3f);

            Destroy(gameObject);//, 0.1f);
            var inventoryIcon = GameController.instance.imgDict[iconName != "" ? iconName : name]; 
            Loot2D.Create(inventoryIcon, name, lootTier);
            isInInventory = true;
        }
    }
}
