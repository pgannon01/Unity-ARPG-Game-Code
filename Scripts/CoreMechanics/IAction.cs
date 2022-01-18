namespace RPG.Core 
{
    public interface IAction 
    {
        // Interfaces are similar to classes, but using the keyword interface instead of class
        // Interfaces are a contract, anything that implements this interface HAS to have a certain method
        void Cancel(); // Don't have to define this as public because everything in an interface is public because there's no implementation in the interface
        // Can't have any bodies to any functions inside an interface, can only be methods or properties 
        // Meant to take things defined in an interface and implement them in OTHER classes (Similar to a Header file/Interfaces in C++)
        // NOTE, CAN have arguments and take parameters, just can't give it a body or define it
    }
}