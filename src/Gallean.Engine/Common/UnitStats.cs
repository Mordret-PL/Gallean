namespace Gallean.Engine;

public sealed record UnitStats(
    int MaxHitpoints,
    int Armor,
    int Regeneration,
    int Initiative,
    int ExperienceKilled,
    int ExperienceNextLevel);
