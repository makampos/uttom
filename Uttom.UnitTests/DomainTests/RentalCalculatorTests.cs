using FluentAssertions;
using Uttom.Domain.Models;

namespace Uttom.UnitTests.DomainTests;

[Collection("Unit Tests")]
public class RentalCalculatorTests
{
    private readonly RentalCalculator _rentalCalculator;

    public RentalCalculatorTests()
    {
        _rentalCalculator = new RentalCalculator();
    }

    [Fact]
    public void CalculateTotalRentalPrice_OnTimeReturn_ReturnsCorrectPrice()
    {
        // Arrange
        var plannedReturnDate = DateTime.Now.AddDays(7);
        var actualReturnDate = plannedReturnDate;
        const decimal dailyRate = 30m;
        const int rentalPlanDays = 7;

        // Act
        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(plannedReturnDate, actualReturnDate, dailyRate, rentalPlanDays);

        // Assert
        totalPrice.Should().Be(210m);
    }

    [Fact]
    public void CalculateTotalRentalPrice_EarlyReturnWithPenalty7Days_ReturnsCorrectPrice()
    {
        // Arrange
        var plannedReturnDate = DateTime.Now.AddDays(7);
        var actualReturnDate = plannedReturnDate.AddDays(-2);
        const decimal dailyRate = 30m;
        const int rentalPlanDays = 7;

        // Act
        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(plannedReturnDate, actualReturnDate, dailyRate, rentalPlanDays);

        // Assert
        totalPrice.Should().Be(162m); // R$210 - R$60 unfulfilledDays + R$12 penalty
    }

    [Fact]
    public void CalculateTotalRentalPrice_EarlyReturnWithPenalty15Days_ReturnsCorrectPrice()
    {
        // Arrange
        var plannedReturnDate = DateTime.Now.AddDays(15);
        var actualReturnDate = plannedReturnDate.AddDays(-2);
        const decimal dailyRate = 28m;
        const int rentalPlanDays = 15;

        // Act
        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(plannedReturnDate, actualReturnDate, dailyRate, rentalPlanDays);

        // Assert
        totalPrice.Should().Be(386.40m); // R$420 - R$56 unfulfilledDays + R$22.40 penalty
    }

    [Fact]
    public void CalculateTotalRentalPrice_LateReturn_ReturnsCorrectPrice()
    {
        // Arrange
        var plannedReturnDate = DateTime.Now.AddDays(7);
        var actualReturnDate = plannedReturnDate.AddDays(2);
        const decimal dailyRate = 30m;
        const int rentalPlanDays = 7;

        // Act
        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(plannedReturnDate, actualReturnDate, dailyRate, rentalPlanDays);

        // Assert
        totalPrice.Should().Be(310m); // R$210 + R$100 for 2 additional days
    }

    [Fact]
    public void CalculateTotalRentalPrice_LateReturnWithAdditionalDays_ReturnsCorrectPrice()
    {
        // Arrange
        var plannedReturnDate = DateTime.Now.AddDays(7);
        var actualReturnDate = plannedReturnDate.AddDays(5);
        const decimal dailyRate = 18m;
        const int rentalPlanDays = 50;

        // Act
        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(plannedReturnDate, actualReturnDate, dailyRate, rentalPlanDays);

        // Assert
        totalPrice.Should().Be(1150m); // R$900 + R$250 for 5 additional days
    }
}