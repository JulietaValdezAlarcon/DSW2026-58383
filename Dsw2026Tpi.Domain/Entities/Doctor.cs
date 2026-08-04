namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? SpecialityId { get; private set; }
    public Speciality? Speciality { get; private set; }
    public List<Availability> Availabilities { get; private set; } = new List<Availability>();

    #region Constructor for EF
#pragma warning disable CS8618
    public Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(
        string name,
        string licenseNumber,
        Speciality speciality,
        Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality.Id;
        IsActive = true;
    }

    public void Update(
        string name,
        string licenseNumber,
        Speciality speciality)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        SpecialityId = speciality.Id;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}