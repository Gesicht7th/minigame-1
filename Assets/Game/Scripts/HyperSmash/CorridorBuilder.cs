// Assets/_Game/Scripts/HyperSmash/CorridorBuilder.cs

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    /// <summary>
    /// Membangun koridor tunnel dari Unity primitives (Plane dan Cube).
    /// Versi "Smash Hit Style" - Material mengkilap seperti kaca/metalik halus.
    /// </summary>
    public class CorridorBuilder : MonoBehaviour
    {
        [Header("── Corridor Settings ───────────────────────")]
        [SerializeField] private float corridorLength = 500f;   // Total panjang koridor
        [SerializeField] private float corridorWidth = 20f;     // Lebar ideal
        [SerializeField] private float corridorHeight = 15f;    // Tinggi ideal
        [SerializeField] private int segmentCount = 100;        // Segmen lebih banyak = lebih mulus

        [Header("── Colors (Smash Hit Style) ────────────────")]
        // Warna bawaan diubah menjadi nuansa kaca / es (Ice Blue / Cyan)
        [SerializeField] private Color floorColor = new Color(0.85f, 0.95f, 1.0f);
        [SerializeField] private Color wallColor = new Color(0.75f, 0.90f, 1.0f);
        [SerializeField] private Color ceilingColor = new Color(0.65f, 0.85f, 1.0f);
        [SerializeField] private Color accentColor = new Color(0.0f, 0.8f, 1.0f);   // Cyan terang

        [Header("── Rune Lights ─────────────────────────────")]
        [SerializeField] private int runeLightCount = 15;
        [SerializeField] private Color runeLightColor = new Color(0.0f, 0.8f, 1.0f); // Cyan light

        private GameObject corridorRoot;

        #region Unity Lifecycle
        // INI YANG SEBELUMNYA HILANG!
        // Memastikan lorong otomatis dibangun saat game di-Play
        void Start()
        {
            BuildCorridor();
        }
        #endregion

        public void BuildCorridor()
        {
            // Bersihkan koridor lama jika ada
            if (corridorRoot != null) Destroy(corridorRoot);

            corridorRoot = new GameObject("Corridor_Geometry");
            corridorRoot.transform.SetParent(this.transform);

            float segmentLength = corridorLength / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                float zCenter = i * segmentLength + segmentLength * 0.5f;
                BuildSegment(zCenter, segmentLength);
            }

            // Tambahkan lampu sepanjang koridor
            AddRuneLights();

            Debug.Log($"[Corridor] Berhasil membangun {segmentCount} segmen bergaya kaca!");
        }

        private void BuildSegment(float zCenter, float length)
        {
            // LANTAI
            CreatePanel(
                parent: corridorRoot.transform,
                position: new Vector3(0f, -corridorHeight * 0.5f, zCenter),
                scale: new Vector3(corridorWidth, 1f, length),
                rotation: Quaternion.identity,
                color: floorColor,
                name: $"Floor_{zCenter}"
            );

            // LANGIT-LANGIT
            CreatePanel(
                parent: corridorRoot.transform,
                position: new Vector3(0f, corridorHeight * 0.5f, zCenter),
                scale: new Vector3(corridorWidth, 1f, length),
                rotation: Quaternion.Euler(180f, 0f, 0f),
                color: ceilingColor,
                name: $"Ceiling_{zCenter}"
            );

            // DINDING KIRI
            CreatePanel(
                parent: corridorRoot.transform,
                position: new Vector3(-corridorWidth * 0.5f, 0f, zCenter),
                scale: new Vector3(corridorHeight, 1f, length), // <--- INI YANG DIPERBAIKI
                rotation: Quaternion.Euler(0f, 0f, -90f),
                color: wallColor,
                name: $"WallLeft_{zCenter}"
            );

            // DINDING KANAN
            CreatePanel(
                parent: corridorRoot.transform,
                position: new Vector3(corridorWidth * 0.5f, 0f, zCenter),
                scale: new Vector3(corridorHeight, 1f, length), // <--- INI YANG DIPERBAIKI
                rotation: Quaternion.Euler(0f, 0f, 90f),
                color: wallColor,
                name: $"WallRight_{zCenter}"
            );

            // ACCENT STRIP — Garis neon di dinding bawah
            if (zCenter % (corridorLength / 10) < 2f)
            {
                CreateAccentStrip(zCenter, length);
            }
        }

        private void CreatePanel(Transform parent, Vector3 position, Vector3 scale,
                                  Quaternion rotation, Color color, string name)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Plane);
            panel.name = name;
            panel.transform.SetParent(parent);
            panel.transform.position = position;
            panel.transform.localScale = scale / 10f; // Plane default = 10 units
            panel.transform.rotation = rotation;

            Destroy(panel.GetComponent<Collider>()); // Hapus collider agar peluru tidak nyangkut

            // BIKIN MATERIAL ALA SMASH HIT (KACA/GLOSSY)
            Renderer rend = panel.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;

            // Mengatur pantulan cahaya (Smoothness/Glossiness) agar mengkilap
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f); // Untuk URP
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.85f); // Untuk Standard
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.2f);      // Sedikit metalik

            rend.material = mat;
        }

        private void CreateAccentStrip(float zCenter, float length)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = $"AccentStrip_{zCenter}";
            strip.transform.SetParent(corridorRoot.transform);
            strip.transform.position = new Vector3(0f, -corridorHeight * 0.5f + 0.1f, zCenter);
            strip.transform.localScale = new Vector3(0.5f, 0.05f, length);

            Destroy(strip.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = accentColor;

            // Bikin garisnya menyala (Glowing)
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", accentColor * 2.5f);
            }
            strip.GetComponent<Renderer>().material = mat;
        }

        private void AddRuneLights()
        {
            float spacing = corridorLength / runeLightCount;
            for (int i = 0; i < runeLightCount; i++)
            {
                float z = i * spacing + spacing * 0.5f;
                float xPos = i % 2 == 0 ? -corridorWidth * 0.45f : corridorWidth * 0.45f;

                GameObject lightObj = new GameObject($"NeonLight_{i}");
                lightObj.transform.SetParent(corridorRoot.transform);
                lightObj.transform.position = new Vector3(xPos, corridorHeight * 0.45f, z); // Pindah ke atas

                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = runeLightColor;
                light.intensity = 3f; // Diterangkan
                light.range = 25f;    // Jangkauan cahaya diperlebar agar lorongnya terang
            }
        }
    }
}