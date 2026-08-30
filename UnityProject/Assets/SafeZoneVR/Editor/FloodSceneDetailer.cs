#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SafeZoneVR.Editor
{
    /// <summary>
    /// Adds secondary detail geometry (caps, lenses, straps, spokes, trims...) to the existing
    /// placeholder primitives in the "Alagamento em Casa" scene so they read as real-world objects
    /// instead of bare boxes/cylinders. Purely additive: every piece is a non-colliding child mesh,
    /// so the root GameObjects keep their existing Rigidbody/XRGrabInteractable/ObjectiveItemId/
    /// Collider/XRKnob components and every scene reference to them untouched. Safe to re-run —
    /// each piece is skipped if it already exists.
    /// </summary>
    public static class FloodSceneDetailer
    {
        const string k_MaterialFolder = "Assets/SafeZoneVR/Materials";

        // Exact color constants reused from FloodSceneBuilder so materials resolve to the
        // same already-tuned assets instead of creating near-duplicates.
        static readonly Color k_MetalColor = new Color(0.5f, 0.5f, 0.55f);
        static readonly Color k_SafeGreen = new Color(0.25f, 0.6f, 0.3f);
        static readonly Color k_DangerRed = new Color(0.75f, 0.15f, 0.15f);
        static readonly Color k_DarkPlastic = new Color(0.18f, 0.18f, 0.2f);
        static readonly Color k_Cream = new Color(0.9f, 0.85f, 0.7f);
        static readonly Color k_MedsRed = new Color(0.9f, 0.2f, 0.2f);
        static readonly Color k_SafetyYellow = new Color(0.95f, 0.75f, 0.1f);

        // New colors, distinct from the shared placeholders above so recoloring the breaker
        // panel/switch housing never cascades into the flashlight body or other reused materials.
        static readonly Color k_PanelGrey = new Color(0.42f, 0.44f, 0.47f);
        static readonly Color k_SwitchHousing = new Color(0.12f, 0.12f, 0.13f);

        [MenuItem("SafeZone VR/Detalhar Objetos (Visual)/Alagamento em Casa")]
        public static void DetailScene()
        {
            DetailWaterBottle();
            DetailFlashlight();
            DetailMedicineBox();
            DetailDocuments();
            DetailPhotoFrame();
            DetailBackpack();
            DetailValveWheel();
            DetailBreakerAssembly();
            DetailTableTrim("ValuablesTable", topWorldY: 0.5f, footprintX: 0.7f, footprintZ: 0.5f, safeClearance: 0.008f);
            DetailTableTrim("KitBench", topWorldY: 0.625f, footprintX: 0.9f, footprintZ: 0.45f, safeClearance: 0.010f);
            DetailShelfBrackets();

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SafeZoneVR] Objetos detalhados para um visual mais realista.");
        }

        static void DetailWaterBottle()
        {
            var bottle = GameObject.Find("Item_Agua");
            if (bottle == null)
                return;

            var capMat = GetOrCreateMaterial(new Color(0.85f, 0.9f, 0.95f), 0.6f, 0f);
            AddChildPrimitive(bottle.transform, "Cap", PrimitiveType.Cylinder,
                worldOffset: new Vector3(0f, 0.14f, 0f), worldSize: new Vector3(0.03f, 0.025f, 0.03f), capMat);
        }

        static void DetailFlashlight()
        {
            var flashlight = GameObject.Find("Item_Lanterna");
            if (flashlight == null)
                return;

            // Recolor the body from safety yellow to a dark plastic housing; the lens carries the
            // yellow now, and reads much closer to a real flashlight.
            var bodyMat = GetOrCreateMaterial(k_DarkPlastic, 0.4f, 0.2f);
            flashlight.GetComponent<Renderer>().sharedMaterial = bodyMat;

            var lensMat = GetOrCreateMaterial(new Color(0.98f, 0.85f, 0.3f), 0.7f, 0f);
            lensMat.EnableKeyword("_EMISSION");
            lensMat.SetColor("_EmissionColor", new Color(1.4f, 1.15f, 0.35f));
            lensMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            AddChildPrimitive(flashlight.transform, "Lens", PrimitiveType.Cylinder,
                worldOffset: new Vector3(0f, 0.165f, 0f), worldSize: new Vector3(0.045f, 0.03f, 0.045f), lensMat);

            var buttonMat = GetOrCreateMaterial(k_MetalColor, 0.7f, 0.85f);
            AddChildPrimitive(flashlight.transform, "Button", PrimitiveType.Sphere,
                worldOffset: new Vector3(0.027f, 0.02f, 0f), worldSize: new Vector3(0.012f, 0.012f, 0.012f), buttonMat);
        }

        static void DetailMedicineBox()
        {
            var meds = GameObject.Find("Item_Remedio");
            if (meds == null)
                return;

            // Recolor from solid red to a cream/white medicine-box body with a red cross on top.
            var bodyMat = GetOrCreateMaterial(k_Cream, 0.15f, 0f);
            meds.GetComponent<Renderer>().sharedMaterial = bodyMat;

            var crossMat = GetOrCreateMaterial(k_MedsRed, 0.6f, 0f);
            AddChildPrimitive(meds.transform, "CrossBarA", PrimitiveType.Cube,
                worldOffset: new Vector3(0f, 0.034f, 0f), worldSize: new Vector3(0.07f, 0.008f, 0.018f), crossMat);
            AddChildPrimitive(meds.transform, "CrossBarB", PrimitiveType.Cube,
                worldOffset: new Vector3(0f, 0.034f, 0f), worldSize: new Vector3(0.018f, 0.008f, 0.05f), crossMat);
        }

        static void DetailDocuments()
        {
            var documents = GameObject.Find("Item_Documentos");
            if (documents == null)
                return;

            var folderMat = GetOrCreateMaterial(new Color(0.82f, 0.68f, 0.4f), 0.25f, 0f);
            AddChildPrimitive(documents.transform, "FolderCover", PrimitiveType.Cube,
                worldOffset: new Vector3(0.005f, 0.024f, 0f), worldSize: new Vector3(0.17f, 0.008f, 0.23f), folderMat);
        }

        static void DetailPhotoFrame()
        {
            var frame = GameObject.Find("Item_PortaRetrato");
            if (frame == null)
                return;

            var photoMat = GetOrCreateMaterial(k_Cream, 0.15f, 0f);
            AddChildPrimitive(frame.transform, "Photo", PrimitiveType.Cube,
                worldOffset: new Vector3(0f, 0f, 0.023f), worldSize: new Vector3(0.10f, 0.14f, 0.006f), photoMat);
        }

        static void DetailBackpack()
        {
            var backpack = GameObject.Find("Backpack");
            if (backpack == null)
                return;

            var pocketMat = GetOrCreateMaterial(new Color(0.22f, 0.24f, 0.1f), 0.12f, 0f);
            AddChildPrimitive(backpack.transform, "Pocket", PrimitiveType.Cube,
                worldOffset: new Vector3(0f, -0.05f, 0.155f), worldSize: new Vector3(0.22f, 0.18f, 0.06f), pocketMat);

            var strapMat = GetOrCreateMaterial(k_DarkPlastic, 0.4f, 0.2f);
            AddChildPrimitive(backpack.transform, "StrapLeft", PrimitiveType.Cube,
                worldOffset: new Vector3(-0.09f, 0.21f, 0f), worldSize: new Vector3(0.05f, 0.02f, 0.32f), strapMat);
            AddChildPrimitive(backpack.transform, "StrapRight", PrimitiveType.Cube,
                worldOffset: new Vector3(0.09f, 0.21f, 0f), worldSize: new Vector3(0.05f, 0.02f, 0.32f), strapMat);
        }

        static void DetailValveWheel()
        {
            var wheel = GameObject.Find("ValveWheel");
            if (wheel == null)
                return;

            var metalMat = GetOrCreateMaterial(k_MetalColor, 0.7f, 0.85f);

            // X/Z scale on the wheel handle are equal, so children built without rotation never
            // shear even though the parent's own scale is non-uniform (thin on Y).
            AddChildPrimitive(wheel.transform, "Hub", PrimitiveType.Cylinder,
                worldOffset: Vector3.zero, worldSize: new Vector3(0.05f, 0.07f, 0.05f), metalMat);
            AddChildPrimitive(wheel.transform, "SpokeA", PrimitiveType.Cube,
                worldOffset: Vector3.zero, worldSize: new Vector3(0.14f, 0.025f, 0.03f), metalMat);
            AddChildPrimitive(wheel.transform, "SpokeB", PrimitiveType.Cube,
                worldOffset: Vector3.zero, worldSize: new Vector3(0.03f, 0.025f, 0.14f), metalMat);
        }

        /// <summary>
        /// Real household breakers (e.g. a Tramontina-style MCB) use a single paddle that flips
        /// up/down inside a white DIN module, not a wheel-like dial. The knob still only rotates
        /// its handle around one axis, but that axis already sweeps through the vertical plane the
        /// player faces — a symmetric bar looks identical at 180° apart (the earlier "pinwheel"),
        /// while a single-sided paddle built mostly along local Z reads as standing upright at
        /// rest and tipping up or down at the knob's two snap angles, exactly like a real toggle.
        /// Only the look changes here — minAngle/maxAngle and the value/target logic are untouched.
        /// </summary>
        static void DetailBreakerAssembly()
        {
            var handle = GameObject.Find("BreakerHandle");
            if (handle != null)
            {
                // Superseded designs from earlier passes.
                RemoveChildIfExists(handle.transform, "Collar");
                RemoveChildIfExists(handle.transform, "Indicator");
                RemoveChildIfExists(handle.transform, "Lever");
                RemoveChildIfExists(handle.transform, "Tip");
                RemoveChildIfExists(handle.transform, "Paddle");

                var housingMat = GetOrCreateMaterial(k_SwitchHousing, 0.45f, 0.1f);
                handle.GetComponent<Renderer>().sharedMaterial = housingMat;

                var paddleMat = GetOrCreateMaterial(new Color(0.15f, 0.35f, 0.75f), 0.55f, 0.05f);
                AddChildPrimitive(handle.transform, "Paddle", PrimitiveType.Cube,
                    worldOffset: new Vector3(0f, 0.012f, 0.045f), worldSize: new Vector3(0.035f, 0.03f, 0.16f), paddleMat);

                // The scene's original -60/60 range suited the old horizontal bar; a vertical-rest
                // paddle needs a realistic toggle throw instead, or a 60 degree twist lays it
                // almost sideways. angleIncrement still equals the full span, so it stays a clean
                // 2-position snap — only the geometry driving m_Value's visual read changes here.
                var knobSo = new SerializedObject(handle.GetComponent<Unity.VRTemplate.XRKnob>());
                knobSo.FindProperty("m_MinAngle").floatValue = -25f;
                knobSo.FindProperty("m_MaxAngle").floatValue = 25f;
                knobSo.FindProperty("m_AngleIncrement").floatValue = 50f;
                knobSo.ApplyModifiedProperties();
            }

            var plate = GameObject.Find("BreakerPlate");
            if (plate != null)
            {
                var panelMat = GetOrCreateMaterial(k_PanelGrey, 0.35f, 0.15f);
                plate.GetComponent<Renderer>().sharedMaterial = panelMat;

                // White DIN-rail module body the paddle appears to pivot out of.
                var moduleMat = GetOrCreateMaterial(new Color(0.92f, 0.92f, 0.9f), 0.4f, 0f);
                AddChildPrimitive(plate.transform, "ModuleBody", PrimitiveType.Cube,
                    worldOffset: new Vector3(0f, 0f, 0.04f), worldSize: new Vector3(0.12f, 0.20f, 0.025f), moduleMat);

                var screwMat = GetOrCreateMaterial(k_MetalColor, 0.7f, 0.85f);
                AddChildPrimitive(plate.transform, "ScrewTL", PrimitiveType.Sphere,
                    worldOffset: new Vector3(-0.13f, 0.18f, 0.035f), worldSize: new Vector3(0.015f, 0.015f, 0.015f), screwMat);
                AddChildPrimitive(plate.transform, "ScrewTR", PrimitiveType.Sphere,
                    worldOffset: new Vector3(0.13f, 0.18f, 0.035f), worldSize: new Vector3(0.015f, 0.015f, 0.015f), screwMat);
                AddChildPrimitive(plate.transform, "ScrewBL", PrimitiveType.Sphere,
                    worldOffset: new Vector3(-0.13f, -0.18f, 0.035f), worldSize: new Vector3(0.015f, 0.015f, 0.015f), screwMat);
                AddChildPrimitive(plate.transform, "ScrewBR", PrimitiveType.Sphere,
                    worldOffset: new Vector3(0.13f, -0.18f, 0.035f), worldSize: new Vector3(0.015f, 0.015f, 0.015f), screwMat);

                // A little yellow/black hazard stripe near the bottom edge, like the warning
                // labels real distribution panels carry.
                var stripeMat = GetOrCreateMaterial(k_SafetyYellow, 0.55f, 0.15f);
                AddChildPrimitive(plate.transform, "WarningStripe", PrimitiveType.Cube,
                    worldOffset: new Vector3(0f, -0.17f, 0.035f), worldSize: new Vector3(0.22f, 0.035f, 0.008f), stripeMat);

                var blackMat = GetOrCreateMaterial(k_SwitchHousing, 0.45f, 0.1f);
                AddChildPrimitive(plate.transform, "WarningStripeMarkA", PrimitiveType.Cube,
                    worldOffset: new Vector3(-0.06f, -0.17f, 0.036f), worldSize: new Vector3(0.035f, 0.035f, 0.009f), blackMat);
                AddChildPrimitive(plate.transform, "WarningStripeMarkB", PrimitiveType.Cube,
                    worldOffset: new Vector3(0.06f, -0.17f, 0.036f), worldSize: new Vector3(0.035f, 0.035f, 0.009f), blackMat);
            }
        }

        /// <summary>
        /// Adds a thin tabletop trim just above the existing solid block, kept within the gap
        /// already left below the items resting on it so nothing clips.
        /// </summary>
        static void DetailTableTrim(string name, float topWorldY, float footprintX, float footprintZ, float safeClearance)
        {
            var table = GameObject.Find(name);
            if (table == null)
                return;

            var trimMat = GetOrCreateMaterial(new Color(0.62f, 0.44f, 0.26f), 0.5f, 0f);

            var thickness = safeClearance * 0.8f;
            var localCenterY = table.transform.position.y;
            var offsetY = (topWorldY + thickness * 0.5f) - localCenterY;

            AddChildPrimitive(table.transform, "TopTrim", PrimitiveType.Cube,
                worldOffset: new Vector3(0f, offsetY, 0f),
                worldSize: new Vector3(footprintX + 0.04f, thickness, footprintZ + 0.04f), trimMat);
        }

        static void DetailShelfBrackets()
        {
            var shelf = GameObject.Find("ShelfLedge");
            if (shelf == null)
                return;

            var bracketMat = GetOrCreateMaterial(k_MetalColor, 0.7f, 0.85f);
            AddChildPrimitive(shelf.transform, "BracketA", PrimitiveType.Cube,
                worldOffset: new Vector3(-0.02f, -0.065f, 0.35f), worldSize: new Vector3(0.06f, 0.08f, 0.06f), bracketMat);
            AddChildPrimitive(shelf.transform, "BracketB", PrimitiveType.Cube,
                worldOffset: new Vector3(-0.02f, -0.065f, -0.35f), worldSize: new Vector3(0.06f, 0.08f, 0.06f), bracketMat);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// Adds a visual-only (no collider) child primitive expressed entirely in world units, so
        /// callers never have to divide through the parent's (often non-uniform) local scale by
        /// hand. Skips creation if a child with this name already exists, so re-running is safe.
        /// Only rotation-free placements are supported: rotating a child under a non-uniformly
        /// scaled parent would shear it, so every call site keeps children axis-aligned.
        /// </summary>
        static GameObject AddChildPrimitive(Transform parent, string name, PrimitiveType type, Vector3 worldOffset, Vector3 worldSize, Material material)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);

            var scale = parent.lossyScale;
            go.transform.localPosition = new Vector3(worldOffset.x / scale.x, worldOffset.y / scale.y, worldOffset.z / scale.z);
            go.transform.localScale = new Vector3(worldSize.x / scale.x, worldSize.y / scale.y, worldSize.z / scale.z);

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        static void RemoveChildIfExists(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }

        static Material GetOrCreateMaterial(Color color, float smoothness, float metallic)
        {
            var path = $"{k_MaterialFolder}/Placeholder_{ColorUtility.ToHtmlStringRGB(color)}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { color = color };
                material.SetFloat("_Smoothness", smoothness);
                material.SetFloat("_Metallic", metallic);
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }
    }
}
#endif
