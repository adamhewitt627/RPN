namespace PolishNotation;

internal record struct ElementaryUnit(ElementaryUnitType Type, string Value);

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