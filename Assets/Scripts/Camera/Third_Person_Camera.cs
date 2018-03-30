using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Third_Person_Camera : MonoBehaviour
{
    public class CameraOrientedPlane
    {
        public CameraOrientedPlane(GameObject obj)
        {
            Object = obj;
            Rotation = obj.transform.rotation;
            UpVector = obj.transform.up;
        }

        public GameObject Object;
        public Quaternion Rotation;
        public Vector3 UpVector;
    }
    struct CameraParameters
    {
        public CameraParameters(float distBehind, float distAbove, float tarDistAhead, float tarDistAbove, float sideAng)
        {
            DistanceBehind = distBehind;
            DistanceAbove = distAbove;
            TargetDistanceAhead = tarDistAhead;
            TargetDistanceAbove = tarDistAbove;
            AngleSide = sideAng;
        }

        public float DistanceBehind;
        public float DistanceAbove;
        public float TargetDistanceAhead;
        public float TargetDistanceAbove;
        public float AngleSide;
    }

    CameraParameters StartPos;
    CameraParameters MaxUp;
    CameraParameters MaxDown;
    CameraParameters AgainstWall;

    public GameObject Target;

    public List<CameraOrientedPlane> Planes;

    //Starting Position
    [Header("Starting Position")]
    [Range(0.0f, 10.0f)]
    public float Distance_Behind;
    [Range(0.0f, 10.0f)]
    public float Distance_Above;
    [Range(0.0f, 20.0f)]
    public float Target_Distance_Ahead;
    [Range(-1.0f, 2.0f)]
    public float Target_Distance_Above;
    [Range(-30.0f, 30.0f)]
    public float Angle_Side;
    public bool Test_Start_Pos;
    [Space()]

    //Looking up max angle
    [Header("Maximum Upwards Direction")]
    [Range(0.0f, 10.0f)]
    public float Max_Up_Distance_Behind;
    [Range(-5.0f, 5.0f)]
    public float Max_Up_Distance_Above;
    [Range(0.0f, 20.0f)]
    public float Max_Up_Target_Distance_Ahead;
    [Range(0.0f, 20.0f)]
    public float Max_Up_Target_Distance_Above;
    [Range(-30.0f, 30.0f)]
    public float Max_Up_Angle_Side;
    public bool Test_Max_Up;
    [Space()]

    //Looking down max angle
    [Header("Maximum Downwards Direction")]
    [Range(0.0f, 10.0f)]
    public float Max_Down_Distance_Behind;
    [Range(0.0f, 15.0f)]
    public float Max_Down_Distance_Above;
    [Range(0.0f, 20.0f)]
    public float Max_Down_Target_Distance_Ahead;
    [Range(0.0f, -20.0f)]
    public float Max_Down_Target_Distance_Below;
    [Range(-30.0f, 30.0f)]
    public float Max_Down_Angle_Side;
    public bool Test_Max_Down;
    [Space()]

    //When the camera would go through an outer wall
    [Header("Camera Through Wall")]
    [Range(5.0f, -5.0f)]
    public float Wall_Distance_Ahead;
    [Range(0.0f, 15.0f)]
    public float Wall_Distance_Above;
    [Range(0.0f, 10.0f)]
    public float Wall_Target_Distance_Ahead;
    [Range(-5.0f, 5.0f)]
    public float Wall_Target_Distance_Above;
    [Range(-30.0f, 30.0f)]
    public float Wall_Angle_Side;
    public bool Test_Wall;
    [Space()]

    [Range(-1.0f, 1.0f)]
    public float VerticalRotation;

    public LayerMask OpaqueLayer;
    public LayerMask TransparentLayer;

    public GameObject Crosshair;
    public GameObject OuterCrosshair;

    public float ReticleScaleFactor = 0.005f;
    public float ReticleColorFactor = 0.05f;

    public float ReticleScaleGravity = 1;
    public float ReticleColorGravity = 1;

    public Vector3 m_LookTarget { get; private set; }

    //GameObject m_Shadowcaster;

    // Use this for initialization
    void Start()
    {

        m_LookTarget = Vector3.zero;

        SetupStructs();

        TransformCamera(StartPos, StartPos, 0);

        Planes = new List<CameraOrientedPlane>();

        //GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    void OnValidate()
    {
        SetupStructs();
    }

    void SetupStructs()
    {
        StartPos = new CameraParameters(Distance_Behind, Distance_Above, Target_Distance_Ahead, Target_Distance_Above, Angle_Side);
        MaxUp = new CameraParameters(Max_Up_Distance_Behind, Max_Up_Distance_Above, Max_Up_Target_Distance_Ahead, Max_Up_Target_Distance_Above, Max_Up_Angle_Side);
        MaxDown = new CameraParameters(Max_Down_Distance_Behind, Max_Down_Distance_Above, Max_Down_Target_Distance_Ahead, Max_Down_Target_Distance_Below, Max_Down_Angle_Side);
        AgainstWall = new CameraParameters(Wall_Distance_Ahead, Wall_Distance_Above, Wall_Target_Distance_Ahead, Wall_Target_Distance_Above, Wall_Angle_Side);
    }

    public void AddReticleDamage(int damage)
    {
        //always make the reticle jump a bit
        damage += 10;

        float newScale = OuterCrosshair.transform.localScale.x + damage * ReticleScaleFactor;
        newScale = Mathf.Clamp(newScale, 0.1f, 0.27f);
        OuterCrosshair.transform.localScale = new Vector3(newScale, newScale, newScale);
        Material mat = OuterCrosshair.GetComponent<Renderer>().material;
        float val = mat.GetFloat("_AccentVal");
        mat.SetFloat("_AccentVal", Mathf.Clamp01(val + damage * ReticleColorFactor));
    }

    private void Update()
    {
        float newScale = Mathf.Max(0.1f, OuterCrosshair.transform.localScale.x - ReticleScaleGravity * Time.deltaTime);
        OuterCrosshair.transform.localScale = new Vector3(newScale, newScale, newScale);

        Material mat = OuterCrosshair.GetComponent<Renderer>().material;
        float accVal = mat.GetFloat("_AccentVal");
        accVal -= ReticleColorGravity * Time.deltaTime;
        mat.SetFloat("_AccentVal", Mathf.Clamp01(accVal));
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Test_Start_Pos)
        {
            VerticalRotation = 0;
        }
        else if (Test_Max_Up)
        {
            VerticalRotation = 1;
        }
        else if (Test_Max_Down)
        {
            VerticalRotation = -1;
        }


        AgainstWall.TargetDistanceAbove = 5 * VerticalRotation;

        float LinearInterpolationValue = Mathf.Abs(VerticalRotation);
        CameraParameters camParams;

        //block requires trailing else so compiler knows camParams is initialized
        if (VerticalRotation > 0)
            camParams = MaxUp;
        else if (VerticalRotation < 0)
            camParams = MaxDown;
        else
            camParams = StartPos;

        TransformCamera(StartPos, camParams, LinearInterpolationValue);
    }

    void TransformCamera(CameraParameters fromParams, CameraParameters toParams, float linearInterp, bool recursiveCall = false)
    {
        Vector3 targetPosition = Target.transform.position + Target.GetComponentInParent<CapsuleCollider>().height * 0.5f * Vector3.up;

        Vector3 lookTargetVertical = Target.transform.up * Mathf.Lerp(fromParams.TargetDistanceAbove, toParams.TargetDistanceAbove, linearInterp);
        Vector3 lookTargetHorizontal = Target.transform.forward * Mathf.Lerp(fromParams.TargetDistanceAhead, toParams.TargetDistanceAhead, linearInterp);
        m_LookTarget = targetPosition + lookTargetHorizontal + lookTargetVertical;

        float sideAngle = Mathf.Lerp(fromParams.AngleSide, toParams.AngleSide, linearInterp);

        //interpolate along quadratic bezier curve
        float interpDistAbove = Mathf.Lerp(fromParams.DistanceAbove, toParams.DistanceAbove, linearInterp);
        float interpDistBehind = Mathf.Lerp(fromParams.DistanceBehind, toParams.DistanceBehind, linearInterp);

        float distAbove = Mathf.Lerp(interpDistAbove, toParams.DistanceAbove, linearInterp);
        float distBehind = Mathf.Lerp(interpDistBehind, toParams.DistanceBehind, linearInterp);

        Vector3 xOffset = Mathf.Sin(sideAngle * Mathf.Deg2Rad) * -distBehind * Target.transform.right;
        Vector3 zOffset = Mathf.Cos(sideAngle * Mathf.Deg2Rad) * -distBehind * Target.transform.forward;
        Vector3 yOffset = distAbove * Target.transform.up;

        //Vector3 viewTargetPosition = Target.transform.position;
        Vector3 position = xOffset + yOffset + zOffset + targetPosition;

        Vector3 raycastDirection = Vector3.Normalize(position - targetPosition);
        float cameraDistance = Vector3.Magnitude(position - targetPosition);
        int layerMask = OpaqueLayer | TransparentLayer;

        //if we've already tried to get out of a wall and it didn't work, let the camera go through the wall instead of overflowing the stack trying to fix it
        if (!recursiveCall)
        {
            //if the camera is going through an outer level wall
            RaycastHit[] raycastHit = Physics.SphereCastAll(targetPosition, Target.GetComponentInParent<CapsuleCollider>().radius, raycastDirection, cameraDistance, layerMask);

            foreach (RaycastHit collision in raycastHit)
            {
                //comparing layers to a layermask requires bitshifting
                if ((OpaqueLayer.value >> collision.transform.gameObject.layer) == 1 && !collision.collider.isTrigger)
                {
                    float hitDist = 1 - ((collision.distance - Target.GetComponentInParent<CapsuleCollider>().radius) / cameraDistance);

                    //Debug.Log(collision.normal);

                    if (Test_Wall)
                        hitDist = 1;

                    float LinearInterpolationValue = hitDist;

                    CameraParameters interpolatedParams = new CameraParameters((zOffset + xOffset).magnitude, yOffset.magnitude, lookTargetHorizontal.magnitude, lookTargetVertical.y, sideAngle);

                    TransformCamera(interpolatedParams, AgainstWall, LinearInterpolationValue, true);

                    //allow the player to still look up and down when against a wall
                    transform.rotation = Quaternion.LookRotation(m_LookTarget - position);

                    //leave early so we don't set our position to what it would have been if the wall wasn't there
                    return;
                }
            }
        }

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(m_LookTarget - position);

        Ray castRay = new Ray(targetPosition, raycastDirection);
        layerMask = OpaqueLayer;

        if (Physics.SphereCast(castRay, Target.GetComponentInParent<CapsuleCollider>().radius, cameraDistance, layerMask))
        {
            Debug.Log("Camera is not being moved out of wall");
        }

        Crosshair.transform.position = m_LookTarget;
        OuterCrosshair.transform.position = m_LookTarget;
    }

    void OnPreRender()
    {
        for (int i = 0; i < Planes.Count; ++i)
        {
            Planes[i].Rotation = Planes[i].Object.transform.rotation;
            Planes[i].UpVector = Planes[i].Object.transform.up;

            //determine where the camera forward vector collides with the Planes defined by this object's right and up vectors
            Vector3 camToObj = Planes[i].Object.transform.position - transform.position;
            float normalDot = Vector3.Dot(camToObj, Planes[i].Object.transform.up);

            //because of the geometry of our situation, the equation can be simplified to
            float distance = normalDot;

            //intersection of ray and Planes[i]
            Vector3 collisionPoint = Planes[i].Object.transform.up * distance + transform.position;

            //Direction from object to collision point
            Vector3 collisionPointVector = collisionPoint - Planes[i].Object.transform.position;

            //Calculate angle it needs to be rotated
            //float angle = Vector3.Angle(-Planes[i].Object.transform.forward, collisionPointVector);

            //Orient the Planes[i]
            Planes[i].Object.transform.rotation = Quaternion.LookRotation(-collisionPointVector, Planes[i].UpVector);
        }
    }

    private void OnPreCull()
    {
        Crosshair.layer = LayerMask.GetMask("Default");
        OuterCrosshair.layer = LayerMask.GetMask("Default");
    }

    void OnPostRender()
    {
        Crosshair.layer = LayerMask.NameToLayer("Cull");
        OuterCrosshair.layer = LayerMask.NameToLayer("Cull");

        //reset rotations for next frame/camera
        foreach (CameraOrientedPlane plane in Planes)
        {
            plane.Object.transform.rotation = plane.Rotation;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawLine(transform.position, Target.transform.position + Target.GetComponentInParent<CapsuleCollider>().height * Vector3.up);
    //}
}