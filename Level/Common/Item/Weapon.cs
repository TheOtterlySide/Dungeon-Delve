
using DungeonDelve.Level.Common;

public class Weapon : Item
{
    public int Damage { get; set; }
    public int Durability { get; set; }

    public Weapon(string name, int damage, int durability)
    {
        Name = name;
        Damage = damage;
        Durability = durability;
    }
}