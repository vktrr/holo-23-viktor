using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.InputSystem;

public class AttractionForce : MonoBehaviour
{
    public ParticleSystem particles;
    public GameObject handReference;
    new SkinnedMeshRenderer renderer;
    public float attractionForce = 2f;
    public float repulsionForce = 5f;
    public float repelDuration = 1f;
    public float surfaceCheckDistance = 0.1f;
    public float dampingFactor = 0.1f;
    public float maxVelocity = 5f;

    public float minVibrationValue = 0f;
    public float maxVibrationValue = 1f;
    public float vibrationFrequency = 1f;

    public AudioSource chimes;
    public AudioSource dragonfly;
    public float minVolume = 0f;
    public float dragonflyMaxVolume = 1f;
    public float chimesMaxVolume = 1f;
    public float slowingDistance = 1f;


    public int motor = 0;
    private float currentVibrationValue = 0f;

    private bool isAttracted = false;
    private List<Vector3> surfacePointsLocal = new List<Vector3>();
    List<Vector3> surfacePointsWorld = new List<Vector3>();

     ParticleSystem.Particle[] particleArray;

    Mesh mesh;

    void OnEnable()
    {
       mesh = new Mesh();
       chimes.volume = minVolume;
         dragonfly.volume = minVolume;
    }

    void OnDisable()
    {
        Destroy(mesh);
        mesh = null;
    }

    void Update()
{

    var lastInputDevice = InputSystem.GetDevice<Esp32InputDevice>();

    float surfaceCoverage = CalculateSurfaceCoverage(surfaceCheckDistance);

    vibrations(surfaceCoverage, lastInputDevice);

    soundManager(surfaceCoverage);
}


    public void Enable()
    {
        if (!isAttracted)
        {
            Debug.Log("Enable attraction");
            isAttracted = true;
            var handController =  handReference.GetComponent<ArticulatedHandController>();
             renderer = handController.model.GetComponent<RiggedHandMeshVisualizer>().GetComponentInChildren<SkinnedMeshRenderer>();
             GetSurfacePoints(0.1f);
            particles.Stop();
            StartCoroutine(AttractParticles());
        }
    }

    public void Disable(float repelFactor)
    {
        if (isAttracted)
        {
            Debug.Log("Disable attraction");
            var lastInputDevice = InputSystem.GetDevice<Esp32InputDevice>();

            if(lastInputDevice!=null){
                lastInputDevice.SendMotorSpeed(motor,0);
            }
    
            isAttracted = false;
            particles.Stop();
            StopAllCoroutines();
            StartCoroutine(RepelParticles(repelFactor));
        }
    }

    private IEnumerator RepelParticles(float repelFactor)
    {
        float elapsedTime = 0f;

        while (elapsedTime < repelDuration)
        {
            UpdateWorldPositions();


            if(particleArray == null || particleArray.Length<particles.particleCount)
            {
                particleArray = new ParticleSystem.Particle[particles.particleCount];
            }
            
            int count = particles.GetParticles(particleArray);

            for (int i = 0; i < count; i++)
            {
                Vector3 particlePosition = particleArray[i].position;

                if (particleArray[i].remainingLifetime > 0f)
                {
                    float minDistance = float.MaxValue;
                    Vector3 closestPoint = Vector3.zero;

                    foreach (Vector3 surfacePoint in surfacePointsWorld)
                    {
                        float distance = Vector3.Distance(particlePosition, surfacePoint);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestPoint = surfacePoint;
                        }
                    }

                    Vector3 repulsionDirection = particlePosition - closestPoint;
                    float force = repulsionForce / Mathf.Pow(minDistance, 2) * (float)(repelFactor * 0.01);
                    particleArray[i].velocity += repulsionDirection * force * Time.deltaTime;

                    // Clamp velocity to maxVelocity
                    // particleArray[i].velocity = Vector3.ClampMagnitude(particleArray[i].velocity, (float)maxVelocity*3.0f);
                }
            }

            particles.SetParticles(particleArray, count);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }


