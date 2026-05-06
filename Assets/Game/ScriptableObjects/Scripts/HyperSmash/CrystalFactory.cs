// Assets/_Game/Scripts/HyperSmash/CrystalFactory.cs
// Membuat kristal secara prosedural dari Unity primitives
// Tidak butuh prefab atau asset apapun

using UnityEngine;

namespace WizardPunk.HyperSmash
{
    public static class CrystalFactory
    {
        // Warna per tipe
        private static readonly Color ColorBlue = new Color(0.1f, 0.4f, 1.0f, 0.85f);
        private static readonly Color ColorPurple = new Color(0.6f, 0.1f, 1.0f, 0.85f);
        private static readonly Color ColorRed = new Color(1.0f, 0.1f, 0.15f, 0.85f);
        private static readonly Color ColorBomb = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        /// <summary>
        /// Buat GameObject kristal dari primitive.
        /// Tidak perlu prefab — langsung bisa dipakai saat testing.
        /// </summary>
        public static GameObject CreateCrystal(CrystalType type, Transform parent = null)
        {
            // Root object
            GameObject root = new GameObject($"Crystal_{type}");
            if (parent != null) root.transform.SetParent(parent);

            // Visual — kristal = rotated cube, bomb = sphere
            PrimitiveType primitiveType = type == CrystalType.Bomb
                ? PrimitiveType.Sphere
                : PrimitiveType.Cube;

            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform);
            visual.transform.localPosition = Vector3.zero;

            // Rotate sedikit agar terlihat seperti kristal (diamond shape)
            if (type != CrystalType.Bomb)
                visual.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);

            // Remove default collider dari visual (collider ada di root)
            Object.Destroy(visual.GetComponent<Collider>());

            // Setup material dengan URP Lit
            Renderer rend = visual.GetComponent<Renderer>();
            Material mat = CreateCrystalMaterial(type);
            rend.material = mat;

            // Collider di root (lebih mudah detect)
            BoxCollider col = type == CrystalType.Bomb
                ? null
                : root.AddComponent<BoxCollider>();

            if (type == CrystalType.Bomb)
                root.AddComponent<SphereCollider>();
            else if (col != null)
                col.size = Vector3.one * 1.1f; // sedikit lebih besar dari visual

            root.layer = LayerMask.NameToLayer("Crystal"); // buat layer ini dulu!

            // Tambah Crystal component
            Crystal crystal = root.AddComponent<Crystal>();

            // Tambah simple hit particles (particle system dari code)
            ParticleSystem hitPS = CreateHitParticles(root.transform, GetColor(type));
            crystal.GetType()
                .GetField("hitParticles",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(crystal, hitPS);

            // Assign renderer ke crystal
            crystal.GetType()
                .GetField("crystalRenderer",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(crystal, rend);

            // Init type
            crystal.Initialize(type);

            return root;
        }

        private static Material CreateCrystalMaterial(CrystalType type)
        {
            // Cari shader URP Lit
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
                urpLit = Shader.Find("Standard"); // Fallback ke Standard shader

            Material mat = new Material(urpLit);
            Color baseColor = GetColor(type);
            mat.color = baseColor;

            // Emissive agar terlihat glowing
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", baseColor * 0.5f);
            }

            // Sedikit metallic/smooth untuk tampilan kristal
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", 0.3f);
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", 0.8f);

            return mat;
        }

        private static Color GetColor(CrystalType type)
        {
            return type switch
            {
                CrystalType.Blue => ColorBlue,
                CrystalType.Purple => ColorPurple,
                CrystalType.Red => ColorRed,
                CrystalType.Bomb => ColorBomb,
                _ => Color.white
            };
        }

        private static ParticleSystem CreateHitParticles(Transform parent, Color color)
        {
            GameObject psGO = new GameObject("HitParticles");
            psGO.transform.SetParent(parent);
            psGO.transform.localPosition = Vector3.zero;

            ParticleSystem ps = psGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 3f;
            main.startSize = 0.1f;
            main.startColor = color;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 10)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            return ps;
        }
    }
}