using System;

namespace Ginkgo.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UIWidgetAttribute : Attribute
    {
        protected string m_fullname;
        public UIWidgetAttribute(string dottedfullname = "")
        {
            m_fullname = dottedfullname;
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
