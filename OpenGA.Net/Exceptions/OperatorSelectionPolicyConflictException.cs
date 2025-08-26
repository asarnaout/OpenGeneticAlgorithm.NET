namespace OpenGA.Net.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict between custom operator weights and the selected operator selection policy.
/// 
/// This exception is raised when operators have custom weights configured but a non-CustomWeight operator
/// selection policy is applied, which would ignore the specified weights and potentially confuse users
/// about the expected behavior.
/// </summary>
public class OperatorSelectionPolicyConflictException : Exception
{
    public OperatorSelectionPolicyConflictException()
    {
    }

    public OperatorSelectionPolicyConflictException(string message)
        : base(message)
    {
    }

    public OperatorSelectionPolicyConflictException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
