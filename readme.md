# BlazorStore

Library for managing data in Blazor app. It re-render only those component, that uses changed data.

## Using

To use this library you need to add nuGet package. Use NuGet package manager or following command:

```bash
dotnet add package Dumba.BlazorStore
```

### Declaration

```csharp
// This can be any data class
public class State
{
  public User? User { get; set; }
  public string? Url { get; set; }
}

// definition of store
public class Store : BlazorStore<State>
{
}
```

### Usage in components

```csharp
@inject Store Store

<div>@UserName</div>

@code
{
  private string UserName
  {
    get => Store.GetOrDefault(s => s.User.Name, ChangedState);
    set => Store.Set(s => s.User.Name, value);
  }
}
```

There are 3 main methods:
1. Get - returns value, can throw null-reference
2. GetOrDefault - returns value, if there are any null, returns default (in expressions there cannot be User?.Name)
3. Set - updates value and re-render dependent components

### Optional: Store actions

When you have some actions composed by multiple actions, you can create store method

```csharp
public class Store : BlazorStore<State>
{
  public void Login(User user)
  {
    Set(s => s.User, user);
    Set(s => s.Url, "dashboard");
  }
}
```

### Common Mistakes

```csharp
// DO NOT USE THIS
private string UserName
{
  get => Store.GetOrDefault(s => s.User, StateChanged)?.Name;
  // this component will be re-rendered 
  // when any User property will change.
  // It is useless
  
  set
  {
    var user = Store.GetOrDefault(s => s.User);
    user.Name = value;
  }
  // This will not re-render dependent component. 
  // Store data should be updated only through Set method.
  
  set
  {
    var user = Store.GetOrDefault(s => s.User);
    user.Name = value;
    Store.Set(s => s.User, user);
  }
  // This will re-render all components that use any user
  // property. It is useless
}
```

### More features

You can handle any data change by your own using `OnChange` method.

```csharp
store.OnChange(
  s => s.User.Age, 
  (newValue, oldValue, changeExpression) =>
  {
    Console.WriteLine("This will be executed anytime Age or User changes");
  });
```

Your OnChange method is called even if any containing object is changed. When previous example continues with following code, method will be executed twice, even if newValue and oldValue are equal. 

```csharp
store.Set(s => s.User.Age, 13);
store.Set(s => s.User, new User { Age = 13 });
```
