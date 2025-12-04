using System.Collections.Generic;
using NiceHouse.Data;
using UnityEngine;

namespace NiceHouse.LayeredVisualization
{
    /// <summary>
    /// 在整个户型范围内生成一张连续的地面热力图网格。
    /// 每个小格子根据其所在房间的环境数据着色，看起来是一整块连续的热力图。
    /// </summary>
    public class FloorHeatmapGrid : MonoBehaviour
    {
        public enum MetricType
        {
            Temperature,
            Humidity,
            Pm25
        }

        [Header("网格设置")]
        [Tooltip("单个格子的边长（米），越小越精细，但格子数越多。")]
        public float cellSize = 0.4f;

        [Tooltip("地面上抬起的高度，避免与模型 Z-fighting。")]
        public float heightOffset = 0.03f;

        [Header("可视化")]
        public Material heatmapMaterial;

        [Tooltip("要展示的数据类型")]
        public MetricType metric = MetricType.Temperature;

        [Tooltip("当没有环境数据时的默认值，用于归一化。")]
        public float defaultValue = 24f;

        [Tooltip("颜色映射的最小/最大值（单位取决于数据类型）。")]
        public float minValue = 16f;
        public float maxValue = 30f;

        [Tooltip("颜色渐变定义，用于热力渲染与图例")]
        public Gradient colorGradient;

        [Header("动态更新")]
        [Tooltip("热力图刷新时间间隔（秒）")]
        public float updateInterval = 1f;

        private readonly List<RoomDefinition> _rooms = new List<RoomDefinition>();

        private readonly List<CellInfo> _cells = new List<CellInfo>();
        private float _timer;

        private class CellInfo
        {
            public Vector3 position;
            public MeshRenderer renderer;
        }

        private void Start()
        {
            if (heatmapMaterial == null)
            {
                Debug.LogWarning("[FloorHeatmapGrid] 未指定 heatmapMaterial，不会生成网格。");
                return;
            }

            _rooms.AddRange(FindObjectsOfType<RoomDefinition>());
            if (_rooms.Count == 0)
            {
                Debug.LogWarning("[FloorHeatmapGrid] 场景中没有 RoomDefinition。");
                return;
            }

            // 等一小会儿，让 EnvironmentDataSimulator 先跑一帧，填充 EnvironmentDataStore
            Invoke(nameof(GenerateGrid), 0.2f);
        }

        private void Update()
        {
            if (_cells.Count == 0) return;

            _timer += Time.deltaTime;
            if (_timer < updateInterval) return;
            _timer = 0f;

            UpdateColors();
        }

        private void OnValidate()
        {
            ApplyPresetForMetric();

            if (colorGradient == null)
            {
                colorGradient = CreateDefaultGradient();
            }
        }

        private void GenerateGrid()
        {
            // 1. 计算整个户型的总包围盒
            bool hasBounds = false;
            Bounds total = new Bounds();
            foreach (var room in _rooms)
            {
                Bounds b = room.GetBounds();
                if (!hasBounds)
                {
                    total = b;
                    hasBounds = true;
                }
                else
                {
                    total.Encapsulate(b);
                }
            }

            if (!hasBounds)
            {
                Debug.LogWarning("[FloorHeatmapGrid] 无法计算总 bounds。");
                return;
            }

            float minX = total.min.x;
            float maxX = total.max.x;
            float minZ = total.min.z;
            float maxZ = total.max.z;
            float y = total.min.y + heightOffset;

            int countX = Mathf.CeilToInt((maxX - minX) / cellSize);
            int countZ = Mathf.CeilToInt((maxZ - minZ) / cellSize);

            // 2. 初始生成格子（只要在总 bounds 内就生成，不再按房间硬切）
            for (int ix = 0; ix < countX; ix++)
            {
                for (int iz = 0; iz < countZ; iz++)
                {
                    Vector3 center = new Vector3(
                        minX + (ix + 0.5f) * cellSize,
                        y,
                        minZ + (iz + 0.5f) * cellSize);

                    CreateCell(center);
                }
            }

            // 3. 立即刷新一次颜色
            UpdateColors();
        }

        public string GetMetricUnit()
        {
            return metric switch
            {
                MetricType.Temperature => "℃",
                MetricType.Humidity => "%",
                MetricType.Pm25 => "μg/m³",
                _ => string.Empty
            };
        }

