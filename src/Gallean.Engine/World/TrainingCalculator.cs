namespace Gallean.Engine;

internal static class TrainingCalculator
{
    public sealed record TrainingInfo(
        int CurrentExperience,
        int MaxExperience,
        int TrainableExperience,
        WorldResourceSet CostPerExperience,
        WorldResourceSet Cost,
        WorldCommandError? Error);

    public static TrainingInfo GetTrainingInfo(WorldUnit unit, WorldResourceSet available, IEngineContext context)
    {
        UnitStats stats = context.UnitStats.GetWorldUnitStats(unit);
        UnitCosts costs = context.UnitStats.GetUnitCosts(unit.DefinitionId, unit.Level);

        int maxExperience = stats.ExperienceNextLevel - 1;
        int affordableExperience = GetAffordableExperience(available, costs.TrainingCost);
        int experience = Math.Min(maxExperience - unit.Experience, affordableExperience);

        WorldCommandError? error = GetError(unit, maxExperience, costs.TrainingCost, experience, context);

        if (error is not null)
        {
            return new TrainingInfo(unit.Experience, maxExperience, 0, costs.TrainingCost, WorldResourceSet.Empty, error);
        }

        WorldResourceSet cost = ResourceSetCalculator.Multiply(costs.TrainingCost, experience);

        return new TrainingInfo(unit.Experience, maxExperience, experience, costs.TrainingCost, cost, null);
    }

    private static WorldCommandError? GetError(
        WorldUnit unit,
        int maxExperience,
        WorldResourceSet costPerExperience,
        int experience,
        IEngineContext context)
    {
        if (!unit.IsAlive)
        {
            return WorldCommandError.UnitDead;
        }

        if (costPerExperience == WorldResourceSet.Empty)
        {
            return WorldCommandError.UnitTrainingForbidden;
        }

        UnitExperience unitExperience = context.UnitStats.GetWorldUnitExperience(unit);

        if (unitExperience.IsMaximumLevel)
        {
            return WorldCommandError.UnitFullyTrained;
        }

        if (maxExperience < 0)
        {
            return WorldCommandError.UnitFullyTrained;
        }

        if (unit.Experience >= maxExperience)
        {
            return WorldCommandError.UnitAlreadyTrained;
        }

        if (experience <= 0)
        {
            return WorldCommandError.InsufficientResources;
        }

        return null;
    }

    private static int GetAffordableExperience(WorldResourceSet available, WorldResourceSet costPerExperience)
    {
        int affordable = int.MaxValue;

        affordable = Limit(affordable, available.Gold, costPerExperience.Gold);
        affordable = Limit(affordable, available.DeathMana, costPerExperience.DeathMana);
        affordable = Limit(affordable, available.RuneMana, costPerExperience.RuneMana);
        affordable = Limit(affordable, available.LifeMana, costPerExperience.LifeMana);
        affordable = Limit(affordable, available.InfernalMana, costPerExperience.InfernalMana);
        affordable = Limit(affordable, available.GroveMana, costPerExperience.GroveMana);

        if (affordable != int.MaxValue)
        {
            return affordable;
        }
        else
        {
            return 0;
        }

        static int Limit(int affordable, int available, int costPerExperience)
        {
            if (costPerExperience > 0)
            {
                return Math.Min(affordable, available / costPerExperience);
            }
            else
            {
                return affordable;
            }
        }
    }
}
