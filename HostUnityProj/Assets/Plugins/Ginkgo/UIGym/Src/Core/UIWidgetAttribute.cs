using System;

namespace Ginkgo.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UIWidgetAttribute : Attribute
    {
        protected string m_fullname;
        protected int m_index;
        public UIWidgetAttribute(string dottedfullname = "")
        {
            m_fullname = dottedfullname;
            m_index = -1;
        }
        public string FullName
        {
            get
            {
                return m_fullname;
            }
        }
    }
}
