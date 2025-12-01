using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using UnityEngine;

namespace SealedSustenance.Components
{
    public class LedProgressScreenComponent : MonoBehaviour
    {
        [Header("OnOff")]
        public Material offMaterial;
        public Material onMaterial;

        [Header("LEDs")]
        public MeshRenderer[] ledRenderers = {};
    
        private int _currentLevel = 0;

        public void RefreshState(float num, bool powered)
        {
            if (!powered)
            {
                for (int i = 0; i < ledRenderers.Length; i++)
                {
                    MeshRenderer meshRenderer = ledRenderers[i];
                    meshRenderer.material = offMaterial;
                }
            }
            else
            {
                var levels = ledRenderers.Length;
                _currentLevel = Mathf.RoundToInt(Mathf.Clamp(num * levels, 0, levels));
                _currentLevel = (_currentLevel == 0 && num > 0) ? 1 : _currentLevel;

                for (int i = 0; i < ledRenderers.Length; i++)
                {
                    MeshRenderer meshRenderer = ledRenderers[i];
                    if (i < _currentLevel)
                        meshRenderer.material = onMaterial;
                    else
                        meshRenderer.material = offMaterial;
                }
            }
        }
    }
}
