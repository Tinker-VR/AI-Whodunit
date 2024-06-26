using System.Collections.Generic;
using System.Reflection;
using Service;
using UnityEngine;

namespace Convai.Scripts.Utils.LipSync.Types
{
    public class ConvaiOVRLipsync : ConvaiVisemesLipSync
    {
        int _firstIndex;

        public override void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            base.Initialize(convaiLipSync, convaiNPC);
            _firstIndex = convaiLipSync.firstIndex;
        }

        protected override Dictionary<string, string> GetHeadRegexMapping()
        {
            string mouth = "[Mm]outh";
            string spacer = "[\\s_]*";
            string left = "[Ll]eft";
            string right = "[Rr]ight";
            string lower = "[Ll]ower";
            string upper = "[Uu]pper";
            string open = "[Oo]pen";
            string funnel = "[Ff]unnel";
            string pucker = "[Pp]ucker";
            string prefix = "(?:[A-Z]\\d{1,2}_)?";

            return new Dictionary<string, string>()
            {
                {"PP", $"{prefix}{mouth}{spacer}{pucker}"},
                {"FF", $"{prefix}{mouth}{spacer}{funnel}"},
                {"THL", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{left}"},
                {"THR", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{right}"},
                {"DDL", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{left}"},
                {"DDR", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{right}"},
                {"KK", $"{prefix}[Jj]aw{spacer}{open}"},
                {"CHL",$"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{left}"},
                {"CHR",$"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{right}"},
                {"SSL", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{left}"},
                {"SSR", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{right}"},
                {"NNL", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{left}"},
                {"NNR", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{right}"},
                {"RRU",$"{prefix}{mouth}{spacer}[Rr]oll{spacer}{upper}"},
                {"RRL", $"{prefix}{mouth}{spacer}[Rr]oll{spacer}{lower}"},
                {"AA", $"{prefix}[Jj]aw{spacer}[Oo]pen"},
                {"EL", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{left}"},
                {"ER", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{right}"},
                {"IHL", $"{prefix}{mouth}{spacer}[Ff]rown{spacer}{left}"},
                {"IHR",$"{prefix}{mouth}{spacer}[Ff]rown{spacer}{right}"},
                {"OU", $"{prefix}{mouth}{spacer}{pucker}"},
                {"OH", $"{prefix}{mouth}{spacer}{funnel}"},
            };
        }
        private void Update()
        {
            // Check if the dequeued frame is not null.
            if (_currentViseme == null) return;
            // Check if the frame represents silence (-2 is a placeholder for silence).
            if (_currentViseme.Sil == -2) return;

            float weight;
            float alpha = 1.0f;
            float weightMultiplier = 100f;
            List<int> knownIndexs = new List<int>();
            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, -5.0f));
            if (HasHeadSkinnedMeshRenderer)
            {
                foreach (PropertyInfo propertyInfo in typeof(Viseme).GetProperties())
                {
                    if (propertyInfo.PropertyType != typeof(float)) continue;
                    string fieldName = propertyInfo.Name.ToUpper();
                    float value = (float)propertyInfo.GetValue(_currentViseme);
                    weight = fieldName switch
                    {
                        "KK" => 1.0f / 1.5f,
                        "DD" => 1.0f / 0.7f,
                        "CH" => 1.0f / 2.7f,
                        "SS" => 1.0f / 1.5f,
                        "NN" => 1.0f / 2.0f,
                        "RR" => 1.0f / 0.9f,
                        "AA" => 1.0f / 2.0f,
                        "II" => 1.0f / 1.2f,
                        "OH" => 1.2f,
                        _ => 1.0f
                    };
                    foreach (string s in _possibleCombinations)
                    {
                        float weightThisFrame = value * weight * alpha * weightMultiplier;
                        string modifiedFieldName = fieldName + s;
                        FindAndUpdateBlendWeight(_headSkinMeshRenderer, modifiedFieldName, weightThisFrame, knownIndexs, _headMapping);
                    }
                }
            }

            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f - (
                        0.2f * _currentViseme.Th
                        + 0.1f * _currentViseme.Dd
                        + 0.5f * _currentViseme.Kk
                        + 0.2f * _currentViseme.Nn
                        + 0.2f * _currentViseme.Rr
                        + 1.0f * _currentViseme.Aa
                        + 0.2f * _currentViseme.E
                        + 0.3f * _currentViseme.Ih
                        + 0.8f * _currentViseme.Oh
                        + 0.3f * _currentViseme.Ou
                    )
                    / (0.2f + 0.1f + 0.5f + 0.2f + 0.2f + 1.0f + 0.2f + 0.3f + 0.8f + 0.3f)
                    * 30f));

            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, (
                        0.1f * _currentViseme.Th
                        + 0.2f * _currentViseme.Nn
                        + 0.15f * _currentViseme.Rr
                    )
                    / (0.1f + 0.2f + 0.15f)
                    * 80f - 5f));

            // Similar settings for _teethSkinMeshRenderer.
            if (_teethSkinMeshRenderer.sharedMesh.blendShapeCount < (_firstIndex + 15)) return;
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(0 + _firstIndex, _currentViseme.Sil * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(1 + _firstIndex, _currentViseme.Pp * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(2 + _firstIndex, _currentViseme.Ff * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(3 + _firstIndex, _currentViseme.Th * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(4 + _firstIndex, _currentViseme.Dd * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(5 + _firstIndex, _currentViseme.Kk * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(6 + _firstIndex, _currentViseme.Ch * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(7 + _firstIndex, _currentViseme.Ss * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(8 + _firstIndex, _currentViseme.Nn * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(9 + _firstIndex, _currentViseme.Rr * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(10 + _firstIndex, _currentViseme.Aa * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(11 + _firstIndex, _currentViseme.E * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(12 + _firstIndex, _currentViseme.Ih * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(13 + _firstIndex, _currentViseme.Oh * weightMultiplier, Time.deltaTime);
            _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(14 + _firstIndex, _currentViseme.Ou * weightMultiplier, Time.deltaTime);
        }
    }
}

