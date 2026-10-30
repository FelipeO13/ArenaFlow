using System;
using System.Collections.Generic;
using System.Linq;
using BoxeGymManager.Models;

namespace BoxeGymManager.Services;

public record ClassOptionDto(
    int Id,
    string DisplayText,
    bool IsFull,
    int Occupied,
    int Capacity,
    bool IsCurrent
);

public static class ScheduleHelper
{
    public static string FormatDayOfWeek(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Segunda",
        DayOfWeek.Tuesday => "Terça",
        DayOfWeek.Wednesday => "Quarta",
        DayOfWeek.Thursday => "Quinta",
        DayOfWeek.Friday => "Sexta",
        DayOfWeek.Saturday => "Sábado",
        _ => "Domingo"
    };

    public static string FormatDayOfWeekFull(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Segunda-feira",
        DayOfWeek.Tuesday => "Terça-feira",
        DayOfWeek.Wednesday => "Quarta-feira",
        DayOfWeek.Thursday => "Quinta-feira",
        DayOfWeek.Friday => "Sexta-feira",
        DayOfWeek.Saturday => "Sábado",
        _ => "Domingo"
    };

    public static int GetDayOrder(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        DayOfWeek.Saturday => 6,
        _ => 7
    };

    public static ClassOptionDto ToOptionDto(BoxingClass c, int? currentStudentId = null)
    {
        int occupied = c.Students.Count;
        bool isCurrent = currentStudentId.HasValue && c.Students.Any(s => s.Id == currentStudentId.Value);
        bool isFull = occupied >= c.MaxCapacity && !isCurrent;
        
        string vagasText = isFull ? $"{occupied}/{c.MaxCapacity} vagas (Lotada)" : $"{occupied}/{c.MaxCapacity} vagas";
        string text = $"{FormatDayOfWeek(c.DayOfWeek)} {c.StartTime:hh\\:mm} às {c.EndTime:hh\\:mm} · {c.ModalityName} · {vagasText}";

        return new ClassOptionDto(
            Id: c.Id,
            DisplayText: text,
            IsFull: isFull,
            Occupied: occupied,
            Capacity: c.MaxCapacity,
            IsCurrent: isCurrent
        );
    }

    public static string FormatClassSummary(BoxingClass c)
    {
        var day = FormatDayOfWeek(c.DayOfWeek);
        var occupied = c.Students?.Count ?? 0;
        return $"{day} {c.StartTime:hh\\:mm} às {c.EndTime:hh\\:mm} · {c.ModalityName} · {occupied}/{c.MaxCapacity} vagas";
    }
}
