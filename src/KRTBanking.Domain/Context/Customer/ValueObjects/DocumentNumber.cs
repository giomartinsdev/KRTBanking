namespace KRTBanking.Domain.Context.Customer.ValueObjects;

using System.Text;
using System.Text.RegularExpressions;
using BankingProject.Domain.Abstractions;

public sealed class DocumentNumber : IValueObject, IEquatable<DocumentNumber>
{
    private static readonly Regex CpfPattern = new(@"^\d{11}$", RegexOptions.Compiled);
    private const int CpfLength = 11;
    public string Value { get; }

    public DocumentNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Document number cannot be null or empty.", nameof(value));
        }

        var cleanedValue = CleanValue(value);
        
        if (!IsValidFormat(cleanedValue))
        {
            throw new ArgumentException($"Invalid document number format. Expected 11 digits, got: {value}", nameof(value));
        }

        if (!IsValidCpf(cleanedValue))
        {
            throw new ArgumentException($"Invalid CPF document number: {value}", nameof(value));
        }

        Value = cleanedValue;
    }

    public static DocumentNumber? TryCreate(string value)
    {
        try
        {
            return new DocumentNumber(value);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static implicit operator string(DocumentNumber documentNumber) => documentNumber?.Value ?? string.Empty;

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Value) || Value.Length != CpfLength)
        {
            return Value;
        }

        return $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..11]}";
    }


    public override bool Equals(object? obj)
    {
        return Equals(obj as DocumentNumber);
    }

    public bool Equals(DocumentNumber? other)
    {
        return other is not null && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public static bool operator ==(DocumentNumber? left, DocumentNumber? right)
    {
        return EqualityComparer<DocumentNumber>.Default.Equals(left, right);
    }

    public static bool operator !=(DocumentNumber? left, DocumentNumber? right)
    {
        return !(left == right);
    }

    private static string CleanValue(string value)
    {
        return value.Replace(".", "").Replace("-", "").Replace(" ", "");
    }

    private static bool IsValidFormat(string value)
    {
        return !string.IsNullOrEmpty(value) && 
               value.Length == CpfLength && 
               CpfPattern.IsMatch(value);
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.All(c => c == cpf[0]))
        {
            return false;
        }

        int sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += (cpf[i] - '0') * (10 - i);
        }

        var remainder = sum % 11;
        var firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        if (firstCheckDigit != (cpf[9] - '0'))
        {
            return false;
        }

        sum = 0;
        for (var i = 0; i < 10; i++)
        {
            sum += (cpf[i] - '0') * (11 - i);
        }

        remainder = sum % 11;
        var secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

        return secondCheckDigit == (cpf[10] - '0');
    }
}