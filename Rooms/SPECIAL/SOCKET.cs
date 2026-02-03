using Godot;
using System;

public partial class SOCKET : Marker3D
{
    public bool isUsed;
    public void Use()
    {
        if (isUsed)
        {
            return;
        }

        isUsed = true;
        GD.Print("Socket wurde benutzt");
    }
    
}
