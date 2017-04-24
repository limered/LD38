using UnityEngine;

namespace Assets.Systems.FightingSystem
{
    public class VictimComponent : MonoBehaviour {
        public void KnockBack(Vector3 direction)
        {
            var rigidBody = GetComponent<Rigidbody>();
            rigidBody.AddForce(direction * 3000, ForceMode.Force);
        }
    }
}
