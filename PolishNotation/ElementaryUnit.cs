namespace PolishNotation;

internal record ElementaryUnit(ElementaryUnitType Type, string Value);

internal enum ElementaryUnitType
{
    Digit,
    BinaryOperation,
    UnaryOperation,
    UnaryFunction,
    BinaryFunction,
    Brackets,
    Variable,
    Constant
}
