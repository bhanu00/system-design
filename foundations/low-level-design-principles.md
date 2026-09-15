## Low-Level Design Principles in .NET

### **SOLID Principles**

#### **1. Single Responsibility Principle (SRP)**
- **Definition**: A class should have only one reason to change.
- **Explanation**: Each class should focus on a single responsibility or task. If a class handles multiple concerns, changes to one concern might inadvertently affect the others.
- **Example in .NET**:
```csharp
public class InvoiceService
{
    public void GenerateInvoice() { /* Logic for generating an invoice */ }
}

public class EmailService
{
    public void SendEmail(string recipient, string message) { /* Logic for sending email */ }
}
// Separate responsibilities for invoice generation and email sending.
```

---

#### **2. Open/Closed Principle (OCP)**
- **Definition**: Classes should be open for extension but closed for modification.
- **Explanation**: Extend functionality by adding new code rather than altering existing code, preserving the existing functionality.
- **Example in .NET**:
```csharp
public interface IDiscount
{
    decimal ApplyDiscount(decimal totalAmount);
}

public class NoDiscount : IDiscount
{
    public decimal ApplyDiscount(decimal totalAmount) => totalAmount;
}

public class PercentageDiscount : IDiscount
{
    private readonly decimal _percentage;
    public PercentageDiscount(decimal percentage) { _percentage = percentage; }
    public decimal ApplyDiscount(decimal totalAmount) => totalAmount * (1 - _percentage);
}
// New discount strategies can be added without modifying existing classes.
```

---

#### **3. Liskov Substitution Principle (LSP)**
- **Definition**: Subtypes must be substitutable for their base types.
- **Explanation**: Derived classes should not violate the expectations set by the base class. This ensures that the derived class can be used in place of the base class without altering the correctness of the program.

- **Key Aspects**:
  - A derived class should not remove behavior expected by the base class.
  - The derived class should honor contracts established by the base class, such as method signatures, invariants, and pre/post-conditions.

- **Benefits**:
  - Improves code reusability and consistency.
  - Reduces bugs caused by unexpected behaviors in derived classes.

- **Example in .NET (Violation)**:
```csharp
// Base class
public class Bird
{
    public virtual void Move()
    {
        Console.WriteLine("This bird is moving.");
    }
}

// Derived class: Sparrow
public class Sparrow : Bird
{
    public override void Move()
    {
        Console.WriteLine("Sparrow is flying.");
    }
}

// Derived class: Penguin
public class Penguin : Bird
{
    public override void Move()
    {
        throw new NotImplementedException("Penguins cannot move this way.");
    }
}

// Usage
public void DescribeMovement(Bird bird)
{
    bird.Move();
}

// Test cases
Bird sparrow = new Sparrow();
Bird penguin = new Penguin();
DescribeMovement(sparrow); // Output: "Sparrow is flying."
DescribeMovement(penguin); // Throws runtime exception
```

- **Explanation of Violation**:
  1. The base class `Bird` establishes the expectation that all birds can `Move` in some way.
  2. The `Penguin` class breaks this expectation by throwing an exception, violating the principle.
  3. The `DescribeMovement` method now depends on the specific implementation, which defeats the purpose of polymorphism.

- **Corrected Example**:
```csharp
// Base class
public abstract class Bird
{
    public abstract void Move();
}

// Derived class: Sparrow
public class Sparrow : Bird
{
    public override void Move()
    {
        Console.WriteLine("Sparrow is flying.");
    }
}

// Derived class: Penguin
public class Penguin : Bird
{
    public override void Move()
    {
        Console.WriteLine("Penguin is swimming.");
    }
}

// Correct usage
public void DescribeMovement(Bird bird)
{
    bird.Move();
}

// Test cases
Bird sparrow = new Sparrow();
Bird penguin = new Penguin();
DescribeMovement(sparrow); // Output: "Sparrow is flying."
DescribeMovement(penguin); // Output: "Penguin is swimming."
```

- **Explanation of Correctness**:
  1. The base class `Bird` is now abstract, requiring each derived class to define its own `Move` behavior.
  2. Each derived class adheres to the expected contract and provides meaningful implementations.
  3. The `DescribeMovement` method works correctly for all types of birds without any runtime exceptions.

- **Key Takeaway**: Avoid designing base classes with assumptions that may not hold for all derived types. Use abstraction to enforce behavior consistency.

---

