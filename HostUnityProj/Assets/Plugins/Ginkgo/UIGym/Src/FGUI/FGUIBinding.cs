using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using System;
using UniRx;

namespace Ginkgo.UI
{
    public static class FGUIBindingExtensions
    {
        public static IDisposable BindGTextFieldToProperty(this UIView view, GTextField text, P<string> property)
        {
            var d = property.Subscribe(value =>
            {
                text.text = value;
            });
            text.text = property.Value ?? string.Empty;

            return d.DisposeWith(view);
        }

        public static IDisposable BindGTextInputToProperty(this UIView view, GTextInput widget, P<string> property)
        {
            var callback = new EventCallback1((c) => {
                object d = c.data;
                property.OnNext(d.ToString());
            } );
            widget.onChanged.Add(callback);
            var d1 = property.Subscribe(value =>
            {
                widget.text = value;
            });
            var d2 = Disposable.Create(() =>
            {
                widget.onClick.Remove(callback);
                d1.Dispose();
            });
            widget.text = property.Value ?? string.Empty;

            return d2.DisposeWith(view);
        }

        public static IDisposable BindGButtonToHandler(this UIView view, GButton widget, Action handler)
        {
            var callback = new EventCallback1((c) => handler());
            widget.onClick.Add(callback);
            var d1 = Disposable.Create(() =>
            {
                widget.onClick.Remove(callback);
            });
            return d1;
        }

        public static IDisposable BindGButtonTextToProperty(this UIView view, GButton widget, P<string> property)
        {
            var d = property.Subscribe(value =>
            {
                widget.text = value;
            });
            property.OnNext(widget.text);

            return d.DisposeWith(view);
        }
    }
}