using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.Attributes;

public class OptionalEmailAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        // Si el valor es null o vacío, es válido (es opcional)
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return true;
        }

        // Si tiene valor, validar que sea un email válido
        var emailAttribute = new EmailAddressAttribute();
        return emailAttribute.IsValid(value);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"El campo {name} debe ser una dirección de correo electrónico válida.";
    }
}
