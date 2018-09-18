using System;
using UnityEngine;
using Writership;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    public static class Ops
    {
        public struct Deform
        {
            public Vector3 Point;
            public float Force;
        }
    }

    public float springForce = 20f;
    public float damping = 5f;

    Mesh deformingMesh;
    Vector3[] originalVertices;
    Ar<Vector3> displacedVertices;
    Ar<Vector3> vertexVelocities;

    public Op<Ops.Deform> Deform;

    float uniformScale = 1f;

    public readonly CompositeDisposable cd = new CompositeDisposable();

    void OnDestroy()
    {
        cd.Dispose();
    }

    void Start()
    {
        Deform = G.Engine.Op<Ops.Deform>();

        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        {
            var tmp = new Vector3[originalVertices.Length];
            Array.Copy(originalVertices, tmp, originalVertices.Length);
            displacedVertices = new Ar<Vector3>(G.Engine, tmp);
        }
        vertexVelocities = new Ar<Vector3>(G.Engine, new Vector3[originalVertices.Length]);
        uniformScale = transform.localScale.x;

        G.Engine.Computer(cd, Dep.On(Deform, G.Tick), () =>
        {
            var vertexVelocities = this.vertexVelocities.AsWrite();
            var displacedVertices = this.displacedVertices.AsWrite();
            float dt = G.Tick.Reduced;

            for (int i = 0, n = Deform.Count; i < n; ++i)
            {
                var d = Deform[i];
                AddDeformingForce(d.Point, d.Force, displacedVertices, vertexVelocities, dt);
            }

            for (int i = 0, n = displacedVertices.Length; i < n ; i++)
            {
                UpdateVertex(i, displacedVertices, vertexVelocities, dt);
            }
        });

        G.Engine.Reader(cd, Dep.On(G.Tick), () =>
        {
            deformingMesh.vertices = displacedVertices.Read();
            deformingMesh.RecalculateNormals();
        });
    }

    void UpdateVertex(int i, Vector3[] displacedVertices, Vector3[] vertexVelocities, float dt)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
        velocity -= displacement * springForce * dt;
        velocity *= 1f - damping * dt;
        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (dt / uniformScale);
    }

    void AddDeformingForce(Vector3 point, float force, Vector3[] displacedVertices, Vector3[] vertexVelocities, float dt)
    {
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force, displacedVertices, vertexVelocities, dt);
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force, Vector3[] displacedVertices, Vector3[] vertexVelocities, float dt)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * dt;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
}