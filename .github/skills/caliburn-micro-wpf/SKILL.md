---
name: caliburn-micro-wpf
description: Use whenever writing, reviewing, or troubleshooting WPF code that uses Caliburn.Micro (v4.x) - bootstrapper, SimpleContainer/IoC, ViewModel/View naming conventions, Screen/Conductor, actions (cal:Message.Attach), guard (Can*) properties, IWindowManager dialogs, and logging. Based on the official Caliburn.Micro WPF tutorial (https://caliburnmicro.com/documentation/Tutorials/WPF/Contents).
user-invocable: true
---

# Caliburn.Micro for WPF

Caliburn.Micro is an opinionated, convention-based MVVM framework. Prefer
conventions over explicit bindings and code-behind. Target the current v4.x API
(async methods: `ActivateItemAsync`, `ShowDialogAsync`, `TryCloseAsync`, ...).

Reference: https://caliburnmicro.com/documentation/Tutorials/WPF/Contents
Tutorial code: https://github.com/Caliburn-Micro/Caliburn.Micro/tree/master/samples/tutorals/WPF/Wpf.Tutorial

## 1. Solution setup

- WPF project, NuGet package `Caliburn.Micro` (it brings `Caliburn.Micro.Core`).
- Folder layout used by conventions:
  - `ViewModels/` -> namespace `MyApp.ViewModels`
  - `Views/` -> namespace `MyApp.Views`
- Delete `MainWindow.xaml`; the shell window is created by Caliburn.Micro.

## 2. Naming conventions (core rule)

- `XxxViewModel` <-> `XxxView` (namespace `.ViewModels` <-> `.Views`).
- The view is located from the view model; never set `DataContext` manually.
- Control `x:Name` binds to a ViewModel member of the same name:
  - `TextBox x:Name="FirstName"` -> two-way binding to property `FirstName`.
  - `Button x:Name="Save"` -> invokes method `Save()` on click.
  - `ListBox x:Name="Categories"` -> `ItemsSource` = `Categories`, and
	`SelectedItem` binds to `SelectedCategory` (singular "Selected" + name)
	if it exists.
  - `ContentControl x:Name="ActiveItem"` -> composes the child ViewModel's view.
  - Nested properties: `x:Name="Person_Name"` binds to `Person.Name`.
- Guard properties: `CanSave` (bool property or method) enables/disables the
  `Save` action. Raise `NotifyOfPropertyChange(() => CanSave)` when its inputs change.

## 3. ShellViewModel

```csharp
using Caliburn.Micro;

namespace MyApp.ViewModels;

public class ShellViewModel : Conductor<object>
{
	private readonly IWindowManager _windowManager;

	public ShellViewModel(IWindowManager windowManager)
	{
		_windowManager = windowManager;
	}
}
```

- Base classes: `PropertyChangedBase` (INPC only), `Screen` (lifecycle:
  `OnInitializeAsync`, `OnActivateAsync`, `OnDeactivateAsync`, `CanCloseAsync`,
  `TryCloseAsync`), `Conductor<T>` (one active item),
  `Conductor<T>.Collection.OneActive` / `AllActive`.
- Properties notify through `NotifyOfPropertyChange(nameof(X))` or
  `NotifyOfPropertyChange(() => X)`; or use `Set(ref _field, value)`.

```csharp
private string _name;
public string Name
{
	get => _name;
	set
	{
		_name = value;
		NotifyOfPropertyChange(() => Name);
		NotifyOfPropertyChange(() => CanSave);
	}
}
```

## 4. ShellView

```xml
<Window x:Class="MyApp.Views.ShellView"
		xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
		xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
		xmlns:cal="http://caliburnmicro.com"
		Title="Shell" Height="450" Width="800">
	<DockPanel>
		<Menu DockPanel.Dock="Top">
			<MenuItem Header="_Edit">
				<MenuItem Header="_Categories" cal:Message.Attach="EditCategories" />
			</MenuItem>
			<MenuItem Header="_Help">
				<MenuItem Header="_About" cal:Message.Attach="About" />
			</MenuItem>
		</Menu>
		<ContentControl x:Name="ActiveItem" />
	</DockPanel>
</Window>
```

- `xmlns:cal="http://caliburnmicro.com"`.
- Use `cal:Message.Attach` when the event or control is not covered by
  conventions (e.g. `MenuItem`, or `"[Event MouseDoubleClick] = [Action Edit($dataContext)]"`).
  Special parameters: `$dataContext`, `$eventArgs`, `$source`, `$view`, `$executionContext`, `$this`.
- Code-behind contains only `InitializeComponent()`.

## 5. Bootstrapper with SimpleContainer

```csharp
using System.Windows;
using Caliburn.Micro;
using MyApp.ViewModels;

namespace MyApp;

public class Bootstrapper : BootstrapperBase
{
	private readonly SimpleContainer _container = new();

	public Bootstrapper()
	{
		Initialize();
	}

	protected override void Configure()
	{
		_container.Instance(_container);

		_container
			.Singleton<IWindowManager, WindowManager>()
			.Singleton<IEventAggregator, EventAggregator>();

		GetType().Assembly.GetTypes()
			.Where(type => type.IsClass)
			.Where(type => type.Name.EndsWith("ViewModel"))
			.ToList()
			.ForEach(viewModelType => _container.RegisterPerRequest(
				viewModelType, viewModelType.ToString(), viewModelType));
	}

	protected override async void OnStartup(object sender, StartupEventArgs e)
	{
		await DisplayRootViewForAsync<ShellViewModel>();
	}

	protected override object GetInstance(Type service, string key)
		=> _container.GetInstance(service, key);

	protected override IEnumerable<object> GetAllInstances(Type service)
		=> _container.GetAllInstances(service);

	protected override void BuildUp(object instance)
		=> _container.BuildUp(instance);
}
```

- Registration options: `Singleton<TService, TImpl>()`, `PerRequest<TService, TImpl>()`,
  `Instance(obj)`, `RegisterPerRequest(type, key, impl)`.
- Constructor injection is resolved by the container. Use `IoC.Get<T>()` only
  where injection is impractical (prefer injecting a factory or the type).

## 6. App.xaml

Remove `StartupUri` and add the bootstrapper as a resource:

```xml
<Application x:Class="MyApp.App"
			 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
			 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			 xmlns:local="clr-namespace:MyApp">
	<Application.Resources>
		<ResourceDictionary>
			<ResourceDictionary.MergedDictionaries>
				<ResourceDictionary>
					<local:Bootstrapper x:Key="Bootstrapper" />
				</ResourceDictionary>
			</ResourceDictionary.MergedDictionaries>
		</ResourceDictionary>
	</Application.Resources>
</Application>
```

## 7. Simple logging (diagnose binding/convention issues)

```csharp
public class DebugLogger : ILog
{
	private readonly Type _type;
	public DebugLogger(Type type) => _type = type;

	public void Info(string format, params object[] args) => Debug.WriteLine($"INFO [{_type.Name}] {string.Format(format, args)}");
	public void Warn(string format, params object[] args) => Debug.WriteLine($"WARN [{_type.Name}] {string.Format(format, args)}");
	public void Error(Exception exception) => Debug.WriteLine($"ERROR [{_type.Name}] {exception}");
}
```

Register in the bootstrapper constructor before `Initialize()`:

```csharp
LogManager.GetLog = type => new DebugLogger(type);
```

Use this first when a view isn't found or a convention doesn't bind.

## 8. Composing user controls (child ViewModels)

- Child: `CategoryViewModel : Screen`, view `CategoryView : UserControl`.
- Shell activates it in the `ContentControl x:Name="ActiveItem"`:

```csharp
public Task EditCategories()
	=> ActivateItemAsync(IoC.Get<CategoryViewModel>(), CancellationToken.None);
```

- Lists: use `BindableCollection<T>` (thread-safe UI notifications).

```csharp
public BindableCollection<CategoryModel> Categories { get; } = new();

private CategoryModel _selectedCategory;
public CategoryModel SelectedCategory
{
	get => _selectedCategory;
	set
	{
		_selectedCategory = value;
		NotifyOfPropertyChange(() => SelectedCategory);
		NotifyOfPropertyChange(() => CanDelete);
	}
}

public bool CanDelete => SelectedCategory != null;
public void Delete() => Categories.Remove(SelectedCategory);
```

## 9. Conditional execution (guards)

- For action `Add`, define `bool CanAdd` property (or `CanAdd(...)` method with
  the same parameters). The button is disabled automatically when false.
- Always notify `CanXxx` whenever a dependency property changes.

## 10. Dialogs with IWindowManager

```csharp
public Task About() => _windowManager.ShowDialogAsync(IoC.Get<AboutViewModel>());
```

- `ShowDialogAsync(rootModel, context = null, settings = null)` -> modal, returns `bool?`.
- `ShowWindowAsync(...)` -> modeless; `ShowPopupAsync(...)` -> popup at mouse.
- Dialog ViewModel derives from `Screen`; its view is a `Window`.
  Close with `await TryCloseAsync(true)` (OK) / `TryCloseAsync(false)` (Cancel).
- Window settings via a dictionary/ExpandoObject:

```csharp
var settings = new Dictionary<string, object>
{
	["WindowStartupLocation"] = WindowStartupLocation.CenterOwner,
	["ResizeMode"] = ResizeMode.NoResize,
	["Title"] = "About",
};
await _windowManager.ShowDialogAsync(viewModel, null, settings);
```

## Rules of thumb

- Do not set `DataContext` or write event handlers in code-behind; rely on conventions.
- Match names exactly (`x:Name` <-> property/method, `Can` + action name for guards).
- Keep ViewModels UI-agnostic; use `IWindowManager` and `IEventAggregator` for UI/cross-VM communication.
- Use the async v4 APIs; avoid obsolete sync ones (`ActivateItem`, `ShowDialog`, `TryClose`).
- When something doesn't bind, enable logging and check the namespace/name pairing first.
- For deeper study: https://caliburnmicro.com/documentation/Tutorials/WPF/ForFurtherStudy
