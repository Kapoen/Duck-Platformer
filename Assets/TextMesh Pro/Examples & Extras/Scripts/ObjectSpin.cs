namespace TMPro.Examples
{
    using System.Collections;
    using UnityEngine;

    public class ObjectSpin : MonoBehaviour
    {
        #pragma warning disable 0414
        public enum MotionType { Rotation, SearchLight, Translation };
        public MotionType Motion;

        public Vector3 TranslationDistance = new Vector3(5, 0, 0);
        public float TranslationSpeed = 1.0f;
        public float SpinSpeed = 5;
        public int RotationRange = 15;
        private Transform m_transform;

        private float m_time;
        private Vector3 m_prevPOS;
        private Vector3 m_initial_Rotation;
        private Vector3 m_initial_Position;
        private Color32 m_lightColor;

        void Awake()
        {
            this.m_transform = this.transform;
            this.m_initial_Rotation = this.m_transform.rotation.eulerAngles;
            this.m_initial_Position = this.m_transform.position;

            Light light = this.GetComponent<Light>();
            this.m_lightColor = light != null ? light.color : Color.black;
        }


        // Update is called once per frame
        void Update()
        {
            switch (this.Motion)
            {
                case MotionType.Rotation:
                    this.m_transform.Rotate(0, this.SpinSpeed * Time.deltaTime, 0);
                    break;
                case MotionType.SearchLight:
                    this.m_time += this.SpinSpeed * Time.deltaTime;
                    this.m_transform.rotation = Quaternion.Euler(this.m_initial_Rotation.x, (Mathf.Sin(this.m_time) * this.RotationRange) + this.m_initial_Rotation.y, this.m_initial_Rotation.z);
                    break;
                case MotionType.Translation:
                    this.m_time += this.TranslationSpeed * Time.deltaTime;

                    float x = this.TranslationDistance.x * Mathf.Cos(this.m_time);
                    float y = this.TranslationDistance.y * Mathf.Sin(this.m_time) * Mathf.Cos(this.m_time * 1f);
                    float z = this.TranslationDistance.z * Mathf.Sin(this.m_time);

                    this.m_transform.position = this.m_initial_Position + new Vector3(x, z, y);

                    // Drawing light patterns because they can be cool looking.
                    //if (Time.frameCount > 1)
                    //    Debug.DrawLine(m_transform.position, m_prevPOS, m_lightColor, 100f);

                    this.m_prevPOS = this.m_transform.position;
                    break;
            }
        }
    }
}