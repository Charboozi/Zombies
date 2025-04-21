// EnemyRampage.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRampage : MonoBehaviour
{
    [Header("Eye Renderers")]
    [Tooltip("Assign the Renderer(s) whose material you want tinted and emissive during rampage.")]
    [SerializeField] private Renderer[] eyeRenderers;

    private NavMeshAgent agent;
    private float        originalSpeed;

    // Per‐renderer original colors
    private Color[] originalBaseColors;
    private Color[] originalEmissionColors;

    private bool isRaging = false;

    private void Awake()
    {
        agent         = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;

        int count = eyeRenderers.Length;
        originalBaseColors     = new Color[count];
        originalEmissionColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            var mat = eyeRenderers[i].material;
            // capture base color
            if (mat.HasProperty("_BaseColor"))
                originalBaseColors[i] = mat.GetColor("_BaseColor");
            else
                originalBaseColors[i] = mat.GetColor("_Color");

            // capture emission color (if any)
            if (mat.HasProperty("_EmissionColor"))
            {
                originalEmissionColors[i] = mat.GetColor("_EmissionColor");
            }
            else
            {
                originalEmissionColors[i] = Color.black;
            }
        }
    }

    private void OnEnable()
    {
        if (RampageManager.Instance != null)
        {
            // subscribe
            RampageManager.Instance.OnRampageStart += ApplyRampage;
            RampageManager.Instance.OnRampageEnd   += RevertRampage;

            // if rampage was already active before we spawned, force it on us
            if (RampageManager.Instance.IsRampageActive)
            {
                ApplyRampage(
                  RampageManager.Instance.SpeedMultiplier,
                  RampageManager.Instance.RampageEyeColor
                );
            }
        }
    }

    private void OnDisable()
    {
        if (RampageManager.Instance != null)
        {
            RampageManager.Instance.OnRampageStart -= ApplyRampage;
            RampageManager.Instance.OnRampageEnd   -= RevertRampage;
        }
    }

    private void ApplyRampage(float speedMultiplier, Color eyeColor)
    {
        if (isRaging) return;
        isRaging = true;

        // 1) Speed up
        agent.speed = originalSpeed * speedMultiplier;
        Debug.Log($"Speed changed to: {agent.speed}");

        // 2) Tint base color and enable emission
        for (int i = 0; i < eyeRenderers.Length; i++)
        {
            var rend = eyeRenderers[i];
            var mat  = rend.material;

            // Base color
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", eyeColor);
            else
                mat.SetColor("_Color", eyeColor);

            // Emission
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", eyeColor);
            }
        }
    }

    private void RevertRampage()
    {
        if (!isRaging) return;
        isRaging = false;

        // 1) Restore speed
        agent.speed = originalSpeed;

        // 2) Restore base + emission colors
        for (int i = 0; i < eyeRenderers.Length; i++)
        {
            var rend = eyeRenderers[i];
            var mat  = rend.material;

            // Base color back
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", originalBaseColors[i]);
            else
                mat.SetColor("_Color", originalBaseColors[i]);

            // Emission back
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColors[i]);
                // Optionally disable emission keyword if original was black
                if (originalEmissionColors[i] == Color.black)
                    mat.DisableKeyword("_EMISSION");
            }
        }
    }
}
