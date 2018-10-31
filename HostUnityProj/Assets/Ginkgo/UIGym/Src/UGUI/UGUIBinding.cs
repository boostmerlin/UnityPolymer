using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UniRx;
using Ginkgo.UI;
using Ginkgo;
using UnityEngine;

public static class UGUIBindingExtensions
{
    public static IDisposable BindTextToProperty(this UIView view, Text text, P<string> property)
    {
        var d = property.Subscribe(value =>
        {
            text.text = value;
        });
        text.text = property.Value ?? string.Empty;
        return d.DisposeWith(view);
    }

    public static IDisposable BindInputFieldToProperty(this UIView view, InputField widget, P<string> property, bool submitWhenChange = false)
    {
        var callback = new UnityAction<string>((s) => 
            property.OnNext(s)
        );
        if(submitWhenChange)
        {
            widget.onValueChanged.AddListener(callback);
        }
        else
        {
            widget.onEndEdit.AddListener(callback);
        }
        var d1 = property.Subscribe(value =>
        {
            widget.text = value;
        });
        var d2 = Disposable.Create(() =>
        {
            if (submitWhenChange)
            {
                widget.onValueChanged.RemoveListener(callback);
            }
            else
            {
                widget.onEndEdit.RemoveListener(callback);
            }
            d1.Dispose();
        });
        widget.text = property.Value ?? string.Empty;

        return d2.DisposeWith(view);
    }

    public static IDisposable BindButtonToHandler(this UIView view, Button widget, Action handler)
    {
        var callback = new UnityAction(() => handler());
        widget.onClick.AddListener(callback);
        var d1 = Disposable.Create(() =>
        {
            widget.onClick.RemoveListener(callback);
        });
        return d1;
    }
}