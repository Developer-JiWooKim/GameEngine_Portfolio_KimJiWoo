using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MonsterFieldOfView : MonoBehaviour
{
    [Header("Mesh Quality")]
    [Tooltip("Ray 개수가 많을수록 부드럽지만 연산 비용 증가")]
    [SerializeField] private int _rayCount = 90;

    [Header("Rendering")]
    [SerializeField] private float    _meshHeight = 0.1f;
    [SerializeField] private Material _fovMaterial;

    private float _detectionRange = 0f;
    private float _fieldOfView    = 0f;

    private Mesh       _mesh;
    private MeshFilter _meshFilter;
    private int        _wallLayerMask;

    private Vector3[] _vertices;
    private int[]     _triangles;

    private void Awake() => Initialize();
    /// <summary>
    /// 초기화 메소드
    /// </summary>
    private void Initialize()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _wallLayerMask = LayerMask.GetMask("Wall");

        _mesh = new Mesh { name = "FieldOfViewMesh" };
        _meshFilter.mesh = _mesh;

        _vertices  = new Vector3[_rayCount + 2];

        _triangles = new int[_rayCount * 3];
        for (int i = 0; i < _rayCount; i++)
        {
            _triangles[i * 3] = 0;
            _triangles[i * 3 + 1] = i + 1;
            _triangles[i * 3 + 2] = i + 2;
        }

        GetComponent<MeshRenderer>().material = _fovMaterial;

        MonsterSight sight = GetComponentInParent<MonsterSight>();

        if (sight == null) return;

        _detectionRange = sight.DetectionRange;
        _fieldOfView    = sight.FieldOfView;
    }

    /// <summary>
    /// 매 프레임 시야각 메시를 갱신하는 메소드
    /// </summary>
    public void DrawFieldOfView(Transform monsterTransform)
    {
        float angleStep   = _fieldOfView / _rayCount;
        float startAngle  = -_fieldOfView * 0.5f;
        float originAngle = monsterTransform.eulerAngles.y;

        Vector3 origin = monsterTransform.position;
        origin.y = _meshHeight;

        _vertices[0] = Vector3.zero;

        for (int i = 0; i <= _rayCount; i++)
        {
            float angle = originAngle + startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

            Vector3 endPoint;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _detectionRange, _wallLayerMask))
            {
                endPoint = hit.point;
            }
            else
            {
                endPoint = origin + direction * _detectionRange;
            }

            endPoint.y = _meshHeight;

            // monsterTransform 기준으로 로컬 변환(이 스크립트가 붙어있는 FieldOfView가 몬스터 프리팹의 자식으로 붙어있기 때문)
            _vertices[i + 1]   = monsterTransform.InverseTransformPoint(endPoint);
            _vertices[i + 1].y = 0f;
        }

        _mesh.Clear();
        _mesh.vertices  = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
    }
}
