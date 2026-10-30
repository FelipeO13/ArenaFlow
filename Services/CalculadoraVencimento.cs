using System;
using System.Collections.Generic;
using System.Linq;
using BoxeGymManager.Models;

namespace BoxeGymManager.Services;

public record ResultadoPrimeiroVencimento(
    decimal ValorMensalidade,
    decimal ValorDiario,
    DateTime DataMatricula,
    DateTime DataReferencia,
    DateTime PrimeiroVencimento,
    int DiferencaDias,
    decimal ValorAjuste,
    decimal ValorPrimeiroPagamento
);

public static class CalculadoraVencimento
{
    // Centralização dos dias de vencimento permitidos no sistema
    public static readonly int[] DiasVencimentoPermitidos = new[] { 10, 25 };

    public static ResultadoPrimeiroVencimento CalcularPrimeiroVencimento(
        decimal valorMensalidade,
        int diaVencimento,
        DateTime dataMatricula)
    {
        // 1. Base mês comercial de 30 dias
        decimal valorDiario = Math.Round(valorMensalidade / 30m, 2);

        // 2. dataReferencia = data da matrícula + 1 mês (mesmo número de dia; se o mês seguinte não tiver esse dia, o último dia do mês)
        DateTime dataReferencia = dataMatricula.Date.AddMonths(1);

        // 3. Candidatas nos meses: anterior, atual e seguinte à dataReferencia
        var mesesCandidatos = new[]
        {
            dataReferencia.AddMonths(-1),
            dataReferencia,
            dataReferencia.AddMonths(1)
        };

        var candidatas = mesesCandidatos.Select(m =>
        {
            int diasNoMes = DateTime.DaysInMonth(m.Year, m.Month);
            int diaEfetivo = Math.Min(diaVencimento, diasNoMes);
            return new DateTime(m.Year, m.Month, diaEfetivo);
        }).ToList();

        // 4. Escolhe a mais próxima da dataReferencia
        DateTime primeiroVencimento = candidatas
            .OrderBy(c => Math.Abs((c - dataReferencia).TotalDays))
            .ThenByDescending(c => c >= dataReferencia) // desempate preferindo posterior
            .First();

        // 5. diferencaDias = primeiroVencimento - dataReferencia
        int diferencaDias = (int)Math.Round((primeiroVencimento - dataReferencia).TotalDays);

        // 6. valorPrimeiroPagamento = valorMensalidade + (diferencaDias * valorDiario)
        decimal valorAjuste = Math.Round(diferencaDias * valorDiario, 2);
        decimal valorPrimeiroPagamento = Math.Max(0, Math.Round(valorMensalidade + valorAjuste, 2));

        return new ResultadoPrimeiroVencimento(
            ValorMensalidade: valorMensalidade,
            ValorDiario: valorDiario,
            DataMatricula: dataMatricula.Date,
            DataReferencia: dataReferencia,
            PrimeiroVencimento: primeiroVencimento,
            DiferencaDias: diferencaDias,
            ValorAjuste: valorAjuste,
            ValorPrimeiroPagamento: valorPrimeiroPagamento
        );
    }

    public static DateTime CalcularProximaValidade(Student student, Plan plan)
    {
        var baseDate = student.PlanEndDate.HasValue && student.PlanEndDate.Value > DateTime.Today
            ? student.PlanEndDate.Value
            : (student.PlanEndDate ?? DateTime.Today);

        if (student.DiaVencimento.HasValue)
        {
            int day = student.DiaVencimento.Value;
            int monthsToAdd = plan.BillingCycle switch
            {
                PlanBillingCycle.Trimestral => 3,
                PlanBillingCycle.Semestral => 6,
                PlanBillingCycle.Anual => 12,
                _ => 1
            };

            var targetMonth = baseDate.AddMonths(monthsToAdd);
            int daysInTargetMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
            return new DateTime(targetMonth.Year, targetMonth.Month, Math.Min(day, daysInTargetMonth));
        }

        // Caso o aluno não tenha DiaVencimento (aluno legado), mantém o comportamento anterior
        var fallbackBase = student.PlanEndDate.HasValue && student.PlanEndDate.Value > DateTime.Today 
            ? student.PlanEndDate.Value 
            : DateTime.Today;

        return plan.BillingCycle switch
        {
            PlanBillingCycle.Trimestral => fallbackBase.AddMonths(3),
            PlanBillingCycle.Semestral => fallbackBase.AddMonths(6),
            PlanBillingCycle.Anual => fallbackBase.AddYears(1),
            _ => fallbackBase.AddMonths(1)
        };
    }
}
