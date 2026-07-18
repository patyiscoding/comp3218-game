using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    public class MovePlayerInput2 : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_JumpButton = "Jump";
        [SerializeField]
        private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField]
        private PlayerCamera m_Camera;
        
        [SerializeField]
        private string m_MouseX = "Mouse X";
        [SerializeField]
        private string m_MouseY = "Mouse Y";
        [SerializeField]
        private string m_MouseScroll = "Mouse ScrollWheel";

        private CreatureMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
        }

        // private void Update()
        // {
        //     GatherInput();
        //     SetInput();
        // }

        public void Move(Transform target, bool shouldRun = false, bool shouldJump = false)
        {
            Debug.Log("MovePlayerinput");
            // m_Axis = new Vector2(Input.GetAxis(m_HorizontalAxis), Input.GetAxis(m_VerticalAxis));
            Vector3 direction = target.position - transform.position;
            float distance = direction.magnitude;
            if (distance > 0.2f)
            {
                Vector2 axis = new Vector2(0f, 1f); // forward
                //mover.SetInput(axis, target.position, false, false);
            }
            else
            {
                //mover.SetInput(Vector2.zero, target.position, false, false);
            }
            
            m_Axis = new Vector2(0f, 1f);
            m_IsRun = shouldRun;
            m_IsJump = shouldJump;
            Vector3 destination = target.position - transform.position;
            m_Target = destination;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);
            SetInput();
        }

        public void BindMover(CreatureMover mover)
        {
            m_Mover = mover;
        }

        public void SetInput()
        {
            Debug.Log("setinput");
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        }
    }
}