using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ML
{
    public class ScreenDebugger : MonoBehaviour
    {
        private const int MAX_MESSAGES = 100;

        public float m_DefaultTimeScale = 1f;

        public float m_MinTimeScale = 0.01f;

        public float m_MaxTimeScale = 4f;

        public Vector2 m_GUIPosition = new Vector2(0.01f, 0.065f);

        public Vector2 m_GUISize = new Vector2(175f, 30f);

        private static ScreenDebugger s_instance;

        private Queue<string> m_messages;

        private StringBuilder m_messageBuilder;

        private GUIStyle m_messageStyle;

        private bool m_hideMessages;

        private Vector2 scrollPosition;

        private void Awake()
        {
            ScreenDebugger.s_instance = this;
        }

        private void OnDestroy()
        {
            ScreenDebugger.s_instance = null;
        }

        private void OnGUI()
        {
            this.LayoutLeftScreenControls();
        }

        public static ScreenDebugger Get()
        {
            return ScreenDebugger.s_instance;
        }

        public void AddMessage(string message)
        {
            this.InitMessagesIfNecessary();
            if (this.m_messages.Count >= MAX_MESSAGES)
            {
                this.m_messages.Dequeue();
            }
            this.m_messages.Enqueue(message);
        }

        private void LayoutLeftScreenControls()
        {
            Vector2 gUISize = this.m_GUISize;
            Vector2 vector = new Vector2((float)Screen.width * this.m_GUIPosition.x, (float)Screen.height * this.m_GUIPosition.y);
            Vector2 vector2 = new Vector2(vector.x, vector.y);
            Vector2 vector3 = default(Vector2);
            vector3 = vector2;
            this.LayoutTimeControls(ref vector3, gUISize);
            this.LayoutQualityControls(ref vector3, gUISize);
            this.LayoutStats(ref vector3, gUISize);
            this.LayoutMessages(ref vector3, gUISize);
        }

        private void LayoutTimeControls(ref Vector2 offset, Vector2 size)
        {
            GUI.Box(new Rect(offset.x, offset.y, size.x, size.y), string.Format("Time Scale: {0}", Time.timeScale));
            offset.y += 1f * size.y;
            Time.timeScale = GUI.HorizontalSlider(new Rect(offset.x, offset.y, size.x, size.y), Time.timeScale, this.m_MinTimeScale, this.m_MaxTimeScale);
            offset.y += 1f * size.y;
            if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Reset Time Scale"))
            {
                Time.timeScale = 1f;
            }
            offset.y += 1.5f * size.y;
        }

        private void LayoutQualityControls(ref Vector2 offset, Vector2 size)
        {
            offset.y += 1.5f * size.y;
        }

        private void LayoutStats(ref Vector2 offset, Vector2 size)
        {
        }

        [Conditional("UNITY_EDITOR")]
        private void LayoutCursorControls(ref Vector2 offset, Vector2 size)
        {
            if (Cursor.visible)
            {
                if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Force Hardware Cursor Off"))
                {
                    Cursor.visible = false;
                }
            }
            else if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Force Hardware Cursor On"))
            {
                Cursor.visible = true;
            }
            offset.y += 1.5f * size.y;
        }

        private void InitMessagesIfNecessary()
        {
            if (this.m_messages != null)
            {
                return;
            }
            this.m_messages = new Queue<string>();
        }

        private void InitMessageStyleIfNecessary()
        {
            if (this.m_messageStyle != null)
            {
                return;
            }
            this.m_messageStyle = new GUIStyle("box")
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                clipping = TextClipping.Overflow,
                stretchWidth = true
            };
            m_messageStyle.fontSize = m_messageStyle.fontSize + 20;

        }

        private void LayoutMessages(ref Vector2 offset, Vector2 size)
        {
            if (this.m_messages == null)
            {
                return;
            }
            if (this.m_messages.Count == 0)
            {
                return;
            }
            this.InitMessageStyleIfNecessary();
            if (this.m_hideMessages)
            {
                if (!GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Show Messages"))
                {
                    return;
                }
                this.m_hideMessages = false;
            }
            else if (GUI.Button(new Rect(offset.x, offset.y, size.x, size.y), "Hide Messages"))
            {
                this.m_hideMessages = true;
                return;
            }
            if (GUI.Button(new Rect(size.x + offset.x, offset.y, size.x, size.y), "Clear Messages"))
            {
                this.m_messages.Clear();
                return;
            }
            offset.y += size.y;
            string messageText = this.GetMessageText();
            float num = (float)Screen.height - offset.y;
            var r = new Rect(offset.x, offset.y, (float)Screen.width - offset.x, num);
            GUILayout.BeginArea(r);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(r.width), GUILayout.Height(num));
            GUILayout.Label(messageText, this.m_messageStyle);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            offset.y += num;
        }

        private string GetMessageText()
        {
            this.m_messageBuilder = new StringBuilder();
            foreach (string msg in this.m_messages)
            {
                this.m_messageBuilder.AppendLine(msg);
            }
            return this.m_messageBuilder.ToString();
        }
    }
}