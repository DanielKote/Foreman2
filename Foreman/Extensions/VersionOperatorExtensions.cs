namespace Foreman
{
    public static class VersionOperatorExtensions
    {
        public static string Token(this VersionOperator value)
        {
            switch (value)
            {
                case VersionOperator.EqualTo:
                    return "=";
                case VersionOperator.GreaterThan:
                    return ">";
                case VersionOperator.GreaterThanOrEqual:
                    return ">=";
                case VersionOperator.LessThan:
                    return "<";
                case VersionOperator.LessThanOrEqual:
                    return "<=";
                default:
                    return "";
            }
        }
    }
}
