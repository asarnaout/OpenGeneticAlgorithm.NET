
namespace OpenGA.Net.OperatorSelectionPolicies;

public class FirstChoicePolicy : OperatorSelectionPolicy
{
    private IList<BaseOperator> _operators = [];

    public override void ApplyOperators(IList<BaseOperator> operators)
    {
        _operators = operators;
    }

    public override BaseOperator SelectOperator(Random random)
    {
        if (_operators.Count == 0)
        {
            throw new InvalidOperationException("No operators available for selection.");
        }

        return _operators[0];
    }
}
