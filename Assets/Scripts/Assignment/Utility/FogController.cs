using System;
using UnityEngine;

public class FogController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera   _fogRevealCamera;
    [SerializeField] private Material _accumulateMaterial;

    [Header("Render Textures")]
    [SerializeField] private RenderTexture _fogRevealRT;

    [Header("Texture Settings")]
    [SerializeField] private int _resolution = 256;

    [Header("Fog Plane")]
    [SerializeField] private Renderer _fogPlaneRenderer;

    private static readonly int FogTexID = Shader.PropertyToID("_FogTex");

    private RenderTexture _accumulateA;
    private RenderTexture _accumulateB;

    private bool _useA = true;

    public RenderTexture CurrentAccumulateRT => _useA ? _accumulateA : _accumulateB;

    private static readonly int PrevAccumulateID = Shader.PropertyToID("_PrevAccumulate");
    private static readonly int NewRevealID      = Shader.PropertyToID("_NewReveal");

    private void Awake()
    {
        CreateAccumulateBuffers();
    }

    private void CreateAccumulateBuffers()
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor(_resolution, _resolution, RenderTextureFormat.R8, 0);

        _accumulateA = new RenderTexture(desc);
        _accumulateB = new RenderTexture(desc);

        _accumulateA.Create();
        _accumulateB.Create();

        ClearToBlack(_accumulateA);
        ClearToBlack(_accumulateB);
    }

    private void ClearToBlack(RenderTexture rt)
    {
        RenderTexture prev = RenderTexture.active;

        RenderTexture.active = rt;

        GL.Clear(true, true, Color.black);

        RenderTexture.active = prev;
    }

    private void LateUpdate()
    {
        if (_fogRevealCamera == null || _accumulateMaterial == null) return;

        AccumulateFog();

        UpdateFogPlane();
    }

    private void UpdateFogPlane()
    {
        if (_fogPlaneRenderer == null) return;

        _fogPlaneRenderer.material.SetTexture(FogTexID, CurrentAccumulateRT);
    }

    private void AccumulateFog()
    {
        RenderTexture source = _useA ? _accumulateA : _accumulateB;
        RenderTexture target = _useA ? _accumulateB : _accumulateA;

        _accumulateMaterial.SetTexture(PrevAccumulateID, source);
        _accumulateMaterial.SetTexture(NewRevealID, _fogRevealRT);

        Graphics.Blit(null, target, _accumulateMaterial);

        _useA = !_useA;
    }

    private void OnDestroy()
    {
        if (_accumulateA != null) _accumulateA.Release();
        if (_accumulateB != null) _accumulateB.Release();
    }
}
