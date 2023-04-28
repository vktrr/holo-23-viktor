using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class AttractionForce : MonoBehaviour
{
    public ParticleSystem particles;
    public GameObject handReference;
    new SkinnedMeshRenderer renderer;
    public float attractionForce = 2f;
    public float repulsionForce = 5f;
    public float repelDuration = 1f;
    public float dampingFactor = 0.1f;
    public float maxVelocity = 5f;

    private bool isAttracted = false;
    private List<Vector3> surfacePointsLocal = new List<Vector3>();
    List<Vector3> surfacePointsWorld = new List<Vector3>();

     ParticleSystem.Particle[] particleArray;

    Mesh mesh;

    void OnEnable()
    {
       mesh = new Mesh();
    }

    void OnDisable()
    {
        Destroy(mesh);
        mesh = null;
    }

    public void Enable()
    {
        if (!isAttracted)
        {
            Debug.Log("Enable attraction");
            isAttracted = true;
            var handController =  handReference.GetComponent<ArticulatedHandController>();
             renderer = handController.model.GetComponent<RiggedHandMeshVisualizer>().GetComponentInChildren<SkinnedMeshRenderer>();
             GetSurfacePoints( 0.1f);
            particles.Stop();
            StartCoroutine(AttractParticles());
        }
    }

    public void Disable(float repelFactor)
    {
        if (isAttracted)
        {
            Debug.Log("Disable attraction");
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

    void OnDrawGizmos(){

        foreach (Vector3 surfacePoint in surfacePointsWorld)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(surfacePoint, 0.005f);
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
}
