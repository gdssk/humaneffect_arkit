using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MySample : MonoBehaviour
{
    /// <summary>
    /// The display rotation matrix for the shader.
    /// </summary>
    private Matrix4x4 _displayRotationMatrix = Matrix4x4.identity;
    
    /// <summary>
    /// Camera
    /// </summary>
    [SerializeField]
    private ARCameraManager _cameraManager;
    
    /// <summary>
    /// Occlusion
    /// </summary>
    [SerializeField]
    private AROcclusionManager _occlusionManager;

    /// <summary>
    /// Material
    /// </summary>
    [SerializeField]
    private Material _material;

    /// <summary>
    /// OnEnable
    /// </summary>
    private void OnEnable()
    {
        _cameraManager.frameReceived += OnCameraFrameEventReceived;
    }

    /// <summary>
    /// OnDisable
    /// </summary>
    private void OnDisable()
    {
        _cameraManager.frameReceived -= OnCameraFrameEventReceived;
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        var humanStencil = _occlusionManager.humanStencilTexture;
        var humanDepth = _occlusionManager.humanDepthTexture;
        var envDepth = _occlusionManager.environmentDepthTexture;
        
        _material.SetMatrix("_DisplayRotationPerFrame", _displayRotationMatrix);
        _material.SetTexture("_StencilTex", humanStencil);;
    }

    /// <summary>
    /// OnCameraFrameEventReceived
    /// </summary>
    /// <param name="args">args</param>
    private void OnCameraFrameEventReceived(ARCameraFrameEventArgs args)
    {
        // Try receiving Y/CbCr textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            var tex = args.textures[i];
            if (id == Shader.PropertyToID("_textureY"))
                _material.SetTexture("_YTex", tex);
            else if (id == Shader.PropertyToID("_textureCbCr"))
                _material.SetTexture("_CBCRTex", tex);   
        }

        var cameraMatrix = args.displayMatrix ?? Matrix4x4.identity;
        var affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[1, 0]);
        var affineBasisY = new Vector2(cameraMatrix[0, 1], cameraMatrix[1, 1]);
        var affineTranslation = new Vector2(cameraMatrix[2, 0], cameraMatrix[2, 1]);
        affineBasisX = affineBasisX.normalized;
        affineBasisY = affineBasisY.normalized;

        _displayRotationMatrix = Matrix4x4.identity;
        _displayRotationMatrix[0,0] = affineBasisX.x;
        _displayRotationMatrix[0,1] = affineBasisY.x;
        _displayRotationMatrix[1,0] = affineBasisX.y;
        _displayRotationMatrix[1,1] = affineBasisY.y;
        _displayRotationMatrix[2,0] = Mathf.Round(affineTranslation.x);
        _displayRotationMatrix[2,1] = Mathf.Round(affineTranslation.y);
    }
}
