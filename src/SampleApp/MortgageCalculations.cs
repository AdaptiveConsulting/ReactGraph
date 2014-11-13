using System;

namespace SampleApp
{
    public class MortgageCalculations
    {
        public static double? CalculatePayments(
            double? loanAmount,
            double? interestRate,
            double? loanLength,
            PaymentFrequency? paymentFrequency)
        {
            if (loanAmount == null || interestRate == null || loanLength == null || paymentFrequency == null)
                return null;

            var loanAmountValue = loanAmount.Value;
            var interestRateValue = interestRate.Value;
            var paymentFrequencyValue = paymentFrequency.Value;
            var numberPayments = loanLength.Value * 12;

            interestRateValue /= 1200.00;
            var monthlyPayment = -((-loanAmountValue * Math.Pow(1 + interestRateValue, numberPayments)) / (((Math.Pow((1 + interestRateValue), numberPayments) - 1) / interestRateValue))) * 12 / (int)paymentFrequencyValue;
            return Math.Round(monthlyPayment, 2);
        }

        public static double? CalculateLength(
            double? loanAmount,
            double? paymentAmount,
            double? interestRate,
            PaymentFrequency? paymentFrequency)
        {
            if (loanAmount == null || interestRate == null || paymentAmount == null || paymentFrequency == null)
                return null;

            var interestRateValue = interestRate.Value;
            var loanAmountValue = loanAmount.Value;
            var paymentAmountValue = paymentAmount.Value;
            var paymentFrequencyValue = paymentFrequency.Value;

            interestRateValue /= 1200.00;
            loanAmountValue *= (-1);

            var monthlyPayment = paymentAmountValue / 12 * (int)paymentFrequencyValue;

            return Math.Round(Math.Log(monthlyPayment / (loanAmountValue * interestRateValue + monthlyPayment)) / Math.Log(1 + interestRateValue) / 12, 3);
        }
    }
}