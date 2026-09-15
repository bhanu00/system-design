## Low-Level Design Patterns in .NET

### **Design Patterns**

#### **Creational Patterns**

---

### **1. Singleton Pattern**
**Definition**: Ensures that a class has only one instance and provides a global access point to it. This is useful when exactly one object is needed to coordinate actions across the system.

**Explanation**: The Singleton pattern restricts the instantiation of a class to a single object and provides a global point of access to it. This is achieved by making the constructor private and exposing a static instance property.

**Thread-Safe Singleton in .NET**:
```csharp
public sealed class Singleton
{
    private static readonly Lazy<Singleton> _instance = new Lazy<Singleton>(() => new Singleton());
    public static Singleton Instance => _instance.Value;

    private Singleton() 
    { 
        // Private constructor prevents instantiation from outside. 
    }

    public void Log(string message)
    {
        Console.WriteLine($"Log: {message}");
    }
}
```

**Use Cases**:
- Configuration management.
- Logging frameworks.
- Caching mechanisms.
- Database connection pools.

---

### **2. Factory Method Pattern**
**Definition**: Defines a method for creating objects, but allows subclasses to alter the type of objects that will be created. 

**Explanation**: The Factory Method pattern delegates the responsibility of object creation to subclasses by defining a method in the base class. Subclasses override this method to provide specific implementations.

**Example in .NET**:
```csharp
public abstract class Logger
{
    public abstract void Log(string message);
}

public class FileLogger : Logger
{
    public override void Log(string message) => Console.WriteLine($"FileLogger: {message}");
}

public class DatabaseLogger : Logger
{
    public override void Log(string message) => Console.WriteLine($"DatabaseLogger: {message}");
}

public abstract class LoggerFactory
{
    public abstract Logger CreateLogger();
}

public class FileLoggerFactory : LoggerFactory
{
    public override Logger CreateLogger() => new FileLogger();
}

public class DatabaseLoggerFactory : LoggerFactory
{
    public override Logger CreateLogger() => new DatabaseLogger();
}

// Usage
LoggerFactory factory = new FileLoggerFactory();
Logger logger = factory.CreateLogger();
logger.Log("Factory Method Example");
```

**Use Cases**:
- Choosing between different algorithms or strategies.
- Decoupling object creation from client code.
- Plug-in architectures.

---

### **3. Abstract Factory Pattern**
**Definition**: Provides an interface for creating families of related or dependent objects without specifying their concrete classes.

**Explanation**: The Abstract Factory pattern is used when a system needs to create objects that belong to different families or categories. It ensures that related objects are created together.

**Example in .NET**:
```csharp
public interface IButton
{
    void Render();
}

public class WindowsButton : IButton
{
    public void Render() => Console.WriteLine("Rendering Windows Button");
}

public class MacButton : IButton
{
    public void Render() => Console.WriteLine("Rendering Mac Button");
}

public interface IUIFactory
{
    IButton CreateButton();
}

public class WindowsFactory : IUIFactory
{
    public IButton CreateButton() => new WindowsButton();
}

public class MacFactory : IUIFactory
{
    public IButton CreateButton() => new MacButton();
}

// Usage
IUIFactory factory = new WindowsFactory();
IButton button = factory.CreateButton();
button.Render();
```

**Use Cases**:
- Cross-platform applications.
- Families of related objects, like GUI components (e.g., buttons, textboxes).
- Decoupling client code from concrete implementations.

---

### **Differences Between Factory Method and Abstract Factory**

| **Aspect**             | **Factory Method**                                                    | **Abstract Factory**                                                    |
|------------------------|----------------------------------------------------------------------|------------------------------------------------------------------------|
| **Purpose**            | Creates one type of object at a time.                               | Creates families of related objects.                                   |
| **Hierarchy**          | Focuses on a single product creation and relies on inheritance.     | Focuses on creating multiple related products and relies on interfaces.|
| **Flexibility**        | Less flexible, handles one product family.                         | More flexible, handles multiple product families.                      |
| **Example Use Case**   | Choosing between FileLogger and DatabaseLogger.                    | Creating UI elements (buttons, textboxes) for Windows or Mac.          |

---

### **4. Builder Pattern**
**Definition**: Separates the construction of a complex object from its representation, allowing the same construction process to create different representations.

**Explanation**: The Builder pattern is useful when creating an object involves multiple steps or configurations. It helps construct complex objects step-by-step and can produce different representations using the same construction process.

**Example in .NET**:
```csharp
public class Car
{
    public string Engine { get; set; }
    public int Wheels { get; set; }
    public string Color { get; set; }

    public override string ToString() => $"Engine: {Engine}, Wheels: {Wheels}, Color: {Color}";
}

public class CarBuilder
{
    private readonly Car _car = new();

    public CarBuilder SetEngine(string engine)
    {
        _car.Engine = engine;
        return this;
    }

    public CarBuilder SetWheels(int wheels)
    {
        _car.Wheels = wheels;
        return this;
    }

    public CarBuilder SetColor(string color)
    {
        _car.Color = color;
        return this;
    }

    public Car Build() => _car;
}

// Usage
Car car = new CarBuilder()
    .SetEngine("V8")
    .SetWheels(4)
    .SetColor("Red")
    .Build();

Console.WriteLine(car);
```

**Use Cases**:
- Constructing complex objects with many optional properties or multiple configurations.
- Avoiding constructor overloading when an object has many parameters.
- Creating immutable objects step-by-step.

---

### **5. Prototype Pattern**
**Definition**: Creates new objects by copying existing ones.

**Explanation**: The Prototype pattern allows you to create a new object by cloning an existing object. This is especially useful when object creation is resource-intensive or when creating objects with similar states.

**Example in .NET**:
```csharp
public class Employee : ICloneable
{
    public string Name { get; set; }
    public string Department { get; set; }

    public object Clone()
    {
        return new Employee
        {
            Name = this.Name,
            Department = this.Department
        };
    }
}

// Usage
Employee original = new Employee { Name = "John Doe", Department = "IT" };
Employee clone = (Employee)original.Clone();
clone.Name = "Jane Doe";

Console.WriteLine($"Original: {original.Name}, {original.Department}");
Console.WriteLine($"Clone: {clone.Name}, {clone.Department}");
```

**Use Cases**:
- Creating objects that are expensive to instantiate (e.g., deep copying).
- Avoiding the complexity of building objects from scratch.
- Situations where object properties are nearly identical but require slight modifications.

---

### **Comparison of Use Cases**

| Pattern           | Use Case Scenarios                                                                                   |
|-------------------|-----------------------------------------------------------------------------------------------------|
| **Singleton**     | Shared resources like logging, configuration settings, or thread pools.                             |
| **Factory Method**| When a class doesn't know what objects it needs to create or wants to delegate creation to subclasses.|
| **Abstract Factory**| When families of related objects need to be created (e.g., UI elements for different platforms).   |
| **Builder**       | Constructing complex objects with many optional properties or multiple configurations.               |
| **Prototype**     | Duplicating objects where instantiation is costly or cumbersome.                                    |

These patterns collectively address different needs in object creation, making your code more robust, modular, and maintainable.

