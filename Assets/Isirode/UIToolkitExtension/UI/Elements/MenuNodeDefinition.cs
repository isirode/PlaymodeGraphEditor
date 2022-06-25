using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct MenuNodeDefinition
{
    public String text;

    public List<MenuNodeDefinition> children;

    public EventCallback<PointerUpEvent> callback;

}
