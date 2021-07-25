using UnityEngine;
using Util;
using Random = System.Random;

namespace Debug
{
    public class RandomizedDirection : MonoBehaviour
    {
        public int count = 10;
        [Range(0, 1)] public float difference = 0.5f;

        public float minDot;
        private Random random;

        private Vector3 RandomizeDirection(Vector3 direction, float difference)
        {
            float multiplier = ((float) random.NextDouble() * 2 - 1) * difference;

            Vector3 randomVector = GetRandomVector3() * 2 * multiplier;
            return (direction + randomVector).normalized;
        }

        private Vector3 GetRandomVector3()
        {
            float x = (float) random.NextDouble() * 2 - 1;
            float y = (float) random.NextDouble() * 2 - 1;
            float z = (float) random.NextDouble() * 2 - 1;
            return new Vector3(x, y, z).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            random = new Random(0);
            Vector3 point1 = transform.position;

            minDot = 1.0f;
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = RandomizeDirection(transform.forward, difference);
                float dot = Vector3.Dot(transform.forward, direction);
                if (dot < minDot)
                    minDot = dot;
                Gizmos.color = ColorUtil.RandomColor(i);
                Gizmos.DrawLine(point1, point1 + direction);
            }
        }
    }
}