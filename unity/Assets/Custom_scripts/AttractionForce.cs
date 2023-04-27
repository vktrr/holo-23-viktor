using System.Collections;
using UnityEngine;

public class AttractionForce : MonoBehaviour
{
    public ParticleSystem particles;
    public GameObject target;
    public float attractionForce = 2f;
    public float repulsionForce = 5f;
    public float repelDuration = 1f;
    public float dampingFactor = 0.1f;
    public float maxVelocity = 5f;

    private bool isAttracted = false;
    private Vector3[] surfacePoints;

    public void Enable()
    {
        if (!isAttracted)
        {
            Debug.Log("Enable attraction");
            isAttracted = true;
            surfacePoints = GetSurfacePoints(target, 0.1f);
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
            ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[particles.particleCount];
            particles.GetParticles(particleArray);

            for (int i = 0; i < particleArray.Length; i++)
            {
                Vector3 particlePosition = particleArray[i].position;

                if (particleArray[i].remainingLifetime > 0f)
                {
                    float minDistance = float.MaxValue;
                    Vector3 closestPoint = Vector3.zero;

                    foreach (Vector3 surfacePoint in surfacePoints)
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

            particles.SetParticles(particleArray, particleArray.Length);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AttractParticles()
    {
        while (isAttracted)
        {
            ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[particles.particleCount];
            particles.GetParticles(particleArray);

            for (int i = 0; i < particleArray.Length; i++)
            {
                Vector3 particlePosition = particleArray[i].position;

                if (particleArray[i].remainingLifetime > 0f)
                {
                    float minDistance = float.MaxValue;
                    Vector3 closestPoint = Vector3.zero;

                    foreach (Vector3 surfacePoint in surfacePoints)
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

    private Vector3[] GetSurfacePoints(GameObject meshObject, float range)
    {
        Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] surfacePoints = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            surfacePoints[i] = meshObject.transform.TransformPoint(vertices[i]);
        }

        return surfacePoints;
    }
}