#### **4. Interface Segregation Principle (ISP)**
- **Definition**: Clients should not be forced to depend on interfaces they don’t use.
- **Explanation**: Split large interfaces into smaller, more specific ones tailored to client needs.
- **Example in .NET**:
```csharp
public interface IPrinter
{
    void PrintDocument();
}

public interface IScanner
{
    void ScanDocument();
}

public class MultifunctionPrinter : IPrinter, IScanner
{
    public void PrintDocument() { /* Print logic */ }
    public void ScanDocument() { /* Scan logic */ }
}

public class SimplePrinter : IPrinter
{
    public void PrintDocument() { /* Print logic */ }
}
// Clients can depend only on the interfaces they require.
```

---

#### **5. Dependency Inversion Principle (DIP)**
- **Definition**: Depend on abstractions, not on concrete implementations.
- **Explanation**: High-level modules should not depend on low-level modules; both should depend on abstractions.
- **Example in .NET**:
```csharp
public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger
{
    public void Log(string message) { /* Write to file */ }
}

public class OrderService
{
    private readonly ILogger _logger;
    public OrderService(ILogger logger) { _logger = logger; }
    public void ProcessOrder() { _logger.Log("Order processed."); }
}
// Dependency injection can be used to pass the logger implementation.
```

---

### **Other Key Principles**

#### **1. DRY (Don't Repeat Yourself)**
- **Definition**: Avoid code duplication by abstracting reusable logic.
- **Example in .NET**:
```csharp
public static class TaxCalculator
{
    public static decimal CalculateTax(decimal amount) => amount * 0.1m;
}

public class Invoice
{
    public decimal Amount { get; set; }
    public decimal Tax => TaxCalculator.CalculateTax(Amount);
}

public class Order
{
    public decimal Amount { get; set; }
    public decimal Tax => TaxCalculator.CalculateTax(Amount);
}
// Reuse tax calculation logic.
```

---

#### **2. KISS (Keep It Simple, Stupid)**
- **Definition**: Design should be simple and easy to understand.
- **Example in .NET**:
```csharp
// Complex
public int Calculate(int a, int b, bool multiply)
{
    return multiply ? a * b : a + b;
}

// Simple
public int Add(int a, int b) => a + b;
public int Multiply(int a, int b) => a * b;
```

---

#### **3. YAGNI (You Aren't Gonna Need It)**
- **Definition**: Don’t implement features until they are necessary.
- **Example in .NET**:
```csharp
// Avoid preemptively adding unused features.
public class UserService
{
    public void RegisterUser(string name, string email) { /* Registration logic */ }
}
```

---

#### **4. Separation of Concerns (SoC)**
- **Definition**: Divide the application into distinct features that overlap minimally.
- **Example in .NET**:
```csharp
// Presentation Layer
public class HomeController : Controller
{
    private readonly IProductService _service;
    public HomeController(IProductService service) { _service = service; }
    public IActionResult Index() => View(_service.GetProducts());
}

// Business Logic Layer
public interface IProductService
{
    IEnumerable<Product> GetProducts();
}

public class ProductService : IProductService
{
    public IEnumerable<Product> GetProducts() { /* Fetch products */ }
}

// Data Access Layer
public class ProductRepository
{
    public IEnumerable<Product> GetAll() { /* Database logic */ }
}
```

---

#### **5. Encapsulation**
- **Definition**: Limit the exposure of internal state and behavior.
- **Example in .NET**:
```csharp
public class BankAccount
{
    private decimal _balance;
    public decimal GetBalance() => _balance;
    public void Deposit(decimal amount) { _balance += amount; }
    public void Withdraw(decimal amount) { if (amount <= _balance) _balance -= amount; }
}
// Internal balance is hidden, and controlled methods are exposed.
```

---

#### **6. Law of Demeter (LoD)**
- **Definition**: A module should only interact with its immediate dependencies.
- **Example in .NET**:
```csharp
// Violation
public string GetCustomerZip(Order order) => order.Customer.Address.ZipCode;

// Following LoD
public string GetCustomerZip(Order order) => order.GetCustomerZipCode();

public class Order
{
    public Customer Customer { get; set; }
    public string GetCustomerZipCode() => Customer?.Address?.ZipCode;
}
```

# Detailed Explanation of Liskov Substitution Principle (LSP)** 
— what it is, why it matters, and how to follow it effectively in **C#**, with examples and real-world implications.


## What is Liskov Substitution Principle?

