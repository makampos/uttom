using System.ComponentModel.DataAnnotations;

namespace Uttom.Domain.Models;

public class Rental : TrackableEntity
{
    private Rental(int planId, DateOnly endDate, DateOnly estimatingEndingDate, int delivererId, int motorcycleId)
    {
        PlanId = planId;
        EndDate = endDate;
        EstimatingEndingDate = estimatingEndingDate;
        DelivererId = delivererId;
        MotorcycleId = motorcycleId;
    }

    [Required]
    public int PlanId { get; private set; }

    [Required] public DateOnly StartDate { get; private set; } = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

    [Required]
    public DateOnly EndDate { get; private set; }

    [Required]
    public DateOnly  EstimatingEndingDate{ get; private set; }

    public Deliverer Deliverer { get; set; }

    public Motorcycle Motorcycle { get; set; }

    [Required]
    public int DelivererId { get; private set; }

    [Required]
    public int MotorcycleId { get; private set; }

    public DateOnly? ReturnDate { get; private set; } = null;

    public static Rental Create(int planId, DateOnly endDate, DateOnly estimatingEndingDate, int delivererId, int motorcycleId)
    {
        return new Rental(planId, endDate, estimatingEndingDate, delivererId, motorcycleId);
    }

    public void UpdateReturnDate(DateOnly returnDate)
    {
        ReturnDate = returnDate;
    }
}