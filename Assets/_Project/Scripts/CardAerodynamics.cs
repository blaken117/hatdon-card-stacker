using UnityEngine;

namespace Hatdon.CardStacker
{
    // Still-air flat-plate approximation. Distributed forces resist translation and tumble,
    // without artificial flutter, pose correction, or support at rest.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CardAerodynamics : MonoBehaviour
    {
        public const float AirDensity = 1.225f, NormalCoefficient = 1.28f, SkinCoefficient = .01f;
        private Rigidbody body;
        private void Awake() => body = GetComponent<Rigidbody>();
        private void FixedUpdate() => ApplyForces();

        public static Vector3 PatchForce(Vector3 velocity, Vector3 normal, float area)
        {
            float speed = Vector3.Dot(velocity, normal);
            Vector3 tangent = velocity - normal * speed;
            return -.5f * AirDensity * area *
                (NormalCoefficient * speed * Mathf.Abs(speed) * normal +
                 2 * SkinCoefficient * tangent.magnitude * tangent);
        }

        // Also called explicitly by scripted-physics verification, where Unity doesn't run FixedUpdate.
        public void ApplyForces()
        {
            if (body == null || body.isKinematic || body.IsSleeping()) return;
            Vector3 size = transform.localScale;
            Vector3 normal = body.rotation * Vector3.forward;
            float area = size.x * size.y / 16;
            for (int x = 0; x < 4; x++) for (int y = 0; y < 4; y++)
            {
                Vector3 local = new Vector3(((x + .5f) / 4 - .5f) * size.x, ((y + .5f) / 4 - .5f) * size.y, 0);
                Vector3 point = body.position + body.rotation * local;
                Vector3 force = PatchForce(body.GetPointVelocity(point), normal, area);
                if (force.sqrMagnitude > 0) body.AddForceAtPosition(force, point, ForceMode.Force);
            }
        }
    }
}
