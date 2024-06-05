# BlazorStore

Library for managing data in Blazor app. It re-render only those component, that uses changed data.

## Using

To use this library you need to add nuGet package. Use NuGet package manager or following command:

```bash
dotnet add package DriDrood.BlazorStore
```

Build-in methods:
1. Get - returns value, can throw null-reference
2. GetOrDefault - returns value, if there are any null, returns default (in expressions there cannot be User?.Name)
3. Set - updates value and re-render dependent components
4. Add - add value to collection or dictionary and re-render dependent components
5. Remove - remove value from collection or dictionary and re-render dependent components

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
