using System;
using UnityEngine;

[Serializable]
public struct NodeDto
{
    public String nodeType;
    public Vector2 position;
    // Info : we are using a string because Unity does not serialize Guids
    public String guid;
    // Info : we are using a string because Unity does not serialize objects
    public String userStateObject;
}

