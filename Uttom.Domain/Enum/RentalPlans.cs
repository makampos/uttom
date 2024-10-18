using Uttom.Domain.Models;

namespace Uttom.Domain.Enum;

public static class RentalPlans
{
    // Where int is the number of days and decimal is the price
    private static readonly Dictionary<int, decimal> Plans = new()
    {
        { 7, 30.00m },
        { 15, 28.00m },
        { 30, 22.00m },
        { 45, 20.00m },
        { 50, 18.00m }
    };

    public static Plan? GetPlan(int days)
    {

        if (!Plans.TryGetValue(days, out decimal value))
        {
            return null;
        }

        return new Plan
        {
            Days = days,
            Price = value
        };
    }

    public static int GetDays(decimal price)
    {
        return Plans.FirstOrDefault(x => x.Value == price).Key;
    }

    public static decimal GetPrice(int days)
    {
        return Plans[days];
    }

    // public static int GetDays(decimal price)
    // {
    //     return Plans.FirstOrDefault(x => x.Value == price).Key;
    // }

    public static bool PlanExists(int days)
    {
        return Plans.ContainsKey(days);
    }

    public static bool PriceExists(decimal price)
    {
        return Plans.ContainsValue(price);
    }

    public static bool PlanPriceExists(int days, decimal price)
    {
        return Plans.ContainsKey(days) && Plans.ContainsValue(price);
    }
}
