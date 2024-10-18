namespace Uttom.Domain.Models;

// Better move this to an Interface
public class RentalCalculator
{
    public decimal CalculateTotalRentalPrice(DateTime plannedReturnDate, DateTime actualReturnDate, decimal dailyRate, int rentalPlanDays)
    {
        // Calculate the number of days for the rental
        var plannedDays = (plannedReturnDate - DateTime.Now).Days;
        var actualDays = (actualReturnDate - DateTime.Now).Days;

        // Total price starts with the daily rate multiplied by planned days
        var totalPrice = dailyRate * rentalPlanDays;

        if (actualReturnDate < plannedReturnDate)
        {
            // Calculate the number of unfulfilled days
            var unfulfilledDays = (plannedReturnDate - actualReturnDate).Days;

            // Determine the penalty based on the rental plan
            var penaltyRate = rentalPlanDays == 7 ? 0.20m : 0.40m;
            var penalty = (dailyRate * unfulfilledDays) * penaltyRate;

            // subtract the dailyRate from unfulfilledDays
            var unfulfilledDaysPrice = dailyRate * unfulfilledDays;

            totalPrice = (totalPrice - unfulfilledDaysPrice) + penalty;
        }
        else if (actualReturnDate > plannedReturnDate)
        {
            // Calculate additional days and their cost
            var additionalDays = actualDays - plannedDays;
            totalPrice += additionalDays * 50; // R$50.00 per additional day
        }

        return totalPrice;
    }
}