**Liskov Substitution Principle (LSP)** is the **"L"** in **SOLID** principles. It states:

> **"Objects of a superclass should be replaceable with objects of a subclass without affecting the correctness of the program."**

In other words:

* If `class B` is a subclass of `class A`, then we should be able to use `B` **wherever** we use `A`—without breaking functionality.


## Simple Definition

> **Subtypes must behave like their parent types.**

That means:

* Subclasses must **honor the contract** of the base class or interface.
* They **should not throw new exceptions**, **remove functionality**, or **change meaning** of existing behavior.


## Realistic Example – LSP Followed

```csharp
public abstract class Bird
{
    public abstract void Fly();
}

public class Sparrow : Bird
{
    public override void Fly()
    {
        Console.WriteLine("Sparrow flies");
    }
}
```

Usage:

```csharp
Bird bird = new Sparrow();
bird.Fly(); // Works fine
```

Sparrow behaves just like a Bird should.

---

## ❌ Violation of LSP (Bad Subclassing)

Now imagine:

```csharp
public class Ostrich : Bird
{
    public override void Fly()
    {
        throw new NotSupportedException("Ostrich cannot fly!");
    }
}
```

```csharp
Bird bird = new Ostrich();
bird.Fly(); // ❌ Runtime exception!
```

**Why this violates LSP:**

* `Bird` promises `Fly()` will work.
* `Ostrich` breaks that promise by throwing.
* Any code depending on `Bird.Fly()` is now fragile and error-prone.


## ✅ Refactored with Interface Segregation

Split the design properly:

```csharp
public interface IBird { }

public interface IFlyingBird : IBird
{
    void Fly();
}

public class Sparrow : IFlyingBird
{
    public void Fly() => Console.WriteLine("Sparrow flies");
}

public class Ostrich : IBird
{
    // Ostrich does not fly, no Fly() here
}
```

Now:

* No false promises.
* LSP is preserved.
* Code is more robust and flexible.

---

## C# Strategy Pattern Example (Correct Way)

Instead of subclassing with different behaviors:

```csharp
public interface IShippingStrategy
{
    decimal CalculateShipping(Order order);
}

public class StandardShipping : IShippingStrategy
{
    public decimal CalculateShipping(Order order) => 5.00m;
}

public class ExpressShipping : IShippingStrategy
{
    public decimal CalculateShipping(Order order) => 15.00m;
}

public class FreeShipping : IShippingStrategy
{
    public decimal CalculateShipping(Order order) => 0.0m; // ✅ Safe, no surprises
}
```

### Using it:

```csharp
Order order = new Order { OrderName = "Book" };
IShippingStrategy shipping = new FreeShipping(); // or ExpressShipping

var cost = shipping.CalculateShipping(order);
```

Each strategy honors the same contract — **no violations**.

---

## 🔥 Signs of LSP Violation

Watch out for:

| ❌ Pattern                                             | Why it's a problem                          |
| ----------------------------------------------------- | ------------------------------------------- |
| Subclass throws NotImplementedException               | You're lying about supporting base behavior |
| Subclass returns unexpected values (null, -1)         | Breaks client expectations                  |
| If statements like `if (obj is Subclass)`             | Violates polymorphism — fragile code        |
| Overridden methods change behavior in surprising ways | Leads to bugs in consuming code             |

---

## Tips to Follow LSP

| Principle                         | What to Do                                                              |
| --------------------------------- | ----------------------------------------------------------------------- |
| **Design by contract**            | Ensure all subclasses uphold the promises of their base                 |
| **Use interfaces or composition** | Avoid inheriting behavior that doesn’t apply                            |
| **Don't override just to throw**  | Redesign or segregate your abstraction                                  |
| **Program to abstractions**       | Use interfaces to isolate behavior like flying, drawing, shipping, etc. |


## Mental Model

Think of LSP like this:

> If `X` is a type, and `Y` is a subtype of `X`, you should be able to replace `X` with `Y` **and not know the difference**.

---

## Summary

* ✅ **LSP ensures safe inheritance or interface implementation**
* ❌ Violating LSP leads to fragile, error-prone code
* ✅ Use **interface segregation** or **composition** when behaviors vary
* ✅ Think from the **consumer’s perspective**: “Will this substitution surprise them?”

---

Would you like to see a **.NET application layer (Service, Controller, Strategy) example** that correctly applies LSP in real-world architecture?


By following these principles, you can create maintainable, scalable, and robust .NET applications.

