namespace PolishNotation
{
    using Enums;

    internal class ElementaryUnit
    {
        public ElementaryUnitType Type { get; }

        public string Value { get; }

        public ElementaryUnit(ElementaryUnitType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}
