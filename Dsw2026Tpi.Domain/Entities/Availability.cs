namespace Dsw2026Tpi.Domain.Entities;

public class Availability : EntityBase
{
    public DateOnly Date { get; init; }
    public int Month => Date.Month;
    public int Year => Date.Year;
    public DayOfTheWeak DayOfTheWeak { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public List<Turn> Turns { get; set; } = new List<Turn>();
    public Guid DoctorId { get; set; }
}
