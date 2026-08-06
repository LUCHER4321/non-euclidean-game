using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WobbleEffect : MonoBehaviour
{
    [SerializeField] LiquidSO liquidSO;

    private Renderer renderer;
    private Material wobbleMaterial;
    private Vector3 lastPosition;
    private Vector2 currentWobble;
    private Vector2 targetWobble;
    private readonly int wobblePropertyId = Shader.PropertyToID("_Wobble");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<Renderer>();
        wobbleMaterial = renderer.material;
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        targetWobble.x = Mathf.Clamp(velocity.z * 0.01f, -liquidSO.GetMaxWobble, liquidSO.GetMaxWobble);
        targetWobble.y = Mathf.Clamp(-velocity.x * 0.01f, -liquidSO.GetMaxWobble, liquidSO.GetMaxWobble);
        currentWobble = Vector2.Lerp(currentWobble, targetWobble, Time.deltaTime * liquidSO.GetWobbleSpeed);
        targetWobble = Vector2.Lerp(targetWobble, Vector2.zero, Time.deltaTime * liquidSO.GetRecoverySpeed);
        wobbleMaterial.SetVector(wobblePropertyId, currentWobble);
        lastPosition = transform.position;
    }
}