        private void UpdateColors()
        {
            if (EnvironmentDataStore.Instance == null) return;

            // 为每个房间预取当前指标值和中心位置
            var roomValue = new Dictionary<RoomDefinition, float>();
            var roomCenters = new Dictionary<RoomDefinition, Vector3>();
            foreach (var room in _rooms)
            {
                float value = defaultValue;
                if (EnvironmentDataStore.Instance.TryGetRoomData(room.roomId, out var data))
                {
                    value = metric switch
                    {
                        MetricType.Temperature => data.temperature,
                        MetricType.Humidity => data.humidity,
                        MetricType.Pm25 => data.pm25,
                        _ => defaultValue
                    };
                }

                roomValue[room] = value;
                roomCenters[room] = room.GetBounds().center;
            }

            foreach (var cell in _cells)
            {
                if (cell.renderer == null) continue;

                // 使用所有房间中心做距离加权平均，让整个户型颜色连续过渡
                float value = SampleValueAtPosition(cell.position, roomValue, roomCenters);
                Color color = EvaluateColor(value);
                cell.renderer.sharedMaterial.color = color;
            }
        }

        private float SampleValueAtPosition(
            Vector3 position,
            IReadOnlyDictionary<RoomDefinition, float> roomValue,
            IReadOnlyDictionary<RoomDefinition, Vector3> roomCenters)
        {
            float weightedSum = 0f;
            float weightSum = 0f;

            foreach (var kv in roomValue)
            {
                var room = kv.Key;
                float value = kv.Value;
                Vector3 center = roomCenters[room];

                // 只考虑水平距离
                Vector2 p = new Vector2(position.x, position.z);
                Vector2 c = new Vector2(center.x, center.z);
                float dist = Vector2.Distance(p, c);

                // 距离越近权重越大，避免除零
                float w = 1f / Mathf.Max(dist * dist, 0.01f);
                weightedSum += value * w;
                weightSum += w;
            }

            if (weightSum <= 0.0001f) return defaultValue;
            return weightedSum / weightSum;
        }

        private Color EvaluateColor(float value)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            t = Mathf.Clamp01(t);

            if (colorGradient != null)
            {
                return colorGradient.Evaluate(t);
            }

            return Color.Lerp(Color.blue, Color.red, t);
        }

        private void CreateCell(Vector3 center)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cell.name = "HeatCell";
            // 继承父对象的 Layer，方便通过相机 Culling Mask 控制显示
            cell.layer = gameObject.layer;
            cell.transform.SetParent(transform, worldPositionStays: true);

            cell.transform.position = center;
            cell.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);

            var renderer = cell.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(heatmapMaterial);

            _cells.Add(new CellInfo
            {
                position = center,
                renderer = renderer
            });

            Destroy(cell.GetComponent<Collider>());
        }

        private static Gradient CreateDefaultGradient()
        {
            return CreateGradient(
                ("#003CFF", 0f),
                ("#00F0FF", 0.33f),
                ("#FFC800", 0.66f),
                ("#FF3300", 1f));
        }

        private void ApplyPresetForMetric()
        {
            switch (metric)
            {
                case MetricType.Temperature:
                    defaultValue = 24f;
                    minValue = 22f;
                    maxValue = 28f;
                    colorGradient = CreateGradient(
                        ("#003CFF", 0f),
                        ("#00F0FF", 0.33f),
                        ("#FFC800", 0.66f),
                        ("#FF3300", 1f));
                    break;
                case MetricType.Humidity:
                    defaultValue = 50f;
                    minValue = 30f;
                    maxValue = 70f;
                    colorGradient = CreateGradient(
                        ("#F5F5F5", 0f),
                        ("#00A0FF", 0.5f),
                        ("#006600", 1f));
                    break;
                case MetricType.Pm25:
                    defaultValue = 35f;
                    minValue = 0f;
                    maxValue = 120f;
                    colorGradient = CreateGradient(
                        ("#2FFF00", 0f),
                        ("#FFD800", 0.4f),
                        ("#FF7A00", 0.7f),
                        ("#FF1A1A", 1f));
                    break;
            }
        }

        private static Gradient CreateGradient(params (string hex, float time)[] keys)
        {
            var gradient = new Gradient();
            var colorKeys = new GradientColorKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (ColorUtility.TryParseHtmlString(keys[i].hex, out var color))
                {
                    colorKeys[i] = new GradientColorKey(color, keys[i].time);
                }
                else
                {
                    colorKeys[i] = new GradientColorKey(Color.white, keys[i].time);
                }
            }

            gradient.SetKeys(
                colorKeys,
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });

            return gradient;
        }
    }
}


