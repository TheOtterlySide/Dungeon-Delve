using Godot;
using System;

public partial class RoomSocket : Marker3D
{
    public bool isUsed;
    public void Use()
    {
        if (isUsed)
        {
            return;
        }

        isUsed = true;
    }
    
}
