using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

public class AppointmentServiceTests
{
    private readonly IPersistence _persistenceMock;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        // Creamos el substitute (mock) con NSubstitute
        _persistenceMock = Substitute.For<IPersistence>();
        _appointmentService = new AppointmentService(_persistenceMock);
    }

    [Fact]
    public async Task CreateAppointment_WhenTurnIsNotAvailable_ShouldThrowConflictException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        var requestDto = new AppointmentDto.Request(
            doctorId,
            turnId,
            new PatientDto.patientDni(40123456),
            "Dolor de garganta"
        );

        _persistenceMock.GetById<Doctor>(doctorId).Returns(Task.FromResult(new Doctor()));
        _persistenceMock.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(Task.FromResult(new Patient()));

        var busyTurn = new Turn { Id = turnId, Status = TurnStatus.BOOKED };
        _persistenceMock.GetById<Turn>(turnId).Returns(Task.FromResult(busyTurn));

        // Act
        Func<Task> act = async () => await _appointmentService.CreateAppointment(requestDto);

        // Assert (Usando WithMessage en lugar de Where para evitar desajustes)
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("APPOINTMENT_CONFLICT");
    }

    [Fact]
    public async Task CreateAppointment_WhenDoctorDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var requestDto = new AppointmentDto.Request(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new PatientDto.patientDni(40123456),
            "Control médico"
        );

        // Simulamos que el doctor NO existe (retorna null)
        _persistenceMock.GetById<Doctor>(requestDto.DoctorId).Returns(Task.FromResult<Doctor>(null));

        // Act
        Func<Task> act = async () => await _appointmentService.CreateAppointment(requestDto);

        // Assert
        await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage("*Doctor Not Found*");
    }

    [Fact]
    public async Task CreateAppointment_WhenPatientDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var requestDto = new AppointmentDto.Request(
            doctorId,
            Guid.NewGuid(),
            new PatientDto.patientDni(40123456),
            "Control médico"
        );

        _persistenceMock.GetById<Doctor>(doctorId).Returns(Task.FromResult(new Doctor()));

        // Simulamos que el paciente NO existe (retorna null)
        _persistenceMock.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(Task.FromResult<Patient>(null));

        // Act
        Func<Task> act = async () => await _appointmentService.CreateAppointment(requestDto);

        // Assert
        await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage("*Patient Not Found*");
    }

    [Fact]
    public async Task CreateAppointment_WhenAllDataIsValid_ShouldCreateAppointmentSuccessfully()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        var availabilityId = Guid.NewGuid();

        var requestDto = new AppointmentDto.Request(
            doctorId,
            turnId,
            new PatientDto.patientDni(40123456),
            "Consulta general"
        );

        _persistenceMock.GetById<Doctor>(doctorId).Returns(Task.FromResult(new Doctor()));
        _persistenceMock.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>())
            .Returns(Task.FromResult(new Patient { Id = Guid.NewGuid() }));

        // Turno disponible (Status = NO_SHOW según tu lógica)
        var validTurn = new Turn { Id = turnId, Status = TurnStatus.NO_SHOW, AvailabilityId = availabilityId };
        _persistenceMock.GetById<Turn>(turnId).Returns(Task.FromResult(validTurn));

        var availability = new Availability(
            doctorId,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(8, 0),  // startTime
            new TimeOnly(8, 30), // endTime (exactamente 30 minutos después)
            availabilityId       // (o el orden de parámetros que tenga tu constructor)
        );
        _persistenceMock.GetById<Availability>(availabilityId).Returns(Task.FromResult(availability));

        // Simulamos la transacción ejecutando el delegado que se le pasa
        _persistenceMock.When(x => x.ExecuteInTransaction(Arg.Any<Func<Task>>()))
            .Do(callInfo => {
                var action = callInfo.Arg<Func<Task>>();
                action().Wait(); // Ejecuta el bloque interno de la transacción
            });

        // Act
        var result = await _appointmentService.CreateAppointment(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.DateOfService.Should().Be(availability.Date);
        validTurn.Status.Should().Be(TurnStatus.BOOKED);
    }

}