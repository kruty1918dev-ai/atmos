using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Atmos
{
    /// <summary>
    /// Applies the Atmos/FoliageSway shader to the renderers of a decor object:
    /// tips drift in the breeze while roots stay planted, with cheap baked
    /// half-lambert lighting. Materials are shared per base color so a whole
    /// meadow of swaying props costs only a handful of materials.
    /// Use <see cref="Shared"/> for app-wide deduplication, or construct an
    /// instance when you want an isolated cache you can clear yourself.
    /// </summary>
    public sealed class FoliageSway
    {
        public static readonly FoliageSway Shared = new FoliageSway();

        readonly Dictionary<Color, Material> _byColor = new Dictionary<Color, Material>();

        /// <summary>Shared sway material for a base color (created on demand).</summary>
        public Material MaterialFor(Color baseColor, float amplitude = 0.05f, float frequency = 1.6f)
        {
            if (!_byColor.TryGetValue(baseColor, out var mat) || mat == null)
            {
                mat = AtmosShaders.NewMaterial(AtmosShaders.FoliageSway, "FoliageSway");
                if (mat == null) return null;
                mat.SetColor("_BaseColor", baseColor);
                mat.SetFloat("_SwayAmp", amplitude);
                mat.SetFloat("_SwayFreq", frequency);
                _byColor[baseColor] = mat;
            }
            return mat;
        }

        /// <summary>
        /// Swaps every renderer under <paramref name="root"/> to the sway shader,
        /// preserving each renderer's _BaseColor/_Color. Returns swapped count.
        /// </summary>
        public int Apply(GameObject root, float amplitude = 0.05f, float frequency = 1.6f)
        {
            if (root == null) return 0;
            var swapped = 0;
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                var src = r.sharedMaterial;
                var c = src != null
                    ? (src.HasProperty("_BaseColor") ? src.GetColor("_BaseColor")
                       : src.HasProperty("_Color") ? src.GetColor("_Color")
                       : Color.white)
                    : Color.white;
                var mat = MaterialFor(c, amplitude, frequency);
                if (mat == null) break;
                r.sharedMaterial = mat;
                swapped++;
            }
            return swapped;
        }

        /// <summary>Drops cached materials (they remain assigned; new ones are created).</summary>
        public void Clear() => _byColor.Clear();
    }
}
