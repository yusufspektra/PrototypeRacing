# RuntimeUI
A flexible and easy-to-use UI runtime system for Unity. This package provides runtime UI components such as popups, toast notifications, and fullscreen loading screens, making it simple to manage and display UI elements dynamically.

# Dependencies
- Unity UI System
- Spektra Utilities

# General Usage
- Use `RuntimeUISettings` to configure global settings for UI components.
- To display a popup, toast, or loading screen, use the corresponding `RuntimeUI` methods.

# RuntimeUI Main Classes
### `RuntimeUI.Toast`
- Displays a toast notification with custom text and duration.
```csharp
RuntimeUI.Toast.Show("Hello, world!", 2.0f);
```

### `RuntimeUI.BlockScreen`
- Displays a blocking overlay to prevent user interactions while an action is in progress.
```csharp
RuntimeUI.BlockScreen.Show();
// Perform some operation...
RuntimeUI.BlockScreen.Hide();
```

### `RuntimeUI.FullScreenLoading`
- Shows a fullscreen loading animation while an operation is in progress.
```csharp
RuntimeUI.FullScreenLoading.Show("Loading... Please wait");
await SomeAsyncOperation();
RuntimeUI.FullScreenLoading.Hide();
```

### `RuntimeUI.DynamicPopup`
- Displays a popup with a title, message, and customizable buttons.
```csharp
PopupBuilder popup =
            PopupBuilder.Build(titleText,
                bodyText,
                autoCloseOnAnyActionButtonPress,
                activateCloseButton,
                closeWhenBackgroundPress);

        popup.AddActionButton("Aaaa", () => { Debug.LogError("On Click Aaaa"); });

        popup.AddActionButton("Bbbbb", () => { Debug.LogError("On Click Bbbbb"); });

        popup.SetPopupClosedCallback((closeCause) => { Debug.LogError("Closed: " + closeCause); });
```

# RuntimeUISettings
- Centralized settings for configuring UI behaviors and styles.
- Modify properties such as default durations, animations, and themes.

# Additional Features
- **Lightweight & Optimized**: Designed for minimal performance impact.
- **Easy Integration**: Plug & play without modifying Unity’s UI system.
- **Async Support**: Compatible with async/await patterns.

# Future Improvements
- Customizable UI themes.
- Additional transition effects.
- Support for dynamic UI layouts.