    private IEnumerator AttractParticles()
    {
        while (isAttracted)
        {
            UpdateWorldPositions();
            ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[particles.particleCount];
            particles.GetParticles(particleArray);
            
             for (int i = 0; i < particleArray.Length; i++)
    {
        Vector3 particlePosition = particleArray[i].position;

        if (particleArray[i].remainingLifetime > 0f)
        {
            float minDistance = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;

            foreach (Vector3 surfacePoint in surfacePointsWorld)
            {
                float distance = Vector3.Distance(particlePosition, surfacePoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = surfacePoint;
                }
            }

            Vector3 attractionDirection = closestPoint - particlePosition;
            float force = attractionForce / Mathf.Pow(minDistance, 2);
            particleArray[i].velocity += attractionDirection * force * Time.deltaTime;

            // Clamp velocity to maxVelocity
            particleArray[i].velocity = Vector3.ClampMagnitude(particleArray[i].velocity, (float)maxVelocity);

            // Dampen velocity when close to the surface
            if (minDistance <= 0.1f)
            {
                particleArray[i].velocity *= 1 - dampingFactor * Time.deltaTime;
            }
            
            // Slow down particles as they approach the target object
            if (minDistance <= slowingDistance)
            {
                particleArray[i].velocity = particleArray[i].velocity.normalized;
            }
        }
    }

    particles.SetParticles(particleArray, particleArray.Length);

    yield return null;
}
    }

    private void GetSurfacePoints( float range)
    {
        renderer.BakeMesh(mesh,true);
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        surfacePointsLocal.Clear();

        var scale = renderer.transform.lossyScale.x;
        print(scale);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 offset =  normals[i]*0.003f + Random.insideUnitSphere * 0.005f;
            surfacePointsLocal.Add( vertices[i] +offset/scale );
        }
    }

    void UpdateWorldPositions(){
        
            surfacePointsWorld.Clear();
            for (int i = 0; i < surfacePointsLocal.Count; i++)
            {
                surfacePointsWorld.Add(renderer.transform.TransformPoint(surfacePointsLocal[i]));
            }
    }

    private float CalculateSurfaceCoverage(float distanceThreshold)
{
    int surfaceParticles = 0;
    ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[particles.particleCount];
    particles.GetParticles(particleArray);

    for (int i = 0; i < particleArray.Length; i++)
    {
        Vector3 particlePosition = particleArray[i].position;
        float minDistance = float.MaxValue;

        foreach (Vector3 surfacePoint in surfacePointsWorld)
        {
            float distance = Vector3.Distance(particlePosition, surfacePoint);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        if (minDistance <= distanceThreshold)
        {
            surfaceParticles++;
        }
    }

    float surfaceCoverage = (float)surfaceParticles / particleArray.Length * 100;
    return surfaceCoverage;
}

    void vibrations(float landedPercentage, Esp32InputDevice lastInputDevice)
    {

        float maxValue = Map(landedPercentage, 0, 100, 0, 1);
        float frequency = Map(landedPercentage, 0, 100, 10, vibrationFrequency);

        float oscillation = Mathf.Sin(Time.time * frequency);
        currentVibrationValue = Map(oscillation, -1, 1, 0, maxValue);

        currentVibrationValue = Mathf.Clamp(currentVibrationValue, 0, 1);


        if(lastInputDevice!=null){

            lastInputDevice.SendMotorSpeed(motor,currentVibrationValue);
        }
    } 

    void soundManager(float landedPercentage ) {
        float surfaceCoverage = CalculateSurfaceCoverage(surfaceCheckDistance);
        float chimesVolume = Map(surfaceCoverage, 0, 100, minVolume, chimesMaxVolume);
        float dragonflyVolume = Map(surfaceCoverage, 0, 100, minVolume, dragonflyMaxVolume);

            chimes.volume = chimesVolume;
            dragonfly.volume = dragonflyVolume;
    }



    private static float Map(float value, float min, float max, float newMin, float newMax)
    {
    // Clamp the value between the min and max of the source range
    value = Mathf.Clamp(value, min, max);

    // Map the value from the source range to the target range
    float mappedValue = Mathf.InverseLerp(min, max, value);
    mappedValue = Mathf.Lerp(newMin, newMax, mappedValue);

    return mappedValue;
    }
  
}
