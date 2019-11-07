using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.Experimental.TerrainAPI;
using System.Collections.Generic;

public class SetHeightAndTexture : TerrainPaintTool<SetHeightAndTexture>
{
    enum BrushPreviewType
    {
        Brush = 0,
        Mesh = 1,
    }

    int m_selectedLayer = 0;
    private float m_rotation = 0;
    private float m_targetHeight = 0;

    private Material m_setHeightMaterial;
    Material setHeightMaterial
    {
        get
        {
            if(m_setHeightMaterial == null)
            {
                m_setHeightMaterial = new Material(Shader.Find("SetHeight"));
            }

            return m_setHeightMaterial;
        }
    }

#if UNITY_2019_1_OR_NEWER
    [Shortcut("Terrain/Select SetHeight and Texture", typeof(TerrainToolShortcutContext), KeyCode.F1)]
    static void SelectShortcut(ShortcutArguments args)
    {
        TerrainToolShortcutContext context = (TerrainToolShortcutContext)args.context;
        context.SelectPaintTool<SetHeightAndTexture>();
    }
#endif

    public override string GetName()
    {
        return "Set Height & Texture";
    }

    public override string GetDesc()
    {
        return "Paints height using a brush mask and also paints the selected Terain Layer based using the same brush mask.\n\nHold Shift to paint in the opposite direction of the target height.\n\nCtrl + Left Click to sample target height from terrain.\n\nTool Shortcut = F1";
    }

    public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
    {
        editContext.ShowBrushesGUI(5, BrushGUIEditFlags.Select | BrushGUIEditFlags.Size | BrushGUIEditFlags.Opacity);

        m_selectedLayer = TerrainLayerUtility.ShowTerrainLayersSelectionHelper(terrain, m_selectedLayer);

        m_rotation = EditorGUILayout.Slider("Rotation", m_rotation, -1, 360) % 300;
        m_targetHeight = EditorGUILayout.Slider("Target height", m_targetHeight, terrain.transform.position.y, terrain.transform.position.y + terrain.terrainData.size.y );

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel("Fill:");

            if(GUILayout.Button("Tile"))
            {
                FillTile(terrain);
            }

            if(GUILayout.Button("Group"))
            {
                int groupID = terrain.groupingID;

                TerrainUtility.TerrainMap map = TerrainUtility.TerrainMap.CreateFromPlacement(terrain, (t) => { return t.groupingID == groupID; });

                foreach(KeyValuePair<TerrainUtility.TerrainMap.TileCoord, Terrain>  t in map.m_terrainTiles)
                {
                    FillTile(t.Value);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void FillTile(Terrain terrain)
    {
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, Vector2.one * .5f, terrain.terrainData.size.x, m_rotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

        Material mat = setHeightMaterial;

        float terrainHeight = Mathf.Clamp01((m_targetHeight - terrain.transform.position.y) / terrain.terrainData.size.y);

        Vector4 brushParams = new Vector4(1f, .5f * terrainHeight, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", Texture2D.whiteTexture);
        mat.SetVector("_BrushParams", brushParams);

        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

        // restore old render target
        RenderTexture.active = paintContext.oldRenderTexture;

        TerrainPaintUtility.EndPaintHeightmap(paintContext, "TestTerrainTool Fill");
        TerrainPaintUtility.ReleaseContextResources(paintContext);
    }

    public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
    {
        // only do the rest if user mouse hits valid terrain or they are using the
        // brush parameter hotkeys to resize, etc
        if (!editContext.hitValidTerrain)
        {
            return;
        }

        if(Event.current.control && Event.current.button == 0)
        {
            m_targetHeight = editContext.raycastHit.point.y;
        }

        // dont render preview if this isnt a repaint. losing performance if we do
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        // draw result preview
        {
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.raycastHit.textureCoord,
                                                                                    editContext.brushSize, m_rotation + 45f);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
            Material material = TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial();
            
            TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainPaintUtilityEditor.BrushPreview.SourceRenderTexture,
                                                       editContext.brushTexture, brushXform, material, (int)BrushPreviewType.Brush);

            TerrainPaintUtility.ReleaseContextResources(paintContext);
        }
    }

    private void PaintTexture(Terrain terrain, IOnPaint editContext)
    {
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize * .7f, m_rotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintTexture(terrain, brushXform.GetBrushXYBounds(),
                                                                          terrain.terrainData.terrainLayers[m_selectedLayer], 1);

        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();

        float brushStrength = Event.current.shift ? -editContext.brushStrength : editContext.brushStrength;

        Vector4 brushParams = new Vector4(brushStrength, 1, 0, 0);
        mat.SetTexture("_BrushTex", editContext.brushTexture);
        mat.SetVector("_BrushParams", brushParams);

        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.PaintTexture);

        // restore old render target
        RenderTexture.active = paintContext.oldRenderTexture;

        TerrainPaintUtility.EndPaintTexture(paintContext, "TestTerrainTool Texture");
    }

    private void PaintHeight(Terrain terrain, IOnPaint editContext)
    {
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, m_rotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        int pass = (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SetHeights;

        if(Event.current.shift)
        {
            mat = setHeightMaterial;
            pass = 1;
        }

        float terrainHeight = Mathf.Clamp01((m_targetHeight - terrain.transform.position.y) / terrain.terrainData.size.y);

        float brushStrength = Event.current.shift ? -editContext.brushStrength : editContext.brushStrength;
        
        Vector4 brushParams = new Vector4(brushStrength * .01f, .5f * terrainHeight, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", editContext.brushTexture);
        mat.SetVector("_BrushParams", brushParams);

        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, pass);

        // restore old render target
        RenderTexture.active = paintContext.oldRenderTexture;

        TerrainPaintUtility.EndPaintHeightmap(paintContext, "TestTerrainTool Height");
        TerrainPaintUtility.ReleaseContextResources(paintContext);
    }

    public override bool OnPaint(Terrain terrain, IOnPaint editContext)
    {
        PaintHeight(terrain, editContext);
        PaintTexture(terrain, editContext);

        return true;
    }
}