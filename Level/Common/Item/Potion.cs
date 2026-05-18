using DungeonDelve.Level.Common;

public class Potion : Item
{
        public int HealthRestore { get; set; }
        public int ManaRestore { get; set; }
        public int Duration { get; set; }
        public bool IsPoisonous { get; set; }
    
        public Potion(string name, int healthRestore, int manaRestore, int duration, bool isPoisonous)
        {
            Name = name;
            HealthRestore = healthRestore;
            ManaRestore = manaRestore;
            Duration = duration;
            IsPoisonous = isPoisonous;
        }
}