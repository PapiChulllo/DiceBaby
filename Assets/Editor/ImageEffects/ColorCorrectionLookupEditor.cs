using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [CustomEditor(typeof(ColorCorrectionLookup))]
    class ColorCorrectionLookupEditor : Editor
    {
        SerializedObject serObj;
        private Texture2D tempClutTex2D;

        void OnEnable()
        {
            serObj = new SerializedObject(target);
        }

        public override void OnInspectorGUI()
        {
            serObj.Update();

            EditorGUILayout.LabelField(
                "Converts textures into color lookup volumes (for grading)",
                EditorStyles.miniLabel
            );

            tempClutTex2D = EditorGUILayout.ObjectField(
                " Based on",
                tempClutTex2D,
                typeof(Texture2D),
                false
            ) as Texture2D;

            if (tempClutTex2D == null)
            {
                Texture2D loaded =
                    AssetDatabase.LoadMainAssetAtPath(
                        ((ColorCorrectionLookup)target).basedOnTempTex
                    ) as Texture2D;

                if (loaded)
                    tempClutTex2D = loaded;
            }

            Texture2D tex = tempClutTex2D;

            if (tex && (target as ColorCorrectionLookup).basedOnTempTex != AssetDatabase.GetAssetPath(tex))
            {
                EditorGUILayout.Separator();

                if (!(target as ColorCorrectionLookup).ValidDimensions(tex))
                {
                    EditorGUILayout.HelpBox(
                        "Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.",
                        MessageType.Warning
                    );
                }
                else if (GUILayout.Button("Convert and Apply"))
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

                    bool doImport = false;

                    if (!textureImporter.isReadable)
                        doImport = true;

                    if (textureImporter.mipmapEnabled)
                        doImport = true;

                    if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                        doImport = true;

                    if (doImport)
                    {
                        textureImporter.isReadable = true;
                        textureImporter.mipmapEnabled = false;
                        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                        textureImporter.sRGBTexture = true;

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }

                    (target as ColorCorrectionLookup).Convert(tex, path);
                }
            }

            if ((target as ColorCorrectionLookup).basedOnTempTex != "")
            {
                EditorGUILayout.HelpBox(
                    "Using " + (target as ColorCorrectionLookup).basedOnTempTex,
                    MessageType.Info
                );

                Texture2D preview =
                    AssetDatabase.LoadMainAssetAtPath(
                        ((ColorCorrectionLookup)target).basedOnTempTex
                    ) as Texture2D;

                if (preview)
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    r = GUILayoutUtility.GetRect(r.width, 20);
                    r.x += r.width * 0.05f / 2.0f;
                    r.width *= 0.95f;

                    GUI.DrawTexture(r, preview);
                    GUILayoutUtility.GetRect(r.width, 4);
                }
            }

            serObj.ApplyModifiedProperties();
        }
    }
}
