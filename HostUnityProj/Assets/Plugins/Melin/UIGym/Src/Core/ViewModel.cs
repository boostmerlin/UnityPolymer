using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System;


namespace ML.UI
{
    public abstract class ViewModel
    {
        static Dictionary<Type, ViewModel> viewModelsMaps = new Dictionary<Type, ViewModel>();

        public static T Get<T>(UIView owner, object[] args = null) where T : ViewModel
        {
            var type = typeof(T);
            if (viewModelsMaps.ContainsKey(type))
            {
                return viewModelsMaps[type] as T;
            }
            T vm = MSystem.Container.CreateInstance(type, args) as T;
            vm.OnCreate();
            viewModelsMaps[type] = vm;
            return vm;
        }

        public UIView OwnerView { get; set; }
        public event PropertyChangedHandler PropertyChanged;

        public virtual void OnPropertyChanged(object sender, string propertyName)
        {
            PropertyChangedHandler handler = PropertyChanged;
            if (handler != null) handler(this, propertyName);
        }

        protected abstract void OnCreate();
    }
}
