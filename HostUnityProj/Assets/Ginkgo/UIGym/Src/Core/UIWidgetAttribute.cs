using System;

namespace Ginkgo.UI
{
    /// <summary>
    /// UGUI 下不指定全路径会很慢！
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UIWidgetAttribute : Attribute
    {
        protected string m_fullname;
        protected int depth;
        public UIWidgetAttribute(string dottedfullname = "", int depth = -1)
        {
            m_fullname = dottedfullname;
            this.depth = depth;
        }
        public string FullName
        {
            get
            {
                return m_fullname;
            }
        }
        public int Depth
        {
            get
            {
                return depth;
            }
        }
    }
}
