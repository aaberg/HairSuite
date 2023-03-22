﻿namespace HairSuite.Domain;

public class Reservation : Aggregate
{
    public Guid Id
    {
        get => ReservationId.Value;
        set {}
    }
    public ReservationId ReservationId { get; set; }
    public HairdresserId UserId { get; set; }
    public ReservationDate Date { get; set; }
    public ReservationStatus Status { get; set; }

    public Reservation() { } // For serialization

    private Reservation(Guid id, Guid hairdresserId, DateTime date)
    {
        HandleEvent(new Events.ReservationRequested(id, hairdresserId, date), Apply);
    }

    public static Reservation MakeTentative(Guid id, Guid hairdresserId, DateTime date) => new(id, hairdresserId, date);

    public void Confirm(Guid id)
    {
        if (Status != ReservationStatus.Tentative)
        {
            throw new DomainException("Only requested reservations can be confirmed.");
        }
        HandleEvent(new Events.ReservationConfirmed(id), Apply);
    }

    public void Cancel(Guid id)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new DomainException("Can not cancel a reservation that is already cancelled.");
        }
        HandleEvent(new Events.ReservationCancelled(id), Apply);
    }

    public void Reschedule(Guid id, DateTime date)
    {
        HandleEvent(new Events.ReservationRescheduled(id, date), Apply);
    }

    public void Apply(Events.ReservationRequested @event)
    {
        ReservationId = new ReservationId(@event.Id);
        UserId = new HairdresserId(@event.HairdresserId);
        Date = new ReservationDate(@event.Date);
        Status = ReservationStatus.Tentative;
    }

    public void Apply(Events.ReservationConfirmed _) => Status = ReservationStatus.Booked;

    public void Apply(Events.ReservationCancelled _) => Status = ReservationStatus.Cancelled;

    public void Apply(Events.ReservationRescheduled @event) => Date = new ReservationDate(@event.Date);
}

public record ReservationId(Guid Value) : AggregateId(Value);

public record HairdresserId(Guid Value);

public record ReservationDate(DateTime Value);

public enum ReservationStatus
{
    Tentative,
    Booked,
    Cancelled
}

public static class Events
{
    public record ReservationRequested(Guid Id, Guid HairdresserId, DateTime Date);

    public record ReservationConfirmed(Guid Id);

    public record ReservationCancelled(Guid Id);

    public record ReservationRescheduled(Guid Id, DateTime Date);
}
