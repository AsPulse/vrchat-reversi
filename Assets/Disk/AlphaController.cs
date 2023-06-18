    
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using static VRC.Core.Logger;
using Color = UnityEngine.Color;

public class AlphaController : UdonSharpBehaviour
{
    private MeshRenderer MeshRenderer;
    [SerializeField]
    private PositionConstraint PositionConstraint;
    private Material[] Materials = null;

    
    private float[] PresentTransparency;

    void Update()
    {
        if (Materials == null)
        {
            Debug.LogWarning("Materials are null, AlphaController skipping...");
            return;
        }
        if (PositionConstraint == null)
        {
            Debug.LogWarning("PositionConstraint is null, AlphaControlelr skipping...");
            return;
        }
        float transparency = PositionConstraint.weight;
        for (int i = 0; i < Materials.Length; i++)
        {
            if (PresentTransparency[i] == transparency) return;
            Color c = Materials[i].color;
            c.a = transparency;
            Materials[i].color = c;
            PresentTransparency[i] = transparency;
        };
    }

    void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        Materials = MeshRenderer.materials;
        PresentTransparency = new float[Materials.Length];
        for(int i = 0; i < Materials.Length; i++)
        {
            PresentTransparency[i] = Materials[i].color.a;
        }
    }

    private void OnDestroy()
    {
        for(int i = 0;  i < Materials.Length; i++) {
            Destroy(Materials[i]);
            Materials[i] = null;
        }
        Materials = null;
    }
}
